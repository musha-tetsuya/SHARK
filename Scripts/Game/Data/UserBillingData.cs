/// <summary>
/// ユーザーの課金データ
/// </summary>
public class UserBillingData : UserProductData
{
    /// <summary>
    /// 課金ステータス
    /// </summary>
    public enum Status
    {
        AddItem          = 1,   //アイテム付与済み（成功）
        BillingFailed    = 2,   //何らかのエラー？（失敗）
        ReceiptChkFailed = 3,   //レシート検証失敗（失敗）
    }

    /// <summary>
    /// 課金ID
    /// </summary>
    public uint billingId;
    /// <summary>
    /// プラットフォーム
    /// </summary>
    public uint platform;
    /// <summary>
    /// ステータス
    /// </summary>
    public uint status;
    /// <summary>
    /// レシート
    /// </summary>
    public string receipt;

    public override uint Id => billingId;
    public override uint BuyNum => 0;
}
