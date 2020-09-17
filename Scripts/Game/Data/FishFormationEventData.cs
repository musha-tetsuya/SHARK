using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 魚編隊イベントデータ
/// </summary>
[Serializable]
public class FishFormationEventData
{
    /// <summary>
    /// イベントタイプ
    /// </summary>
    public enum EventType
    {
        Play,   //再生
        Pause,  //一時停止
        Quit,   //強制終了
    }

    /// <summary>
    /// イベントタイプ
    /// </summary>
    [SerializeField]
    public EventType eventType = EventType.Play;

    /// <summary>
    /// イベント発火時間
    /// </summary>
    [SerializeField]
    public float time = 0f;
}
