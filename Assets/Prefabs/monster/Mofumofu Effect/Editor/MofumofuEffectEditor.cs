using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace MofumofuEffect
{
    [CustomEditor(typeof(MofumofuEffect))]
    public class MofumofuEffectEditor : Editor
    {
        MofumofuEffect effect;

        public override void OnInspectorGUI()
        {
            effect = target as MofumofuEffect;

            base.OnInspectorGUI();

            //获取feature
            MofumofuEffectFeature feature = effect.FindFeature();

            //创建feature
            if ((!Application.isPlaying || !effect.createOnAwake) && feature == null && GUILayout.Button("Preview"))
            {
                effect.CreateFeature();
            }
        }
    }
}