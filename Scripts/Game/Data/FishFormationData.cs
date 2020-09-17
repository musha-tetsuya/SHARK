using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 魚編隊データ
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObject/FishFormationData")]
public class FishFormationData : ScriptableObject
{
    /// <summary>
    /// 回遊ルートリスト
    /// </summary>
    [SerializeField, HideInInspector]
    public List<FishRouteData.WithDelay> routeDatas = null;

    /// <summary>
    /// イベントデータリスト
    /// </summary>
    [SerializeField, HideInInspector]
    public List<FishFormationEventData> eventDatas = null;

    /// <summary>
    /// Delay付き魚編隊データ
    /// </summary>
    [Serializable]
    public class WithDelay
    {
        /// <summary>
        /// 魚編隊データ
        /// </summary>
        [SerializeField]
        public FishFormationData data = null;

        /// <summary>
        /// Delay
        /// </summary>
        [SerializeField]
        public float delay = 0f;
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(FishFormationData))]
    private class MyInspector : Editor
    {
        /// <summary>
        /// 回遊ルートリスト
        /// </summary>
        private SimpleReorderableList routeDatas = null;

        /// <summary>
        /// イベントリスト
        /// </summary>
        private SimpleReorderableList eventDatas = null;

        /// <summary>
        /// OnEnable
        /// </summary>
        private void OnEnable()
        {
            this.routeDatas = new SimpleReorderableList(this.serializedObject.FindProperty("routeDatas"), typeof(FishRouteData.WithDelay));
            this.eventDatas = new SimpleReorderableList(this.serializedObject.FindProperty("eventDatas"), typeof(FishFormationEventData));
        }

        /// <summary>
        /// OnInspectorGUI
        /// </summary>
        public override void OnInspectorGUI()
        {
            this.serializedObject.Update();

            base.OnInspectorGUI();

            EditorGUILayout.LabelField("回遊ルートリスト");

            this.routeDatas.DoLayoutList();

            EditorGUILayout.LabelField("イベントリスト");

            this.eventDatas.DoLayoutList();

            this.serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}
