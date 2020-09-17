using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// アイテム詳細ダイアログデータ
/// </summary>
[Serializable]
public class CommonItemInfoDialogData
{
    /// <summary>
    /// アイテム詳細ダイアログ内容プレハブ
    /// </summary>
    [SerializeField]
    private CommonItemInfoDialogContent commonInfoDialogContentPrefab = null;
    /// <summary>
    /// 砲台パーツ詳細ダイアログ内容プレハブ
    /// </summary>
    [SerializeField]
    private TurretPartsInfoDialogContent turretPartsInfoDialogContentPrefab = null;

    /// <summary>
    /// 詳細ダイアログ開く
    /// </summary>
    private CommonItemInfoDialogContentBase OpenDialog(ItemType itemType)
    {
        CommonItemInfoDialogContentBase contentPrefab = null;

        switch (itemType)
        {
            case ItemType.Battery:
            case ItemType.Barrel:
            case ItemType.Bullet:
            case ItemType.Accessory:
            case ItemType.Gear:
            contentPrefab = this.turretPartsInfoDialogContentPrefab;
            break;

            case ItemType.ChargeGem:
            case ItemType.FreeGem:
            case ItemType.Coin:
            case ItemType.BattleItem:
            contentPrefab = this.commonInfoDialogContentPrefab;
            break;

            default:
            Debug.LogWarningFormat("詳細ダイアログは開けない：ItemType={0}", itemType);
            return null;
        }

        var dialog = SharedUI.Instance.ShowSimpleDialog();
        dialog.closeButtonEnabled = true;
        return dialog.AddContent(contentPrefab);
    }

    /// <summary>
    /// 詳細ダイアログ開く
    /// </summary>
    public void OpenDialog(IItemInfo itemInfo)
    {
        this.OpenDialog(itemInfo.GetItemType())?.Setup(itemInfo);
    }

    /// <summary>
    /// 詳細ダイアログ開く
    /// </summary>
    public void OpenDialog(ProductBase product)
    {
        this.OpenDialog((ItemType)product.addItems[0].itemType)?.Setup(product);
    }
}
