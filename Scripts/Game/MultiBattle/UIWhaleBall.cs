using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 龍玉UI
/// </summary>
public class UIWhaleBall : AnimationEventReceiver
{
    /// <summary>
    /// RectTransform
    /// </summary>
    [SerializeField]
    private RectTransform rectTransform = null;
    /// <summary>
    /// 位置値
    /// </summary>
    [SerializeField]
    private float positionValue = 0f;
    /// <summary>
    /// Getテキスト
    /// </summary>
    [SerializeField]
    private RectTransform getText = null;

    /// <summary>
    /// 取得アニメーション再生
    /// </summary>
    public void PlayGetAnimation(Vector3 dropPosition)
    {
        SoundManager.Instance.PlaySe(SeName.BALL_DROP);

        //表示ON
        this.gameObject.SetActive(true);

        //位置調整
        var screenPoint = RectTransformUtility.WorldToScreenPoint(Battle.BattleGlobal.instance.fishCamera, dropPosition);
        Vector2 anchoredPosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(this.rectTransform.parent as RectTransform, screenPoint, Battle.BattleGlobal.instance.uiCamera, out anchoredPosition);
        this.rectTransform.anchoredPosition = anchoredPosition;

        //テキストが反転してたら元に戻す
        this.getText.eulerAngles = Vector3.zero;

        //取得アニメーション開始
        this.positionValue = 1f;
        this.animator.Play("get", 0, 0f);

        //アニメーションに合わせて取得位置から移動
        StartCoroutine(this.Move());
    }

    /// <summary>
    /// 移動
    /// </summary>
    private IEnumerator Move()
    {
        var startPosition = this.rectTransform.anchoredPosition;

        while (true)
        {
            this.rectTransform.anchoredPosition = startPosition * this.positionValue;

            if (this.positionValue > 0f)
            {
                yield return null;
            }
            else
            {
                SoundManager.Instance.PlaySe(SeName.BALL_GET);
                break;
            }
        }
    }

    /// <summary>
    /// 円形回転開始時エフェクト再生
    /// </summary>
    public void PlayCircleStartEffectAnimation()
    {
        this.animator.Play("Circle_start", 0, 0f);
    }

    /// <summary>
    /// 円形回転時エフェクト再生
    /// </summary>
    public void PlayCircleEffectAnimation()
    {
        this.animator.Play("Circle", 0, 0f);
    }

    /// <summary>
    /// 点滅アニメーション再生
    /// </summary>
    public void PlayBlinkAnimation()
    {
        this.animator.Play("blink", 0, 0f);
    }
}
