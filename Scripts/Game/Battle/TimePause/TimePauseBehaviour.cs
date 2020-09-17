using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// 時間停止機能に対応したMonoBihaviour
/// </summary>
public class TimePauseBehaviour : MonoBehaviour, ITimePause
{
    /// <summary>
    /// Awake
    /// </summary>
    protected virtual void Awake()
    {
        TimePauseManager.Add(this);
    }

    /// <summary>
    /// OnDestroy
    /// </summary>
    protected virtual void OnDestroy()
    {
        TimePauseManager.Remove(this);
    }

    /// <summary>
    /// 停止
    /// </summary>
    public virtual void Pause(BinaryWriter writer)
    {
        writer.Write(this.enabled);
        this.enabled = false;
    }

    /// <summary>
    /// 再開
    /// </summary>
    public virtual void Play(BinaryReader reader)
    {
        this.enabled = reader.ReadBoolean();
    }
}
