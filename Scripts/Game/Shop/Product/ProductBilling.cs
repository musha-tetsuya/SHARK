using System.Linq;

public abstract class ProductBilling : ProductBase
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
    public new Master.BillingData master => base.master as Master.BillingData;

    /// <summary>
    /// construct
    /// </summary>
    protected ProductBilling(Master.BillingData master)
        : base(master)
    {
        //値段
        this.price = this.master.needMoney;

        //付与されるアイテム
        this.addItems = Masters.BillingItemDB
            .GetList()
            .Where(x => x.billingItemId == this.master.billingItemId)
            .Select(x => new AddItem{ itemType = x.itemType, itemId = x.itemId, itemNum = x.itemNum })
            .ToArray();
    }
}
