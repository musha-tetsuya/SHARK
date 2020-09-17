using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// 時間停止インターフェース
/// </summary>
public interface ITimePause
{
    /// <summary>
    /// 停止
    /// </summary>
    void Pause(BinaryWriter writer);
    /// <summary>
    /// 再開
    /// </summary>
    void Play(BinaryReader reader);
}
