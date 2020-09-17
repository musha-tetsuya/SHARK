using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// シーン遷移アニメーション
/// </summary>
public class SceneChangeAnimation : MonoBehaviour
{
    /// <summary>
    /// アニメーション
    /// </summary>
    [SerializeField]
    private Animator animator = null;
    /// <summary>
    /// アニメーションイベントレシーバー
    /// </summary>
    [SerializeField]
    private AnimationEventReceiver animationEventReceiver = null;

    /// <summary>
    /// inアニメ終了時コールバック
    /// </summary>
    public Action onFinishedIn = null;
    /// <summary>
    /// outアニメ終了時コールバック
    /// </summary>
    public Action onFinishedOut = null;

    /// <summary>
    /// Awake
    /// </summary>
    private void Awake()
    {
        this.animationEventReceiver.onFinished = this.OnFinished;
    }

    /// <summary>
    /// アニメーション終了時
    /// </summary>
    private void OnFinished(string tag)
    {
        //Inアニメーション終了時
        if (tag == "in")
        {
            this.onFinishedIn?.Invoke();
        }
        //Outアニメーション終了時
        else if (tag == "out")
        {
            this.onFinishedOut?.Invoke();
        }
    }

    /// <summary>
    /// loopからoutに遷移させる
    /// </summary>
    public void SetOut()
    {
        this.animator.SetBool("out", true);
    }
}
