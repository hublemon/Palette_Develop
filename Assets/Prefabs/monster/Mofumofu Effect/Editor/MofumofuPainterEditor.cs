using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace MofumofuEffect
{
    [CustomEditor(typeof(MofumofuPainter))]
    [CanEditMultipleObjects]
    public class MofumofuPainterEditor : Editor
    {
        MofumofuPainter painter;
        MofumofuEffect effect;
        Mesh mesh;

        enum PaintType
        {
            Length, Direction
        }

        PaintType paintType = PaintType.Length;
        bool isPainting;
        float brushSize = 16f;
        float brushStronger = 0.5f;
        float brushSpace = 0.1f;
        int selBrush;
        Texture2D[] brushes;

        private void OnSceneGUI()
        {
            //绘制细节
            if (isPainting)
            {
                PaintDetail();

                //刷新
                effect.RefreshFeature(effect.URPData, effect.FindFeature());
            }
        }

        public override void OnInspectorGUI()
        {
            painter = target as MofumofuPainter;
            effect = painter.GetComponent<MofumofuEffect>();
            mesh = painter.GetComponent<MeshFilter>().sharedMesh;

            base.OnInspectorGUI();

            if (CheckSettings())
            {
                //编辑模式开关
                GUIStyle boolBtn = new GUIStyle(GUI.skin.GetStyle("Button"));
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                isPainting = GUILayout.Toggle(isPainting, EditorGUIUtility.IconContent("EditCollider")
                    , boolBtn, GUILayout.Width(35), GUILayout.Height(25));
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                //笔刷设置
                paintType = (PaintType)EditorGUILayout.EnumPopup("Paint Type", paintType);
                brushSize = EditorGUILayout.Slider("Brush Size", brushSize, 1f, 36f);
                brushStronger = EditorGUILayout.Slider("Brush Stronger", brushStronger, 0, 1f);
                brushSpace = EditorGUILayout.Slider("Brush Space", brushSpace, 0, 1f);

                //选择笔刷
                brushes = GetBrushes();
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.BeginHorizontal("box", GUILayout.Width(318f));
                selBrush = GUILayout.SelectionGrid(selBrush, brushes, 9, "gridlist", GUILayout.Width(340), GUILayout.Height(35));
                GUILayout.EndHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
        }

        enum TextureSize
        {
            _512, _1024, _2048, _4096
        }

        TextureSize textureSize = TextureSize._512;

        /// <summary>
        /// 检测是否能够开始绘制细节
        /// </summary>
        bool CheckSettings()
        {
            bool isValid = false;


            if (effect.settings.material != null && effect.settings.material.shader == Shader.Find("Shader Graphs/Mofumofu Effect Shader Graph"))
            {
                Texture2D mainTex = effect.settings.mainTex;
                Texture2D detailTex = effect.settings.detailTex;

                if (detailTex == null)
                {
                    EditorGUILayout.HelpBox("The property in Mofumofu Effect's settings doesn't have a Detail Texture."
                        , MessageType.Error);

                    textureSize = (TextureSize)EditorGUILayout.EnumPopup("Texture Size", textureSize);

                    if (GUILayout.Button("Create Detail Texture"))
                    {
                        int size = 512;

                        switch (textureSize)
                        {
                            case TextureSize._512: break;
                            case TextureSize._1024: size = 1024; break;
                            case TextureSize._2048: size = 2048; break;
                            case TextureSize._4096: size = 4096; break;
                        }

                        if (mainTex != null) size = mainTex.width;

                        effect.settings.detailTex = CreateDetailTex(size);
                    }
                }
                else
                {
                    isValid = true;
                }
            }
            else
            {
                EditorGUILayout.HelpBox("The material in Mofumofu Effect's settings needs to use: Shader Graphs/Mofumofu Effect Shader Graph."
                    , MessageType.Error);
            }

            return isValid;
        }

        /// <summary>
        /// 创建细节贴图
        /// </summary>
        Texture2D CreateDetailTex(int size)
        {
            //准备
            string detailTexName = "";
            string texFolder = "Assets/Mofumofu Effect/Textures/";

            //创建一个新的detail texture
            Texture2D detailTex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color[] colors = new Color[size * size];
            for (int i = 0; i < colors.Length; i++)
            {
                Color color = effect.PackNormal(new());
                color.a = 0f;
                colors[i] = color;
            }
            detailTex.SetPixels(colors);

            //判断是否重名
            if (!File.Exists(texFolder + effect.name + ".Png"))
            {
                detailTexName = effect.name;
            }
            else
            {
                bool isValid = false;
                for (int num = 1; !isValid; num++)
                {
                    if (!File.Exists(texFolder + effect.name + " " + num + ".Png"))
                    {
                        detailTexName = effect.name + " " + num;
                        isValid = true;
                    }
                }
            }

            //保存
            string path = texFolder + detailTexName + ".png";
            byte[] bytes = detailTex.EncodeToPNG();
            File.WriteAllBytes(path, bytes);

            //导入
            AssetDatabase.ImportAsset(path);
            //设置
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            importer.sRGBTexture = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.isReadable = true;
            //刷新
            AssetDatabase.ImportAsset(path);

            //导入
            detailTex = (Texture2D)AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D));

            return detailTex;
        }

        /// <summary>
        /// 获取笔刷
        /// </summary>
        Texture2D[] GetBrushes()
        {
            string brushFolder = "Assets/Mofumofu Effect/Editor/Brushes/";
            List<Texture2D> brushes = new();

            Texture2D brush;
            int num = 0;
            do
            {
                brush = (Texture2D)AssetDatabase.LoadAssetAtPath(brushFolder + "Brush" + num + ".png", typeof(Texture2D));

                if (brush)
                {
                    brushes.Add(brush);
                }

                num++;
            } while (brush);

            return brushes.ToArray();
        }

        bool toggleF;
        Vector3 lastPos;
        Vector3 lastNormal;
        Vector2 pixelUV;
        /// <summary>
        /// 绘制细节
        /// </summary>
        void PaintDetail()
        {
            //笔刷在模型上的正交大小
            float orthographicSize = brushSize * effect.transform.localScale.x * mesh.bounds.size.x / 200f;

            //获取细节贴图
            Texture2D detailTex = effect.settings.detailTex;

            //笔刷在模型材质上的大小
            int brushSizeOnModelTex = (int)(brushSize * detailTex.width / 100f);

            //从鼠标发射射线
            Event e = Event.current;
            HandleUtility.AddDefaultControl(0);
            if (Physics.Raycast(HandleUtility.GUIPointToWorldRay(e.mousePosition)
                , out RaycastHit hit, float.PositiveInfinity, effect.settings.layerMask))
            {
                //空挡
                if (Vector3.Distance(hit.point, lastPos) >= brushSpace)
                {
                    lastPos = hit.point;
                    lastNormal = hit.normal;

                    //UV坐标
                    pixelUV = hit.textureCoord;
                }

                //根据画笔大小，在鼠标位置显示一个圆形
                Handles.color = new Color(1f, 1f, 0f, 1f);
                Handles.DrawWireDisc(lastPos, lastNormal, orthographicSize);

                //鼠标点下或按下并拖动进行绘制
                if ((e.type == EventType.MouseDrag && !e.alt && !e.control && !e.shift && e.button == 0)
                    || (e.type == EventType.MouseDown && !e.alt && !e.control && !e.shift && e.button == 0 && !toggleF))

                {
                    toggleF = true;

                    //根据上一帧位置得到速度
                    Vector3 vel = new();
                    if (Physics.Raycast(HandleUtility.GUIPointToWorldRay(e.mousePosition - e.delta)
                        , out RaycastHit lastHit, float.PositiveInfinity, effect.settings.layerMask))
                    {
                        vel = hit.point - lastHit.point;
                    }

                    //绘制的颜色
                    if (vel.magnitude != 0f) vel.Normalize();
                    Color velColor = effect.PackNormal(vel);


                    //计算笔刷所覆盖范围
                    int pixelX = (int)(pixelUV.x * detailTex.width);
                    int pixelY = (int)(pixelUV.y * detailTex.height);
                    int x = Mathf.Clamp(pixelX - brushSizeOnModelTex / 2, 0, detailTex.width - 1);
                    int y = Mathf.Clamp(pixelY - brushSizeOnModelTex / 2, 0, detailTex.height - 1);
                    int width = Mathf.Clamp(pixelX + brushSizeOnModelTex / 2, 0, detailTex.width - x);
                    int height = Mathf.Clamp(pixelY + brushSizeOnModelTex / 2, 0, detailTex.height - y);

                    //获取细节贴图在覆盖范围内的颜色
                    Color[] colors = detailTex.GetPixels(x, y, width, height);

                    //根据笔刷贴图计算透明度
                    Texture2D brush = brushes[selBrush];
                    float[] alpha = new float[brushSizeOnModelTex * brushSizeOnModelTex];
                    for (int i = 0; i < brushSizeOnModelTex; i++)
                    {
                        for (int j = 0; j < brushSizeOnModelTex; j++)
                        {
                            alpha[j * brushSizeOnModelTex + i] = brush.GetPixelBilinear((float)i / brushSizeOnModelTex, (float)j / brushSizeOnModelTex).a;
                        }
                    }

                    //计算绘制后的颜色
                    for (int i = 0; i < height; i++)
                    {
                        for (int j = 0; j < width; j++)
                        {
                            int index = (i * width) + j;
                            float brushAlpha = alpha[Mathf.Clamp(y + i - (pixelY - brushSizeOnModelTex / 2), 0, brushSizeOnModelTex - 1) * brushSizeOnModelTex
                                + Mathf.Clamp(x + j - (pixelX - brushSizeOnModelTex / 2), 0, brushSizeOnModelTex - 1)];

                            switch (paintType)
                            {
                                case PaintType.Length:
                                    colors[index].a = Mathf.Lerp(colors[index].a, brushStronger, brushAlpha);
                                    break;
                                case PaintType.Direction:
                                    if(velColor!= effect.PackNormal(Vector3.zero))
                                    {
                                        Color color = velColor;
                                        color.a = colors[index].a;
                                        colors[index] = Color.Lerp(colors[index], color, brushAlpha * brushStronger);
                                    }
                                    break;
                            }
                        }
                    }
                    //方便撤销
                    Undo.RegisterCompleteObjectUndo(detailTex, "DetailPainter");

                    //保存
                    detailTex.SetPixels(x, y, width, height, colors);
                    detailTex.Apply();

                }
                else
                if (e.type == EventType.MouseUp && !e.alt && e.button == 0 && toggleF)
                {
                    toggleF = false;

                    //绘制结束保存
                    string path = AssetDatabase.GetAssetPath(detailTex);
                    byte[] bytes = detailTex.EncodeToPNG();
                    File.WriteAllBytes(path, bytes);
                    //AssetDatabase.ImportAsset(path);
                }
            }
        }
    }
}
