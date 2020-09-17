using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// プレゼント受け取りダイアログ内容
/// </summary>
public class PresentReceiveDialogContent : MonoBehaviour
{
    /// <summary>
    /// スクロールビュー
    /// </summary>
    [SerializeField]
    private InfiniteScrollView scrollView = null;
    /// <summary>
    /// スクロールビュー要素プレハブ
    /// </summary>
    [SerializeField]
    private PresentBoxItem presentBoxItemPrefab = null;

    /// <summary>
    /// アイテム
    /// </summary>
    private TPresentBox[] items = null;

    /// <summary>
    /// 表示構築
    /// </summary>
    public void BuildView(TPresentBox[] items)
    {
        this.items = items;
        this.scrollView.Initialize(this.presentBoxItemPrefab.gameObject, this.items.Length, this.OnUpdateElement);
    }

    /// <summary>
    /// スクロールビュー要素更新時
    /// </summary>
    private void OnUpdateElement(GameObject gobj, int index)
    {
        var item = gobj.GetComponent<PresentBoxItem>();
        item.BuildView(this.items[index], null);
    }
}
