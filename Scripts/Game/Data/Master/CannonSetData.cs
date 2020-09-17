using Newtonsoft.Json;

namespace Master
{
    /// <summary>
    ///  砲台データ
    /// </summary>
    public class CannonSetData : ModelBase, IItemInfo
    {
        [JsonProperty("batteryId")]
        public uint batteryId { get; set; }

        [JsonProperty("barrelId")]
        public uint barrelId { get; set; }

        [JsonProperty("bulletId")]
        public uint bulletId { get; set; }

        ItemType IItemInfo.GetItemType() => ItemType.CannonSet;
        bool IItemInfo.IsCommonSprite() => false;
        string IItemInfo.GetSpritePath() => SharkDefine.GetTurretSetSpritePath(Masters.BatteryDB.FindById(this.batteryId).key);
        Rank IItemInfo.GetRank() => Rank.None;
        string IItemInfo.GetName() => Masters.TurretSerieseDB.FindById(Masters.BatteryDB.FindById(this.batteryId).seriesId).name;
        string IItemInfo.GetDescription() => null;
    }
}
