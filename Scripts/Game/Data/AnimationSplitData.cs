using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// アニメーション分割データ
/// <para>
/// FBXのMorpherモーションをインポート時にクリップ分割するためのデータ
/// </para>
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObject/AnimationSplitData")]
public class AnimationSplitData : ScriptableObject
{
    /// <summary>
    /// クリップデータ
    /// </summary>
    [Serializable]
    public class ClipData
    {
        /// <summary>
        /// クリップ名
        /// </summary>
        [SerializeField]
        public string name = null;
        /// <summary>
        /// 開始フレーム
        /// </summary>
        [SerializeField]
        public int startFrame = 0;
        /// <summary>
        /// 終了フレーム
        /// </summary>
        [SerializeField]
        public int endFrame = 0;
        /// <summary>
        /// ループ
        /// </summary>
        [SerializeField]
        public bool loop = false;
    }

    /// <summary>
    /// クリップデータリスト
    /// </summary>
    [SerializeField]
    public ClipData[] clips = null;
}
