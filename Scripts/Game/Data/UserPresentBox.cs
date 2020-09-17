using System;

/// <summary>
/// ユーザーのプレゼントBoxデータ
/// </summary>
public class UserPresentBox
{
    /// <summary>
    /// presentBoxId
    /// </summary>
    public uint id;
    /// <summary>
    /// 
    /// </summary>
    public uint limitFlg;
    /// <summary>
    /// 受け取り期限
    /// </summary>
    public DateTime limitDate;
    /// <summary>
    /// アイテムタイプ
    /// </summary>
    public uint itemType;
    /// <summary>
    /// アイテムID
    /// </summary>
    public uint itemId;
    /// <summary>
    /// 所持数
    /// </summary>
    public uint itemNum;
    /// <summary>
    /// 後々使うが、現状では未使用
    /// </summary>
    public uint? attachId;
    /// <summary>
    /// 
    /// </summary>
    public uint receiveFlg;
    /// <summary>
    /// 
    /// </summary>
    public DateTime receiveDate;
}

/// <summary>
/// 無期限のプレゼントBoxの情報
/// </summary>
public class TPresentBox : AddItem
{
    /// <summary>
    /// 後々使うが、現状では未使用
    /// </summary>
    public uint? attachId;
    /// <summary>
    /// 説明文のマスターに紐づくID
    /// </summary>
    public uint messageId;
    /// <summary>
    /// 生成時間
    /// </summary>
    public DateTime? created_at;
}

/// <summary>
/// 無期限のプレゼントBoxの情報
/// </summary>
public class TPresentBoxLimited : TPresentBox
{
    /// <summary>
    /// 受け取り期限
    /// </summary>
    public DateTime? limitDate;
}

/// <summary>
/// 無期限のプレゼントBoxの情報
/// </summary>
public class TPresentBoxReceived : TPresentBox
{
    /// <summary>
    /// プレゼントBoxの種類 (1 = 無期限  ,  2 = 期限付き)
    /// </summary>
    public uint presentBoxType;
    /// <summary>
    /// ???
    /// </summary>
    public uint presentBoxId;
}
