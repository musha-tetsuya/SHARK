using System;

/// <summary>
/// ユーザーのミッションの進捗データ
/// </summary>
public class UserMission
{
    /// <summary>
    /// Id
    /// </summary>
    public uint id;
    /// <summary>
    /// missionId
    /// </summary>
    public uint missionId;
    /// <summary>
    /// 進捗度
    /// </summary>
    public ulong missionCount;
    /// <summary>
    /// ミッションの状態 (1:未クリア  2:クリア済み未受け取り  3:クリア済み受け取り済み)
    /// </summary>
    public uint clearStatus;
    /// <summary>
    /// 任意型期間限定の開始時間 (スタートダッシュなど)
    /// </summary>
    public DateTime? startDate;
}
