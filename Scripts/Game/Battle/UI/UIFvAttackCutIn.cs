using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// FVアタック発動時カットイン
/// </summary>
public class UIFvAttackCutIn : TimePauseBehaviour
{
    /// <summary>
    /// アニメーター
    /// </summary>
    [SerializeField]
    private Animator animator = null;

    /// <summary>
    /// アニメ終了時コールバック
    /// </summary>
    public Action onFinished = null;

    /// <summary>
    /// 一時停止時
    /// </summary>
    public override void Pause(BinaryWriter writer)
    {
        base.Pause(writer);
        writer.Write(this.animator.enabled);
        this.animator.enabled = false;
    }

    /// <summary>
    /// 一時停止解除時
    /// </summary>
    public override void Play(BinaryReader reader)
    {
        base.Play(reader);
        this.animator.enabled = reader.ReadBoolean();
    }

    /// <summary>
    /// アニメ終了時
    /// </summary>
    private void OnFinished()
    {
        this.onFinished?.Invoke();
    }
}