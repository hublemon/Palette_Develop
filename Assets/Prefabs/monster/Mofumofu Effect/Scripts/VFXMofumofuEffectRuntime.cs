using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

namespace MofumofuEffect
{
    public class VFXMofumofuEffectRuntime : MonoBehaviour
    {
        VisualEffect effect;
        SkinnedMeshRenderer skinned;
        Mesh mesh;

        private void Start()
        {
            effect = GetComponent<VisualEffect>();
            skinned = effect.GetSkinnedMeshRenderer("Skinned Mesh Renderer");
            mesh = effect.GetMesh("Mesh Filter");

            CreatePhysTex();
        }

        private void LateUpdate()
        {
            RefreshPhysTex();
        }

        [Header("Physics")]
        [Tooltip("The texture's size which used to save physics info.")]
        public int size = 16;

        [Tooltip("To detect a small swing, needs to used a bigger multiplier.")]
        public float multipier = .1f;

        [Tooltip("The spring's scale.")]
        [Range(0f,1f)]
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

            //赋值给effect
            effect.SetTexture("Physics Texture", physTex);
            effect.SetBool("Use Physics Texture", true);

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
            if (mesh != null)
            {
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
                Color color = PackNormal(vels[i]);
                color.a = vels[i].magnitude;
                physTex.SetPixel(pixels[i].x, pixels[i].y, color);
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