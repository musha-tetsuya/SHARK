using System.Linq;

public abstract class ProductShop : ProductBase
{
    /// <summary>
    /// 商品名
    /// </summary>
    public override string productName => this.master.name;
    /// <summary>
    /// 商品説明
    /// </summary>
    public override string description => this.master.description;
    /// <summary>
    /// 購入上限
    /// </summary>
    public override uint? maxCount => this.master.maxCount;
    /// <summary>
    /// ソート
    /// </summary>
    public override uint sort => this.master.sort;

    /// <summary>
    /// マスター
    /// </summary>
    public new Master.ShopData master => base.master as Master.ShopData;

    /// <summary>
    /// construct
    /// </summary>
    protected ProductShop(Master.ShopData master)
        : base(master)
    {
        //支払い方法と値段の設定
        if (this.master.needChargeGem > 0)
        {
            this.payType = ItemType.ChargeGem;
            this.price = this.master.needChargeGem;
        }
        else if (this.master.needFreeGem > 0)
        {
            this.payType = ItemType.FreeGem;
            this.price = this.master.needFreeGem;
        }
        else if (this.master.needCoin > 0)
        {
            this.payType = ItemType.Coin;
            this.price = this.master.needCoin;
        }

        //付与されるアイテム
        this.addItems = Masters.ShopItemDB
            .GetList()
            .Where(x => x.shopItemId == this.master.shopItemId)
            .Select(x => new AddItem{ itemType = x.itemType, itemId = x.itemId, itemNum = x.itemNum })
            .ToArray();
    }
}
