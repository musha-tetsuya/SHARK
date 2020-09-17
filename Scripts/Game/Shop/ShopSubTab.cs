using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ショップサブタブ
/// </summary>
public class ShopSubTab : TabBase
{
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

        //テキストアウトライン色変化
        if (this.isActive)
        {
            this.textShadow.effectColor = this.activeTextOutlineColor;
        }
        else
        {
            this.textShadow.effectColor = this.inactiveTextOutlineColor;
        }
    }
}
