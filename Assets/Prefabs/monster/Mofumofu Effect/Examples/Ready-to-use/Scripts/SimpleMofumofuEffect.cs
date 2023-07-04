using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace MofumofuEffect
{
    public class SimpleMofumofuEffect : MonoBehaviour
    {
        UniversalRendererData URPData;

        [Tooltip("The Name of the Renderer Feature used in Universal Renderer Data.")]
        public string featureName = "";

        [Tooltip("Whether to create on awake?")]
        public bool createOnAwake = true;

        public MofumofuEffectFeature.Settings settings;

        SkinnedMeshRenderer skinned;
        MeshFilter filter;

        private void Start()
        {
            //获得urpdata
            URPData = GetURPData();

            //如果在开始时创建
            if (createOnAwake) CreateFeature();

            //尝试获得组件
            TryGetComponent(out skinned);
            TryGetComponent(out filter);

            //创建velocity texture
            CreatePhysTex();
        }

        private void LateUpdate()
        {
            //刷新velocity texture
            RefreshPhysTex();

            //刷新feature
            RefreshFeature(URPData, FindFeature());
        }

        private void OnApplicationQuit()
        {
            //删除自己
            MofumofuEffectFeature feature = FindFeature();
            if (feature != null && URPData.rendererFeatures.Contains(feature))
            {
                URPData.rendererFeatures.Remove(feature);
                URPData.SetDirty();
            }
        }

        UniversalRendererData GetURPData()
        {
            UniversalRenderPipelineAsset URPAsset = (UniversalRenderPipelineAsset)QualitySettings.renderPipeline;
            FieldInfo propertyInfo = URPAsset.GetType().GetField("m_RendererDataList", BindingFlags.Instance | BindingFlags.NonPublic);
            UniversalRendererData URPData = (UniversalRendererData)(((ScriptableRendererData[])propertyInfo?.GetValue(URPAsset))?[0]);

            return URPData;
        }

        public void CreateFeature()
        {
            //如果已经创建了，同名，不予执行
            if (FindFeature() != null)
            {
                Debug.LogError("Feature Name is Repeated!");
                return;
            }

            //当featureName没有赋值的时候不予执行
            if (featureName == "")
            {
                Debug.LogError("Feature Name can't be null!");
                return;
            }

            //当sortingLayerIndex超出范围时候不予执行
            if (SortingLayer.layers.Length <= settings.sortingLayerIndex || settings.sortingLayerIndex < 0)
            {
                Debug.LogError("Sorting Layer Index is out of range!");
                return;
            }

            //新建一个instance
            MofumofuEffectFeature feature = ScriptableObject.CreateInstance<MofumofuEffectFeature>();

            //命名
            feature.name = featureName;

            //设置
            feature.settings = settings;

            //添加进去
            URPData.rendererFeatures.Add(feature);

            //去除坏掉的render feature
            bool isValid;
            do
            {
                isValid = true;

                for (int i = 0; i < URPData.rendererFeatures.Count; i++)
                {
                    if (URPData.rendererFeatures[i] == null)
                    {
                        URPData.rendererFeatures.RemoveAt(i);
                        isValid = false;
                        break;
                    }
                }
            } while (!isValid);

            URPData.SetDirty();

            //刷新feature
            RefreshFeature(URPData, feature);
        }

        public MofumofuEffectFeature FindFeature()
        {
            //当urpdata没有赋值的时候不予执行
            if (URPData == null)
            {
                Debug.LogError("URP Data must be Assigned!");
                return null;
            }
            URPData.SetDirty();

            //遍历
            MofumofuEffectFeature outPut = null;
            foreach (ScriptableRendererFeature feature in URPData.rendererFeatures)
            {
                if (feature != null && feature is MofumofuEffectFeature && feature.name == featureName)
                {
                    outPut = feature as MofumofuEffectFeature;
                    break;
                }
            }
            return outPut;
        }

        public void RefreshFeature(UniversalRendererData URPData, MofumofuEffectFeature feature)
        {
            //当URPData没有赋值的时候不予执行
            if (URPData == null)
            {
                Debug.LogError("Can't find Universal Renderer Data!");
                return;
            }

            //当feature没有赋值的时候不予执行
            if (feature == null)
            {
                //Debug.Log("Can't find Effect Feature!");
                return;
            }

            //当sortingLayerIndex超出范围时候不予执行
            if (SortingLayer.layers.Length <= settings.sortingLayerIndex || settings.sortingLayerIndex < 0)
            {
                Debug.LogError("Sorting Layer Index is out of range!");
                return;
            }

            //刷新
            feature.settings = settings;
            feature.Create();
            URPData.SetDirty();

            //着色器
            GetComponent<Renderer>().sortingLayerID = SortingLayer.layers[settings.sortingLayerIndex].id;
        }

        [Header("Physics")]
        [Tooltip("The texture's size which used to save physics info.")]
        public int size = 16;

        [Tooltip("To detect a small swing, needs to used a bigger multiplier.")]
        public float multipier = 1f;

        [Tooltip("The spring's scale.")]
        [Range(0f, 1f)]
        public float spring = .08f;

        [Tooltip("The drag's scale.")]
        [Range(0f, 1f)]
        public float drag = .15f;

        [Tooltip("The fur's default direction.")]
        public Vector3 gravity = Vector3.down * 0.098f;

        Texture2D physTex;

        int count;
        List<int> indexes = new();
        List<Vector3> vertices = new();
        List<Vector2Int> pixels = new();
        Vector3[] newPos;
        Vector3[] oldPos;
        Vector3[] forces;
        Vector3[] vels;

        void CreatePhysTex()
        {
            //创建physics texture
            physTex = new(size, size, TextureFormat.RGBA64, false, true);
            List<Color> colors = new();
            for (int i = 0; i < size * size; i++)
            {
                colors.Add(PackNormal(Vector3.zero));
            }
            physTex.SetPixels(colors.ToArray());

            //赋值给feature的settings
            settings.physTex = physTex;

            //数据
            if (skinned != null)
            {
                for (int i = 0; i < skinned.sharedMesh.vertexCount; i++)
                {
                    if (skinned.sharedMesh.colors.Length == 0 || skinned.sharedMesh.colors[i].r != 0f)
                    {
                        Vector2Int pixel = new(Mathf.FloorToInt(size * skinned.sharedMesh.uv[i].x), Mathf.FloorToInt(size * skinned.sharedMesh.uv[i].y));
                        if (!pixels.Contains(pixel))
                        {
                            count++;
                            indexes.Add(i);
                            pixels.Add(pixel);
                        }
                    }
                }
                vertices = new List<Vector3>(count);
            }
            else
            if (filter != null)
            {
                Mesh mesh = GetComponent<MeshFilter>().sharedMesh;
                for (int i = 0; i < mesh.vertexCount; i++)
                {
                    if (mesh.colors.Length == 0 || mesh.colors[i].r != 0f)
                    {
                        Vector2Int pixel = new(Mathf.FloorToInt(size * mesh.uv[i].x), Mathf.FloorToInt(size * mesh.uv[i].y));
                        if (!pixels.Contains(pixel))
                        {
                            count++;
                            indexes.Add(i);
                            pixels.Add(pixel);
                            vertices.Add(mesh.vertices[i]);
                        }
                    }
                }
            }
            newPos = new Vector3[count];
            oldPos = new Vector3[count];
            forces = new Vector3[count];
            vels = new Vector3[count];
        }

        void RefreshPhysTex()
        {
            Transform root = transform;
            if (skinned != null)
            {
                List<Vector3> v = ReadVertices(skinned);
                if (v != null && v.Count == count) vertices = v;
                root = skinned.rootBone;
            }

            for (int i = 0; i < count; i++)
            {
                //顶点变换
                if (i >= 0 && i < vertices.Count) newPos[i] = root.localToWorldMatrix.MultiplyPoint(vertices[i]);

                Vector3 delta = root.lossyScale.x * multipier * (oldPos[i] - newPos[i]);
                oldPos[i] = newPos[i];

                //风场
                Vector3 wind = new();
                if (DirectionalWindZone.instance != null) wind = DirectionalWindZone.instance.SampleWindZone(newPos[i]);

                //汇总计算
                Vector3 force = delta + gravity + wind;
                forces[i] += force;
                vels[i] += forces[i] * spring;
                forces[i] -= vels[i];
                vels[i] *= 1f - drag;
                vels[i] = Vector3.ClampMagnitude(vels[i], 1f);

                //赋予颜色
                physTex.SetPixel(pixels[i].x, pixels[i].y, PackNormal(vels[i]));
            }

            physTex.Apply();
        }

        List<Vector3> ReadVertices(SkinnedMeshRenderer skinned)
        {
            GraphicsBuffer buffer = skinned.GetVertexBuffer();

            if (buffer != null)
            {
                int size = skinned.sharedMesh.vertexCount * buffer.stride;

                float[] data = new float[size / sizeof(float)];

                buffer.GetData(data);

                List<Vector3> list = new();

                for (int i = 0; i < count; i++)
                {
                    list.Add(skinned.worldToLocalMatrix * new Vector3(data[indexes[i] * buffer.stride / sizeof(float)]
                        , data[indexes[i] * buffer.stride / sizeof(float) + 1]
                        , data[indexes[i] * buffer.stride / sizeof(float) + 2]));
                }

                buffer.Release();

                return list;
            }
            else
            {
                return null;
            }
        }

        public Color PackNormal(Vector3 normal)
        {
            Color color = new();

            color.r = (normal.x + 1f) / 2f;
            color.g = (normal.y + 1f) / 2f;
            color.b = (normal.z + 1f) / 2f;

            color.a = 1f;

            return color;
        }
    }
}
