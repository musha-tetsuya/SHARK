using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// アイテム詳細ダイアログ内容
/// </summary>
public class CommonItemInfoDialogContent : CommonItemInfoDialogContentBase
{
    /// <summary>
    /// CommonIcon
    /// </summary>
    [SerializeField]
    private CommonIcon icon = null;
    /// <summary>
    /// 名前テキスト
    /// </summary>
    [SerializeField]
    private Text nameText = null;
    /// <summary>
    /// 説明テキスト
    /// </summary>
    [SerializeField]
    private Text descriptionText = null;

    /// <summary>
    /// 内容構築
    /// </summary>
    public override void Setup(IItemInfo itemInfo)
    {
        this.icon.Set(itemInfo, false);
        this.nameText.text = itemInfo.GetName();
        this.descriptionText.text = itemInfo.GetDescription();
    }

    /// <summary>
    /// 内容構築
    /// </summary>
    public override void Setup(ProductBase product)
    {
        this.icon.Set(product.addItems[0].itemType, product.addItems[0].itemId, false);
        this.icon.SetFrameVisible(product.isVisibleProductIconFrame);
        product.SetCommonIcon(this.icon);
        this.nameText.text = product.productName;
        this.descriptionText.text = product.description;
    }
}
