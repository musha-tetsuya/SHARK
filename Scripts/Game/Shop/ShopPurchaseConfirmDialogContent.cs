using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 商品購入確認ダイアログ内容
/// </summary>
public class ShopPurchaseConfirmDialogContent : PurchaseConfirmDialogBase
{
    /// <summary>
    /// 商品アイコン
    /// </summary>
    [SerializeField]
    private CommonIcon commonIcon = null;
    /// <summary>
    /// 購入する商品詳細テキスト
    /// </summary>
    [SerializeField]
    private Text productInfoText = null;

    /// <summary>
    /// 内容構築
    /// </summary>
    public override void Set(ProductIcon icon, ProductBase product, SimpleDialog dialog, Action<UserBillingData, UserShopData, ProductIcon, ProductBase> onPurchaseCompleted, Action onNonPurchase)
    {
        base.Set(icon, product, dialog, onPurchaseCompleted, onNonPurchase);

        //CommonIcon表示構築
        this.commonIcon.Set(this.product.addItems[0].itemType, this.product.addItems[0].itemId, false);

        //商品説明
        this.productInfoText.text = this.product.description;
    }
}
