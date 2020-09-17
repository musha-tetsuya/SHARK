using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 砲台購入確認ダイアログ内容
/// </summary>
public class ShopCannonPurchaseConfirmDialogContent : PurchaseConfirmDialogBase
{
    /// <summary>
    /// 砲台のコンテンツ
    /// </summary>
    [SerializeField]
    private ProductTurretContent turretContent = null;

    /// <summary>
    /// 内容構築
    /// </summary>
    public void Set(TurretViewer turretViewerPrefab, ProductIcon icon, ProductBase product, SimpleDialog dialog, Action<UserBillingData, UserShopData, ProductIcon, ProductBase> onPurchaseCompleted, Action onNonPurchase)
    {
        base.Set(icon, product, dialog, onPurchaseCompleted, onNonPurchase);

        //砲台表示設定
        this.turretContent.SetInfo(product as ProductCannon, turretViewerPrefab);
    }
}
