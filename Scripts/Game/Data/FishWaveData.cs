using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// バトルWAVEデータ
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObject/FishWaveData")]
public class FishWaveData : ScriptableObject
{
    /// <summary>
    /// 編隊データリスト
    /// </summary>
    [SerializeField, HideInInspector]
    public List<FishFormationData.WithDelay> formationDatas = null;

#if UNITY_EDITOR
    [CustomEditor(typeof(FishWaveData))]
    private class MyInspector : Editor
    {
        /// <summary>
        /// 編隊データリスト
        /// </summary>
        private SimpleReorderableList formationDatas = null;

        /// <summary>
        /// OnEnable
        /// </summary>
        private void OnEnable()
        {
            this.formationDatas = new SimpleReorderableList(this.serializedObject.FindProperty("formationDatas"), typeof(FishFormationData.WithDelay));
        }

        /// <summary>
        /// OnInspectorGUI
        /// </summary>
        public override void OnInspectorGUI()
        {
            this.serializedObject.Update();

            base.OnInspectorGUI();

            EditorGUILayout.LabelField("編隊データリスト");

            this.formationDatas.DoLayoutList();

            this.serializedObject.ApplyModifiedProperties();
        }
    }
#endif

    /// <summary>
    /// Delay付きWaveデータ
    /// </summary>
    [Serializable]
    public class WithDelay
    {
        /// <summary>
        /// Waveデータ
        /// </summary>
        [SerializeField]
        public FishWaveData data = null;

        /// <summary>
        /// Delay
        /// </summary>
        [SerializeField]
        public float delay = 0f;
    }
}
