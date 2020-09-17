using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class ProductBase
{
    public enum CoinSizeType
    {
        ShCoin_000_0001,
        ShCoin_000_0002,
        ShCoin_000_0003,
        ShCoin_000_0004
    }

    public enum GemSizeType
    {
        ShGem_000_0001,
        ShGem_000_0002,
        ShGem_000_0003,
        ShGem_000_0004
    }

    /// <summary>
    /// 商品タイプからPruductBaseへの変換
    /// </summary>
    private static readonly Dictionary<int, Func<Master.ModelBase, ProductBase>> productTypeDatas = new Dictionary<int, Func<Master.ModelBase, ProductBase>>
    {
        { (int)ItemType.ChargeGem,  x => new ProductGem(x as Master.BillingData) },
        { (int)ItemType.FreeGem,    x => new ProductGem(x as Master.BillingData) },
        { (int)ItemType.Coin,       x => new ProductCoin(x as Master.ShopData) },
        { (int)ItemType.Gear,       x => new ProductGear(x as Master.ShopData) },
        { (int)ItemType.CannonSet,  x => new ProductCannon(x as Master.ShopData) },
        { (int)ItemType.BattleItem, x => new ProductBattleItem(x as Master.ShopData) },
    };

    /// <summary>
    /// 商品名
    /// </summary>
    public abstract string productName { get; }
    /// <summary>
    /// 商品説明
    /// </summary>
    public abstract string description { get; }
    /// <summary>
    /// 購入上限
    /// </summary>
    public virtual uint? maxCount => null;
    /// <summary>
    /// ソート
    /// </summary>
    public abstract uint sort { get; }
    /// <summary>
    /// 商品アイコンのフレームと背景を表示するかどうか（コイン、ジェム、ギアは表示しない）
    /// </summary>
    public virtual bool isVisibleProductIconFrame => true;
    /// <summary>
    /// 商品アイコンサイズ
    /// </summary>
    public Vector2 iconSize { get; protected set; } = new Vector2(180, 180);
    /// <summary>
    /// 残り購入回数
    /// </summary>
    public uint? remainCount => this.maxCount.HasValue ? this.maxCount - this.buyCount : null;
    /// <summary>
    /// 購入上限に達しているか
    /// </summary>
    public bool isOverPurchaseLimit => this.maxCount.HasValue && this.maxCount <= this.buyCount;

    /// <summary>
    /// マスター
    /// </summary>
    public Master.ModelBase master { get; private set; }
    /// <summary>
    /// 支払方法
    /// </summary>
    public ItemType payType { get; protected set; }
    /// <summary>
    /// 値段
    /// </summary>
    public ulong price { get; protected set; }
    /// <summary>
    /// 付与されるアイテム
    /// </summary>
    public AddItem[] addItems { get; protected set; }
    /// <summary>
    /// 購入回数
    /// </summary>
    [NonSerialized]
    public uint buyCount = 0;

    /// <summary>
    /// construct
    /// </summary>
    protected ProductBase(Master.ModelBase master)
    {
        this.master = master;
    }

    /// <summary>
    /// マスターからProductBaseを生成
    /// </summary>
    public static ProductBase Create(Master.ModelBase master, uint buyCount)
    {
        ProductBase product = null;
        ItemType mainItemType = 0;

        if (master is Master.BillingData)
        {
            var billingData = master as Master.BillingData;
            mainItemType = (ItemType)Masters.BillingItemDB
                .GetList()
                .Find(x => x.billingItemId == billingData.billingItemId)
                .itemType;
        }
        else if (master is Master.ShopData)
        {
            var shopData = master as Master.ShopData;
            mainItemType = (ItemType)Masters.ShopItemDB
                .GetList()
                .Find(x => x.shopItemId == shopData.shopItemId)
                .itemType;
        }

        if (productTypeDatas.ContainsKey((int)mainItemType))
        {
            product = productTypeDatas[(int)mainItemType](master);
            product.buyCount = buyCount;

            if (product.payType == 0 && master is Master.ShopData)
            {
                Debug.LogWarningFormat("未対応の支払いタイプが設定されている:{0}:{1}", product.master.GetType(), product.master.id);
                product = null;
            }
        }

        return product;
    }

    /// <summary>
    /// CommonIconに対する追加処理
    /// </summary>
    public virtual void SetCommonIcon(CommonIcon icon){}
}
