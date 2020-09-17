using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Master
{
    public class GearData : ModelBase, IItemInfo
    {
        [JsonProperty("gearName"), Localize]
        public string name { get; set; }

        [JsonProperty("description"), Localize]
        public string description { get; set; }

        [JsonProperty("key")]
        public uint key { get; set; }

        [JsonProperty("subKey")]
        public uint subKey { get; set; }

        [JsonProperty("gearType")]
        public uint partsType { get; set; }

        [JsonProperty("skillGroupId")]
        public uint skillGroupId { get; set; }

        [JsonProperty("rarity")]
        public uint rarity { get; set; }

        [JsonProperty("rejectCoin")]
        public uint rejectCoin { get; set; }

        [JsonProperty("power")]
        public uint power { get; set; }

        [JsonProperty("speed")]
        public uint speed { get; set; }

        [JsonProperty("fvPoint")]
        public uint fvPoint { get; set; }

        [JsonProperty("itemSellId")]
        public uint itemSellId { get; set; }

        ItemType IItemInfo.GetItemType() => ItemType.Gear;
        bool IItemInfo.IsCommonSprite() => true;
        string IItemInfo.GetSpritePath() => null;
        Rank IItemInfo.GetRank() => (Rank)this.rarity;
        string IItemInfo.GetName() => this.name;
        string IItemInfo.GetDescription() => this.description;
    }
}