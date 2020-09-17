using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Master
{
    /// <summary>
    ///  アクセサリ、エフェクトデータ
    /// </summary>
    public class AccessoriesData : ModelBase, IItemInfo
    {
        [JsonProperty("accessoriesName"), Localize]
        public string name { get; set; }

        [JsonProperty("description"), Localize]
        public string info { get; set; }

        [JsonProperty("key")]
        public string key { get; set; }

        [JsonProperty("rarity")]
        public uint rarity { get; set; }

        [JsonProperty("itemSellId")]
        public uint itemSellId { get; set; }

        ItemType IItemInfo.GetItemType() => ItemType.Accessory;
        bool IItemInfo.IsCommonSprite() => false;
        string IItemInfo.GetSpritePath() => SharkDefine.GetAccessoryThumbnailPath(this.key);
        Rank IItemInfo.GetRank() => (Rank)this.rarity;
        string IItemInfo.GetName() => this.name;
        string IItemInfo.GetDescription() => this.info;
    }
}