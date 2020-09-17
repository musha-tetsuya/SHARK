using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// マルチバトル用WAVEグループデータ
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObject/MultiFishWaveGroupData")]
public class MultiFishWaveGroupData : ScriptableObject
{
    /// <summary>
    /// バトルWAVEデータリスト
    /// </summary>
    [SerializeField, HideInInspector]
    public List<FishWaveData.WithDelay> waveDatas = null;
    /// <summary>
    /// ランダムLowレートルート
    /// </summary>
    [SerializeField]
    public RandomFishRouteData randomLowRoute = null;
    /// <summary>
    /// ランダムMidレートルート
    /// </summary>
    [SerializeField]
    public RandomFishRouteData randomMidRoute = null;
    /// <summary>
    /// ランダムHighレートルート
    /// </summary>
    [SerializeField]
    public RandomFishRouteData randomHighRoute = null;
    /// <summary>
    /// ランダムSPレートルート
    /// </summary>
    [SerializeField]
    public RandomFishRouteData randomSpRoute = null;

#if UNITY_EDITOR
    [CustomEditor(typeof(MultiFishWaveGroupData))]
    private class MyInspector : Editor
    {
        /// <summary>
        /// バトルWAVEデータリスト
        /// </summary>
        private SimpleReorderableList waveDatas = null;

        /// <summary>
        /// OnEnable
        /// </summary>
        private void OnEnable()
        {
            this.waveDatas = new SimpleReorderableList(this.serializedObject.FindProperty("waveDatas"), typeof(FishWaveData.WithDelay));
        }

        /// <summary>
        /// OnInspectorGUI
        /// </summary>
        public override void OnInspectorGUI()
        {
            this.serializedObject.Update();

            base.OnInspectorGUI();

            EditorGUILayout.LabelField("バトルWAVEデータリスト");

            this.waveDatas.DoLayoutList();

            this.serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}
