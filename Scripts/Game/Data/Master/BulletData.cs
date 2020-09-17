using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Master
{
    public class BulletData : ModelBase, IItemInfo
    {
        [JsonProperty("bulletName"), Localize]
        public string name { get; set; }
        
        [JsonProperty("description"), Localize]
        public string description { get; set; }

        [JsonProperty("key")]
        public string key { get; set; }

        [JsonProperty("rarity")]
        public uint rarity { get; set; }

        [JsonProperty("seriesId")]
        public uint seriesId { get; set; }

        [JsonProperty("power")]
        public uint power { get; set; }
        
        [JsonProperty("skillGroupId")]
        public uint skillGroupId { get; set; }

        [JsonProperty("defaultGearSlotSize")]
        public uint defaultGearSlotSize { get; set; }

        [JsonProperty("seName")]
        public string seName { get; set; }

        [JsonProperty("itemSellId")]
        public uint itemSellId { get; set; }

        ItemType IItemInfo.GetItemType() => ItemType.Bullet;
        bool IItemInfo.IsCommonSprite() => false;
        string IItemInfo.GetSpritePath() => SharkDefine.GetBulletThumbnailPath(this.key);
        Rank IItemInfo.GetRank() => (Rank)this.rarity;
        string IItemInfo.GetName() => this.name;
        string IItemInfo.GetDescription() => this.description;
    }
}