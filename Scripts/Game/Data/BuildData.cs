using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ビルド情報
/// </summary>
public class BuildData : ScriptableObject
{
    /// <summary>
    /// ビルド番号
    /// </summary>
    /// <para>
    /// Androidの場合BundleVersionCode、iOSの場合BuildNumber。Jenkinsのビルド番号。
    /// </para>
    [SerializeField]
    public int buildNumber = 0;
}
