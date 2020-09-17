using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// ランダム魚回遊ルートデータ
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObject/RandomFishRouteData")]
public class RandomFishRouteData : ScriptableObject
{
    /// <summary>
    /// ルートリスト
    /// </summary>
    [SerializeField, HideInInspector]
    public List<FishRouteData.WithProbability> routeDatas = null;
    /// <summary>
    /// 魚データリスト
    /// </summary>
    [SerializeField, HideInInspector]
    public List<FishData> fishDatas = null;
    /// <summary>
    /// 生成開始までの遅延
    /// </summary>
    [SerializeField, Header("生成開始までの遅延")]
    public float delay = 0f;
    /// <summary>
    /// 生成イベントデータ
    /// </summary>
    [SerializeField, Header("生成イベントデータ")]
    public AnimationClip createEvent = null;

#if UNITY_EDITOR
    [CustomEditor(typeof(RandomFishRouteData))]
    private class MyInspector : Editor
    {
        /// <summary>
        /// 回遊ルートリスト
        /// </summary>
        private SimpleReorderableList routeDatas = null;

        /// <summary>
        /// 魚リスト
        /// </summary>
        private SimpleReorderableList fishDatas = null;

        /// <summary>
        /// OnEnable
        /// </summary>
        private void OnEnable()
        {
            this.routeDatas = new SimpleReorderableList(this.serializedObject.FindProperty("routeDatas"), typeof(FishRouteData.WithProbability));
            this.fishDatas = new SimpleReorderableList(this.serializedObject.FindProperty("fishDatas"), typeof(FishData));
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

            EditorGUILayout.LabelField("魚リスト");

            this.fishDatas.DoLayoutList();

            this.serializedObject.ApplyModifiedProperties();
        }
    }
#endif

    /// <summary>
    /// 魚データ
    /// </summary>
    [Serializable]
    public class FishData
    {
        /// <summary>
        /// 魚ID
        /// </summary>
        [SerializeField]
        public uint fishId = 100001;
        /// <summary>
        /// ユニット数1～5
        /// </summary>
        [SerializeField]
        public uint unitSizeMax = 1;
        /// <summary>
        /// ユニットの広がり
        /// </summary>
        [SerializeField]
        public float unitSpread = 1f;
    }
}
