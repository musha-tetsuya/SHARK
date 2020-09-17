using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Master
{
    /// <summary>
    /// 砲身データ
    /// </summary>
    public class BarrelData : ModelBase, IItemInfo
    {
        [JsonProperty("barrelName"), Localize]
        public string name { get; set; }
        
        [JsonProperty("description"), Localize]
        public string description { get; set; }

        [JsonProperty("key")]
        public string key { get; set; }

        [JsonProperty("rarity")]
        public uint rarity { get; set; }

        [JsonProperty("seriesId")]
        public uint seriesId { get; set; }

        [JsonProperty("speed")]
        public uint speed { get; set; }

        [JsonProperty("interval")]
        public uint interval { get; set; }

        [JsonProperty("defaultGearSlotSize")]
        public uint defaultGearSlotSize { get; set; }

        [JsonProperty("seName")]
        public string seName { get; set; }

        [JsonProperty("itemSellId")]
        public uint itemSellId { get; set; }

        ItemType IItemInfo.GetItemType() => ItemType.Barrel;
        bool IItemInfo.IsCommonSprite() => false;
        string IItemInfo.GetSpritePath() => SharkDefine.GetBarrelSpritePath(this.key);
        Rank IItemInfo.GetRank() => (Rank)this.rarity;
        string IItemInfo.GetName() => this.name;
        string IItemInfo.GetDescription() => this.description;
    }
}