using System.Collections.Generic;
using UnityEngine;

public class ProductCannon : ProductShop
{
    /// <summary>
    /// 商品アイコンのフレームと背景を表示するかどうか
    /// </summary>
    public override bool isVisibleProductIconFrame => false;
    /// <summary>
    /// 砲台セットデータ
    /// </summary>
    public Master.CannonSetData cannonSetData { get; private set; }

    /// <summary>
    /// constructor
    /// </summary>
    public ProductCannon(Master.ShopData master)
        : base(master)
    {
        this.iconSize = new Vector2(256, 256);
        this.cannonSetData = Masters.CannonSetDB.FindById(this.addItems[0].itemId);
    }
}
