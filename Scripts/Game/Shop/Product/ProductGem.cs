using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ProductGem : ProductBilling
{
    /// <summary>
    /// 商品名
    /// </summary>
    public override string productName => this.m_productName;
    private string m_productName = null;
    /// <summary>
    /// 商品説明
    /// </summary>
    public override string description => this.m_description;
    private string m_description = null;
    /// <summary>
    /// 商品アイコンのフレームと背景を表示するかどうか
    /// </summary>
    public override bool isVisibleProductIconFrame => false;

    /// <summary>
    /// 付与される有償ジェム数
    /// </summary>
    public uint chargeGem { get; private set; }
    /// <summary>
    /// 付与される無償ジェム数
    /// </summary>
    public uint freeGem { get; private set; }

    /// <summary>
    /// constructor
    /// </summary>
    public ProductGem(Master.BillingData master)
        : base(master)
    {
        this.iconSize = new Vector2(256, 256);
        this.chargeGem = (uint)this.addItems.Where(x => x.itemType == (uint)ItemType.ChargeGem).Sum(x => x.itemNum);
        this.freeGem = (uint)this.addItems.Where(x => x.itemType == (uint)ItemType.FreeGem).Sum(x => x.itemNum);

        //商品名は「〇〇ジェム」になる
        this.m_productName = Masters.LocalizeTextDB.GetFormat("UnitGem", this.chargeGem + this.freeGem);

        //商品説明は「{0:#,0}ジェム（有償{1:#,0}ジェム）」になる
        this.m_description = Masters.LocalizeTextDB.GetFormat("BreakdownTotalGem", this.chargeGem + this.freeGem, this.chargeGem);
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
