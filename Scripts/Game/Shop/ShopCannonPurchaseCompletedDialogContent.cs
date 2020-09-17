using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 砲台購入完了ダイアログの内容
/// </summary>
public class ShopCannonPurchaseCompletedDialogContent : PurchaseCompletedDialogBase
{
    /// <summary>
    /// 砲台のコンテンツ
    /// </summary>
    [SerializeField]
    private ProductTurretContent turretContent = null;

    /// <summary>
    /// 内容構築
    /// </summary>
    public void Set(TurretViewer turretViewerPrefab, ProductBase product, SimpleDialog dialog)
    {
        base.Set(product, dialog);

        //砲台表示設定
        this.turretContent.SetInfo(product as ProductCannon, turretViewerPrefab);
    }
}
