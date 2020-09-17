using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemInventoryDecompositionScrollViewItem : MonoBehaviour
{
    /// <summary>
    /// コモンアイコン
    /// </summary>
    [SerializeField]
    private CommonIcon commonIcon = null;

    /// <summary>
    /// 分解リストスクロールビューアイテムセット
    /// </summary>
    public void Set(uint itemType, uint itemId, uint itemNum)
    {
        var itemInfo = CommonIconUtility.GetItemInfo(itemType, itemId);
        this.commonIcon.Set(itemInfo, false);
        this.commonIcon.SetCountText(itemNum);
    }
}
