using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 龍魂UI
/// </summary>
public class UIWhaleSoul : AnimationEventReceiver
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
    /// イメージ
    /// </summary>
    [SerializeField]
    private Image image = null;
    /// <summary>
    /// Getテキスト
    /// </summary>
    [SerializeField]
    private RectTransform getText = null;

    /// <summary>
    /// 龍魂番号設定
    /// </summary>
    public void SetNumber(uint num)
    {
                                    //123456789
        uint n = (num - 1) / 3 + 1; //111222333
        this.image.sprite = SharedUI.Instance.commonAtlas.GetSprite("Soul_0" + n);
    }

    /// <summary>
    /// 取得アニメーション再生
    /// </summary>
    public void PlayGetAnimation(uint num, Vector3 dropPosition, RectTransform parent, Action onFinished)
    {
        //ゴール位置
        var screenPoint = RectTransformUtility.WorldToScreenPoint(Battle.BattleGlobal.instance.uiCamera, this.rectTransform.position);
        Vector2 goalPosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, screenPoint, Battle.BattleGlobal.instance.uiCamera, out goalPosition);

        //取得アニメーション用龍魂複製
        var soul = Instantiate(this, parent, false);

        //龍魂番号設定
        soul.SetNumber(num);

        //取得アニメーション再生
        soul.PlayGetAnimation(dropPosition, goalPosition, () =>
        {
            //取得アニメーション用龍魂破棄
            Destroy(soul.gameObject);
            
            //代わりに自身の表示をON
            this.gameObject.SetActive(true);
            this.SetNumber(num);

            //終了を通知
            onFinished?.Invoke();
        });
    }

    /// <summary>
    /// 取得アニメーション再生
    /// </summary>
    private void PlayGetAnimation(Vector3 dropPosition, Vector3 goalPosition, Action onFinished)
    {
        SoundManager.Instance.PlaySe(SeName.SOUL_DROP);

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
        StartCoroutine(this.Move(goalPosition, onFinished));
    }

    /// <summary>
    /// 移動
    /// </summary>
    private IEnumerator Move(Vector2 goalPosition, Action onFinished)
    {
        var startPosition = this.rectTransform.anchoredPosition;

        while (true)
        {
            this.rectTransform.anchoredPosition = Vector2.Lerp(goalPosition, startPosition, this.positionValue);

            if (this.positionValue > 0f)
            {
                yield return null;
            }
            else
            {
                SoundManager.Instance.PlaySe(SeName.SOUL_GET);
                break;
            }
        }

        onFinished?.Invoke();
    }

    /// <summary>
    /// エフェクトアニメーション再生
    /// </summary>
    public void PlayEffectAnimation()
    {
        this.animator.Play("SoulEffect", 0, 0f);
    }
}
