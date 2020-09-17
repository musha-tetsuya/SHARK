using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// BGMクリップデータ
/// </summary>
public class BgmClipData : ScriptableObject
{
    /// <summary>
    /// BGMクリップ
    /// </summary>
    [SerializeField, HideInInspector]
    public AudioClip clip = null;
    /// <summary>
    /// 音量
    /// </summary>
    [SerializeField, HideInInspector]
    public float volume = 1f;
    /// <summary>
    /// ループ開始位置正規化時間
    /// </summary>
    [SerializeField, HideInInspector]
    public float loopStartNormalizedTime = 0f;
    /// <summary>
    /// ループ終了位置正規化時間
    /// </summary>
    [SerializeField, HideInInspector]
    public float loopEndNormalizedTime = 1f;

#if UNITY_EDITOR
    /// <summary>
    /// BGMクリップデータ作成
    /// </summary>
    [MenuItem("CONTEXT/AudioClip/CreateBgmClipData")]
    private static void CreateBgmClipData(MenuCommand menuCommand)
    {
        var clip = menuCommand.context as AudioClip;
        var path = Path.ChangeExtension(AssetDatabase.GetAssetPath(clip), "asset");
        var data = ScriptableObject.CreateInstance<BgmClipData>();
        data.clip = clip;
        AssetDatabase.CreateAsset(data, path);
    }

    /// <summary>
    /// OnInspectorGUI
    /// </summary>
    public void OnInspectorGUI()
    {
        //BGMクリップ
        this.clip = EditorGUILayout.ObjectField("Clip", this.clip, typeof(AudioClip), false) as AudioClip;

        //音量
        this.volume = EditorGUILayout.Slider("Volume", this.volume, 0f, 1f);

        if (this.clip != null)
        {
            //BGM長さ
            float length = this.clip.length;

            //ループ開始位置
            float loopStartTime = Mathf.Lerp(0f, length, this.loopStartNormalizedTime);
            loopStartTime = EditorGUILayout.Slider("ループ開始位置", loopStartTime, 0f, length);
            this.loopStartNormalizedTime = Mathf.InverseLerp(0f, length, loopStartTime);

            //ループ終了位置
            float loopEndTime = Mathf.Lerp(0f, length, this.loopEndNormalizedTime);
            loopEndTime = EditorGUILayout.Slider("ループ終了位置", loopEndTime, 0f, length);
            this.loopEndNormalizedTime = Mathf.InverseLerp(0f, length, loopEndTime);
        }
    }

    /// <summary>
    /// カスタムインスペクター
    /// </summary>
    [CustomEditor(typeof(BgmClipData))]
    private class MyInspector : Editor
    {
        /// <summary>
        /// target
        /// </summary>
        private new BgmClipData target => base.target as BgmClipData;

        /// <summary>
        /// OnInspectorGUI
        /// </summary>
        public override void OnInspectorGUI()
        {
            this.serializedObject.Update();

            base.OnInspectorGUI();

            this.target.OnInspectorGUI();

            this.serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}
