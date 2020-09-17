using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ショップメインタブ
/// </summary>
public class ShopMainTab : TabBase
{
    /// <summary>
    /// RectTransform
    /// </summary>
    [SerializeField]
    private RectTransform rectTransform = null;
    /// <summary>
    /// アクティブ時移動値
    /// </summary>
    [SerializeField]
    private Vector2 activePosition = new Vector2(40f, 0f);
    /// <summary>
    /// テキストアウトライン
    /// </summary>
    [SerializeField]
    private Shadow textShadow = null;
    /// <summary>
    /// アクティブ時テキストアウトライン色
    /// </summary>
    [SerializeField]
    private Color activeTextOutlineColor = Color.white;
    /// <summary>
    /// 非アクティブ時テキストアウトライン色
    /// </summary>
    [SerializeField]
    private Color inactiveTextOutlineColor = Color.white;
    /// <summary>
    /// ショップページタイプ
    /// </summary>
    [SerializeField]
    public ShopScene.PageType tabType = ShopScene.PageType.None;

    /// <summary>
    /// アクティブ切り替え
    /// </summary>
    protected override void OnChangeActive()
    {
        base.OnChangeActive();

        //テキストアウトライン色と位置の変化
        if (this.isActive)
        {
            this.textShadow.effectColor = this.activeTextOutlineColor;
            this.rectTransform.sizeDelta += this.activePosition;
        }
        else
        {
            this.textShadow.effectColor = this.inactiveTextOutlineColor;
            this.rectTransform.sizeDelta -= this.activePosition;
        }
    }
}
