using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 商品購入完了ダイアログの内容
/// </summary>
public class ShopPurchaseCompletedDialogContent : PurchaseCompletedDialogBase
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
    public override void Set(ProductBase product, SimpleDialog dialog)
    {
        base.Set(product, dialog);

        //CommonIcon表示構築
        this.commonIcon.Set(this.product.addItems[0].itemType, this.product.addItems[0].itemId, false);

        //商品説明
        this.productInfoText.text = this.product.description;
    }
}
