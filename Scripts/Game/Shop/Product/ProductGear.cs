using System.Collections.Generic;

public class ProductGear : ProductShop
{
    /// <summary>
    /// 商品アイコンのフレームと背景を表示するかどうか
    /// </summary>
    public override bool isVisibleProductIconFrame => false;

    /// <summary>
    /// constructor
    /// </summary>
    public ProductGear(Master.ShopData master)
        : base(master)
    {

    }
}
