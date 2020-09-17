using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// プレゼントBOXタブ
/// </summary>
public class PresentBoxTab : TabBase
{
    /// <summary>
    /// ページ内アイテム最大表示件数
    /// </summary>
    private const int MAX_PAGE_ITEM_SIZE = 30;

    /// <summary>
    /// テキストアウトライン
    /// </summary>
    [SerializeField]
    private Outline textShadow = null;
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
    /// ページ毎アイテム
    /// </summary>
    public TPresentBox[][] items { get; private set; }
    /// <summary>
    /// ページインデックス
    /// </summary>
    [NonSerialized]
    public int pageIndex = 0;

    /// <summary>
    /// アクティブ切り替え
    /// </summary>
    protected override void OnChangeActive()
    {
        base.OnChangeActive();

        //テキストアウトライン色の変化
        if (this.isActive)
        {
            this.textShadow.effectColor = this.activeTextOutlineColor;
        }
        else
        {
            this.textShadow.effectColor = this.inactiveTextOutlineColor;
        }
    }

    /// <summary>
    /// アイテムセット
    /// </summary>
    public void SetItems(TPresentBox[] items, int maxPageSize)
    {
        //総ページ数決定
        int totalPageSize = items.Length / MAX_PAGE_ITEM_SIZE;

        if (items.Length % MAX_PAGE_ITEM_SIZE > 0)
        {
            totalPageSize++;
        }

        //totalPageSize = Mathf.Clamp(totalPageSize, 1, maxPageSize);
        totalPageSize = Mathf.Max(1, totalPageSize);

        //各ページ内のアイテムを設定
        this.items = new TPresentBox[totalPageSize][];

        for (int i = 0; i < totalPageSize; i++)
        {
            this.items[i] = items
                .Skip(i * MAX_PAGE_ITEM_SIZE)
                .Take(MAX_PAGE_ITEM_SIZE)
                .ToArray();
        }
    }
}
