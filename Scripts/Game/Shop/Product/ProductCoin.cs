using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ProductCoin : ProductShop
{
    /// <summary>
    /// 商品アイコンのフレームと背景を表示するかどうか
    /// </summary>
    public override bool isVisibleProductIconFrame => false;

    /// <summary>
    /// constructor
    /// </summary>
    public ProductCoin(Master.ShopData master)
        : base(master)
    {
        this.iconSize = new Vector2(256, 256);
    }

    /// <summary>
    /// CommonIconに対する追加処理
    /// </summary>
    public override void SetCommonIcon(CommonIcon icon)
    {
        var sprite = SharedUI.Instance.commonAtlas.GetSprite(this.master.key);
        icon.SetIconSprite(sprite);
    }
}
