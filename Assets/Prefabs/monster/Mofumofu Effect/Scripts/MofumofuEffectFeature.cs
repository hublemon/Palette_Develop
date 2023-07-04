using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace MofumofuEffect
{
    public class MofumofuEffectFeature : ScriptableRendererFeature
    {
        [System.Serializable]
        public class Settings
        {
            [Header("Mofumofu Effect's Settings")]

            [Tooltip("The layermask of the gameobject this componenet is attached to")]
            public LayerMask layerMask;

            [Tooltip("Use the sorting layer index to differentiate gameobject")]
            public short sortingLayerIndex = 1;

            [Tooltip("The Mofumofu Effect material")]
            public Material material = null;

            [Tooltip("The mat-index which would use Mofumofu Effect. (Usually setting to 0.)")]
            public int materialPassIndex = 0;

            [Tooltip("The count of the fur. (The larger, the greater, the more expansive.)")]
            public int layerCount = 10;

            [Tooltip("The fur's length. (Shound be larger at horizon with 1, another words, this curve describe the fur layer's position)")]
            public AnimationCurve length = AnimationCurve.Linear(0, 0, 1, 0.2f);

            [Tooltip("The fur's thickness. (This curve describe the fur's thickness along the layer count)")]
            public AnimationCurve thickness = AnimationCurve.Linear(0, 1, 1, 0);

            [Tooltip("The fur's softness. (The larger, the easier to swing.)")]
            public AnimationCurve softness = AnimationCurve.Linear(0, 0, 1, 1);

            [Tooltip("Should the effect use the vertex color's red channel as fur's length?")]
            public bool useVertexColor = true;

            [Tooltip("Should the effect use a voronoi?")]
            public bool useVoronoi = true;

            [Tooltip("The voronoi's density.")]
            public float density = 100f;

            [Tooltip("The strength used when blend normal with alpha.")]
            [Range(0f, 0.05f)]
            public float normalStrength = 0.01f;

            public Texture2D detailTex;


            [System.NonSerialized] public Texture2D physTex = null;

            [Header("Material's Common Settings")]
            public Texture2D mainTex;
            public AnimationCurve AO = AnimationCurve.Linear(0, 0, 1, 1);
            public AnimationCurve smoothness = AnimationCurve.Linear(0, 0, 1, 0);
            public AnimationCurve metallic = AnimationCurve.Linear(0, 0, 1, 0);
            public AnimationCurve alpha = AnimationCurve.Linear(0, 1f, 1, 0.5f);
        }

        public Settings settings = new();

        List<MofumofuEffectPass> passes = new();

        public override void Create()
        {
            passes.Clear();

            for (int i = 0; i < settings.layerCount; i++)
            {
                MofumofuEffectPass pass = new("Mofumofu Effect", settings, i + 1);
                passes.Add(pass);
            }
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            for (int i = 0; i < passes.Count; i++)
            {
                renderer.EnqueuePass(passes[i]);
            }
        }
    }
    class MofumofuEffectPass : ScriptableRenderPass
    {
        private FilteringSettings m_FilteringSettings;

        public Material overrideMaterial { get; set; }
        public int overrideMaterialPassIndex { get; set; }

        public List<ShaderTagId> shaderTagIdList = new()
        {
            new ShaderTagId("UniversalForward")
        };
        public MofumofuEffectPass(string profilerTag, MofumofuEffectFeature.Settings settings, int currentCount)
        {
            profilingSampler = new ProfilingSampler(profilerTag);

            if (settings.material != null)
            {
                overrideMaterial = new Material(settings.material);

                overrideMaterial.SetFloat("_Density", settings.density);
                overrideMaterial.SetFloat("_Length", settings.length.Evaluate((float)currentCount / settings.layerCount));
                overrideMaterial.SetFloat("_Thickness", settings.thickness.Evaluate((float)currentCount / settings.layerCount));
                overrideMaterial.SetFloat("_Softness", settings.softness.Evaluate((float)currentCount / settings.layerCount));
                overrideMaterial.SetTexture("_PhysTex", settings.physTex);

                overrideMaterial.SetTexture("_MainTex", settings.mainTex);
                overrideMaterial.SetFloat("_AO", settings.AO.Evaluate((float)currentCount / settings.layerCount));
                overrideMaterial.SetFloat("_Smoothness", settings.smoothness.Evaluate((float)currentCount / settings.layerCount));
                overrideMaterial.SetFloat("_Metallic", settings.metallic.Evaluate((float)currentCount / settings.layerCount));
                overrideMaterial.SetFloat("_Alpha", settings.alpha.Evaluate((float)currentCount / settings.layerCount));

                if (settings.detailTex != null) overrideMaterial.EnableKeyword("_USE_DETAIL_TEX"); else overrideMaterial.DisableKeyword("_USE_DETAIL_TEX");
                overrideMaterial.SetTexture("_DetailTex", settings.detailTex);
                overrideMaterial.SetFloat("_NormalStrength", settings.normalStrength);


                if (settings.useVertexColor) overrideMaterial.EnableKeyword("_USE_VERTEX_COLOR"); else overrideMaterial.DisableKeyword("_USE_VERTEX_COLOR");

                if (settings.useVoronoi) overrideMaterial.EnableKeyword("_USE_VORONOI"); else overrideMaterial.DisableKeyword("_USE_VORONOI");
            }

            overrideMaterialPassIndex = settings.materialPassIndex;

            renderPassEvent = RenderPassEvent.BeforeRenderingTransparents;

            m_FilteringSettings = new FilteringSettings(RenderQueueRange.all, settings.layerMask);
            m_FilteringSettings.sortingLayerRange = new SortingLayerRange(settings.sortingLayerIndex, settings.sortingLayerIndex);
        }
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (overrideMaterial == null) return;

            //SortingCriteria sortingCriteria = renderingData.cameraData.defaultOpaqueSortFlags;
            SortingCriteria sortingCriteria = SortingCriteria.CommonTransparent;

            var drawingSettings = CreateDrawingSettings(shaderTagIdList, ref renderingData, sortingCriteria);
            drawingSettings.overrideMaterial = overrideMaterial;
            drawingSettings.overrideMaterialPassIndex = overrideMaterialPassIndex;

            RenderStateBlock stateBlock = new(RenderStateMask.Depth);
            stateBlock.depthState = new(true, CompareFunction.Less);

            context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref m_FilteringSettings, ref stateBlock);
        }
    }
}