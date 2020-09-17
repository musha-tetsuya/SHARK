using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Master
{
    /// <summary>
    /// バトルアイテムデータ
    /// </summary>
    public class BattleItemData : ModelBase, IItemInfo
    {
        [JsonProperty("itemName"), Localize]
        public string name { get; set; }

        [JsonProperty("description"), Localize]
        public string description { get; set; }

        [JsonProperty("skillGroupId")]
        public uint skillGroupId { get; set; }

        [JsonProperty("key")]
        public string key { get; set; }

        ItemType IItemInfo.GetItemType() => ItemType.BattleItem;
        bool IItemInfo.IsCommonSprite() => false;
        string IItemInfo.GetSpritePath() => SharkDefine.GetBattleItemIconSpritePath(this.key);
        Rank IItemInfo.GetRank() => Rank.None;
        string IItemInfo.GetName() => this.name;
        string IItemInfo.GetDescription() => this.description;
    }
}
