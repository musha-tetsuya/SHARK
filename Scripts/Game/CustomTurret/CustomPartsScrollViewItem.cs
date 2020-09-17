using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// パーツスクロールビュー要素
/// </summary>
public class CustomPartsScrollViewItem : MonoBehaviour
{
    /// <summary>
    /// CommonIcon
    /// </summary>
    [SerializeField]
    private CommonIcon commonIcon = null;
    /// <summary>
    /// 選択中マーク
    /// </summary>
    [SerializeField]
    private GameObject selectedMark = null;

    /// <summary>
    /// ユーザーパーツデータ
    /// </summary>
    public UserPartsData partsData { get; private set; }

    /// <summary>
    /// 表示構築
    /// </summary>
    public void Set(UserPartsData data, bool isSelected, Action<CustomPartsScrollViewItem> onClick)
    {
        this.partsData = data;

        //アイコン設定
        this.commonIcon.Set(this.partsData.itemType, this.partsData.itemId, true);

        //選択中マークON/OFF
        this.selectedMark.SetActive(isSelected);

        //クリック時処理登録
        this.commonIcon.onClick = () => onClick(this);
    }
}
