/// <summary>
/// ユーザーショップデータ
/// </summary>
public class UserShopData : UserProductData
{
    /// <summary>
    /// ショップID
    /// </summary>
    public uint shopId;
    /// <summary>
    /// 購入数
    /// </summary>
    public uint buyNum;

    public override uint Id => shopId;
    public override uint BuyNum => buyNum;
}
