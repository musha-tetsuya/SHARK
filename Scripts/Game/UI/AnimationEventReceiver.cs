using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// アニメーションイベントレシーバー
/// </summary>
public class AnimationEventReceiver : MonoBehaviour
{
    /// <summary>
    /// アニメーター
    /// </summary>
    [SerializeField]
    protected Animator animator = null;
    /// <summary>
    /// アニメ終了時イベント
    /// </summary>
    [SerializeField]
    public Action<string> onFinished = null;

    /// <summary>
    /// アニメ終了時
    /// </summary>
    protected virtual void OnFinished(string tag)
    {
        this.onFinished?.Invoke(tag);
    }
}
