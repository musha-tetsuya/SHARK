using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Master
{
    /// <summary>
    ///  台座データ
    /// </summary>
    public class BatteryData : ModelBase, IItemInfo
    {
        [JsonProperty("batteryName"), Localize]
        public string name { get; set; }

        [JsonProperty("description"), Localize]
        public string description { get; set; }
        
        [JsonProperty("key")]
        public string key { get; set; }

        [JsonProperty("rarity")]
        public uint rarity { get; set; }

        [JsonProperty("seriesId")]
        public uint seriesId { get; set; }

        [JsonProperty("fvId")]
        public uint fvAttackId { get; set; }

        [JsonProperty("fvPoint")]
        public uint fvPoint { get; set; }

        [JsonProperty("defaultGearSlotSize")]
        public uint defaultGearSlotSize { get; set; }

        [JsonProperty("itemSellId")]
        public uint itemSellId { get; set; }

        ItemType IItemInfo.GetItemType() => ItemType.BattleItem;
        bool IItemInfo.IsCommonSprite() => false;
        string IItemInfo.GetSpritePath() => SharkDefine.GetBatterySpritePath(this.key);
        Rank IItemInfo.GetRank() => (Rank)this.rarity;
        string IItemInfo.GetName() => this.name;
        string IItemInfo.GetDescription() => this.description;
    }
}