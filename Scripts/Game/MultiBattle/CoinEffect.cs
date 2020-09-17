using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// コインエフェクト
/// </summary>
public class CoinEffect : MonoBehaviour
{
    /// <summary>
    /// アニメーションイベント受信
    /// </summary>
    [SerializeField]
    private AnimationEventReceiver animationEventReceiver = null;

    /// <summary>
    /// コイン数テキスト
    /// </summary>
    [SerializeField]
    private Text coinNumText = null;

    /// <summary>
    /// アニメーション終了カウント
    /// </summary>
    [SerializeField]
    private int finishedCount = 1;

    /// <summary>
    /// 破棄時コールバック
    /// </summary>
    public Action onDestroy = null;

    /// <summary>
    /// Awake
    /// </summary>
    private void Awake()
    {
        this.animationEventReceiver.onFinished = this.OnFinished;
    }

    /// <summary>
    /// ポジション設定
    /// </summary>
    public void SetPosition(Vector2 pos)
    {
        (this.transform as RectTransform).anchoredPosition = pos;
    }

    /// <summary>
    /// フォント設定
    /// </summary>
    public void SetFont(Font font)
    {
        this.coinNumText.font = font;
    }

    /// <summary>
    /// コイン数値セット
    /// </summary>
    public void SetNum(long num)
    {
        this.coinNumText.text = string.Format("+{0:#,0}", num);
    }

    /// <summary>
    /// アニメーション終了時
    /// </summary>
    private void OnFinished(string tag)
    {
        this.finishedCount--;

        if (this.finishedCount == 0)
        {
            Destroy(this.gameObject);
            this.onDestroy?.Invoke();
        }
    }
}
