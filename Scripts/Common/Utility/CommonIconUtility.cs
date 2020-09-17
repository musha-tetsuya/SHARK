using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// アイテム情報インターフェース
/// </summary>
public interface IItemInfo
{
    /// <summary>
    /// アイテムタイプ
    /// </summary>
    ItemType GetItemType();
    /// <summary>
    /// 共通素材スプライトかどうか
    /// </summary>
    bool IsCommonSprite();
    /// <summary>
    /// スプライトパス取得
    /// </summary>
    string GetSpritePath();
    /// <summary>
    /// ランク
    /// </summary>
    Rank GetRank();
    /// <summary>
    /// 名前
    /// </summary>
    string GetName();
    /// <summary>
    /// 説明
    /// </summary>
    string GetDescription();
}

/// <summary>
/// CommonIconのUtility
/// </summary>
public static class CommonIconUtility
{
    /// <summary>
    /// アイテムタイプからタイプデータを取得する辞書
    /// </summary>
    private static readonly Dictionary<uint, ItemTypeData> itemTypeDatas = new Dictionary<uint, ItemTypeData>
    {
        { (uint)ItemType.Battery,    new BatteryTypeData()    },
        { (uint)ItemType.Barrel,     new BarrelTypeData()     },
        { (uint)ItemType.Bullet,     new BulletTypeData()     },
        { (uint)ItemType.Accessory,  new AccessoryTypeData()  },
        { (uint)ItemType.CannonSet,  new CannonSetTypeData()  },
        { (uint)ItemType.ChargeGem,  new GemTypeData()        },
        { (uint)ItemType.FreeGem,    new GemTypeData()        },
        { (uint)ItemType.Coin,       new CoinTypeData()       },
        { (uint)ItemType.BattleItem, new BattleItemTypeData() },
        { (uint)ItemType.Gear,       new GearTypeData()       },
    };

    /// <summary>
    /// アイテムタイプからアイテム情報を取得する辞書
    /// </summary>
    private static readonly Dictionary<uint, Func<uint, IItemInfo>> itemDatas = new Dictionary<uint, Func<uint, IItemInfo>>
    {
        { (uint)ItemType.Battery,    Masters.BatteryDB.FindById           },
        { (uint)ItemType.Barrel,     Masters.BarrelDB.FindById            },
        { (uint)ItemType.Bullet,     Masters.BulletDB.FindById            },
        { (uint)ItemType.Accessory,  Masters.AccessoriesDB.FindById       },
        { (uint)ItemType.CannonSet,  Masters.CannonSetDB.FindById         },
        { (uint)ItemType.Gear,       Masters.GearDB.FindById              },
        { (uint)ItemType.BattleItem, Masters.BattleItemDB.FindById        },
        { (uint)ItemType.ChargeGem,  itemId => ChargeGemInfo.instance },
        { (uint)ItemType.FreeGem,    itemId => FreeGemInfo.instance   },
        { (uint)ItemType.Coin,       itemId => CoinInfo.instance      },
    };

    /// <summary>
    /// 指定のアイテムタイプ、アイテムIDからスプライトのキーを取得する
    /// </summary>
    public static string GetSpriteKey(uint itemType, uint itemId)
    {
        return itemTypeDatas[itemType].GetSpriteKey(itemId);
    }

    /// <summary>
    /// 指定のアイテムタイプ、キーからスプライトのパスを取得する
    /// </summary>
    public static string GetSpritePath(uint itemType, string key)
    {
        return itemTypeDatas[itemType].GetSpritePath(key);
    }

    /// <summary>
    /// 指定のアイテムタイプ、アイテムIDからスプライトのパスを取得する
    /// </summary>
    public static string GetSpritePath(uint itemType, uint itemId)
    {
        var data = itemTypeDatas[itemType];
        string key = data.GetSpriteKey(itemId);
        return data.GetSpritePath(key);
    }

    /// <summary>
    /// 指定のアイテムタイプ、アイテムIDからレアリティ取得
    /// </summary>
    public static Rank GetRarity(uint itemType, uint itemId)
    {
        return itemTypeDatas[itemType].GetRarity(itemId);
    }

    /// <summary>
    /// 指定ランクの共通素材スプライトを取得する
    /// </summary>
    public static Sprite GetRaritySprite(Rank rank)
    {
        string spriteName = string.Format("ThumbRank_{0}", rank);
        return SharedUI.Instance.commonAtlas.GetSprite(spriteName);
    }

    /// <summary>
    /// 指定ランクの共通背景素材スプライトを取得する
    /// </summary>
    public static Sprite GetRarityBgSprite(Rank rank)
    {
        string spriteName = string.Format("ThumbRankBg_{0}", rank);
        return SharedUI.Instance.commonAtlas.GetSprite(spriteName);
    }

    /// <summary>
    /// 指定ランクの共通フレーム素材スプライトを取得する
    /// </summary>
    public static Sprite GetRarityFrameSprite(Rank rank)
    {
        if (rank == Rank.None)
        {
            return SharedUI.Instance.commonAtlas.GetSprite("ThumbFlame");
        }
        else
        {
            string spriteName = string.Format("ThumbFlame_{0}", rank);
            return SharedUI.Instance.commonAtlas.GetSprite(spriteName);
        }
    }

    /// <summary>
    /// TODO
    /// </summary>
    public static string GetName(uint itemType, uint itemId)
    {
        return itemTypeDatas[itemType].GetName(itemId);
    }
    /// <summary>
    /// TODO
    /// </summary>
    public static string GetDescription(uint itemType, uint itemId)
    {
        return itemTypeDatas[itemType].GetDescription(itemId);
    }

    /// <summary>
    /// ギアタイプ別、ギア背景スプライトを取得する
    /// </summary>
    public static Sprite GetGearBgSprite(uint gearType)
    {   
        string spriteName = string.Format("CmFrm_080_{0}", gearType.ToString().PadLeft(4, '0'));
        return SharedUI.Instance.commonAtlas.GetSprite(spriteName);
    }

    /// <summary>
    /// TODO.ギアタイプ別、ギアメインイメージスプライトを取得する
    /// </summary>
    public static Sprite GetGearMainImageSprite(uint key)
    {
        string spriteName = string.Format("CmMainG_000_{0}", key.ToString().PadLeft(4, '0'));
        return SharedUI.Instance.commonAtlas.GetSprite(spriteName);
    }
    
    /// <summary>
    /// TODO.ギアタイプ別、ギアサーブイメージスプライトを取得する
    /// </summary>
    public static Sprite GetGearSubImageSprite(uint subKey)
    {
        string spriteName = string.Format("CmSubG_000_{0}", subKey.ToString().PadLeft(4, '0'));
        return SharedUI.Instance.commonAtlas.GetSprite(spriteName);
    }

    /// <summary>
    /// アイテムタイプデータ基底
    /// </summary>
    public abstract class ItemTypeData
    {
        public virtual bool isCommonSprite => false;
        public virtual string commonSpriteName => null;
        public virtual string GetSpriteKey(uint id) { return null; }
        public virtual string GetSpritePath(string key) { return null; }
        public virtual Rank GetRarity(uint id) { return Rank.None; }
        // TODO
        public virtual string GetName(uint id) { return null; }
        // TODO
        public virtual string GetDescription(uint id) { return null; }
    }

    /// <summary>
    /// 台座
    /// </summary>
    public class BatteryTypeData : ItemTypeData
    {
        public override string GetSpriteKey(uint id) { return Masters.BatteryDB.FindById(id).key; }
        public override string GetSpritePath(string key) { return SharkDefine.GetBatterySpritePath(key); }
        public override Rank GetRarity(uint id) { return (Rank)Masters.BatteryDB.FindById(id).rarity; }
        // TODO
        public override string GetName(uint id) { return Masters.BatteryDB.FindById(id).name; }
        // TODO
        public override string GetDescription(uint id) { return Masters.BatteryDB.FindById(id).description; }
    }

    /// <summary>
    /// 砲身
    /// </summary>
    public class BarrelTypeData : ItemTypeData
    {
        public override string GetSpriteKey(uint id) { return Masters.BarrelDB.FindById(id).key; }
        public override string GetSpritePath(string key) { return SharkDefine.GetBarrelSpritePath(key); }
        public override Rank GetRarity(uint id) { return (Rank)Masters.BarrelDB.FindById(id).rarity; }
        // TODO
        public override string GetName(uint id) { return Masters.BarrelDB.FindById(id).name; }
        // TODO
        public override string GetDescription(uint id) { return Masters.BatteryDB.FindById(id).description; }
    }

    /// <summary>
    /// 弾丸
    /// </summary>
    public class BulletTypeData : ItemTypeData
    {
        public override string GetSpriteKey(uint id) { return Masters.BulletDB.FindById(id).key; }
        public override string GetSpritePath(string key) { return SharkDefine.GetBulletThumbnailPath(key); }
        public override Rank GetRarity(uint id) { return (Rank)Masters.BulletDB.FindById(id).rarity; }
        // TODO
        public override string GetName(uint id) { return Masters.BatteryDB.FindById(id).name; }
        // TODO
        public override string GetDescription(uint id) { return Masters.BulletDB.FindById(id).description; }
    }

    /// <summary>
    /// アクセサリ
    /// </summary>
    public class AccessoryTypeData : ItemTypeData
    {
        public override string GetSpriteKey(uint id) { return Masters.AccessoriesDB.FindById(id).key; }
        public override string GetSpritePath(string key) { return SharkDefine.GetAccessoryThumbnailPath(key); }
        public override Rank GetRarity(uint id) { return (Rank)Masters.AccessoriesDB.FindById(id).rarity; }
        // TODO
        public override string GetName(uint id) { return Masters.BatteryDB.FindById(id).name; }
        // TODO
        public override string GetDescription(uint id) { return Masters.AccessoriesDB.FindById(id).info; }
    }

    /// <summary>
    /// 砲台
    /// </summary>
    public class CannonSetTypeData : ItemTypeData
    {
        public override string GetSpriteKey(uint id) { return Masters.BatteryDB.FindById(Masters.CannonSetDB.FindById(id).batteryId).key; }
        public override string GetSpritePath(string key) { return SharkDefine.GetTurretSetSpritePath(key); }
    }

    /// <summary>
    /// ジェム
    /// </summary>
    public class GemTypeData : ItemTypeData
    {
        public override bool isCommonSprite => true;
        public override string commonSpriteName => "GemIcon";
        public override string GetName(uint id) => Masters.LocalizeTextDB.Get("Gem");
        public override string GetDescription(uint id) => Masters.LocalizeTextDB.Get("GemDescription");
    }

    /// <summary>
    /// コイン
    /// </summary>
    public class CoinTypeData : ItemTypeData
    {
        public override bool isCommonSprite => true;
        public override string commonSpriteName => "CoinIcon";
        public override string GetName(uint id) => Masters.LocalizeTextDB.Get("Coin");
        public override string GetDescription(uint id) => Masters.LocalizeTextDB.Get("CoinDescription");
    }

    /// <summary>
    /// バトル用アイテム
    /// </summary>
    public class BattleItemTypeData : ItemTypeData
    {
        public override string GetSpriteKey(uint id) { return Masters.BattleItemDB.FindById(id).key; }
        public override string GetSpritePath(string key) { return SharkDefine.GetBattleItemIconSpritePath(key); }
        // TODO
        public override string GetName(uint id) { return Masters.BattleItemDB.FindById(id).name; }
        // TODO
        public override string GetDescription(uint id) { return Masters.BattleItemDB.FindById(id).description; }
    }

    /// <summary>
    /// ギア
    /// </summary>
    public class GearTypeData : ItemTypeData
    {
        public override bool isCommonSprite => true;
        public override Rank GetRarity(uint id) { return (Rank)Masters.GearDB.FindById(id).rarity; }
    }

    /// <summary>
    /// アイテム情報取得
    /// </summary>
    public static IItemInfo GetItemInfo(uint itemType, uint itemId)
    {
        return itemDatas[itemType](itemId);
    }

    /// <summary>
    /// ジェム情報基底
    /// </summary>
    public abstract class GemInfo : IItemInfo
    {
        public abstract ItemType GetItemType();
        bool IItemInfo.IsCommonSprite() => true;
        string IItemInfo.GetSpritePath() => "GemIcon";
        Rank IItemInfo.GetRank() => Rank.None;
        string IItemInfo.GetName() => Masters.LocalizeTextDB.Get("Gem");
        string IItemInfo.GetDescription() => Masters.LocalizeTextDB.Get("GemDescription");
    }

    /// <summary>
    /// 有償ジェム
    /// </summary>
    public class ChargeGemInfo : GemInfo
    {
        public static readonly ChargeGemInfo instance = new ChargeGemInfo();
        public override ItemType GetItemType() => ItemType.ChargeGem;
    }

    /// <summary>
    /// 無償ジェム
    /// </summary>
    public class FreeGemInfo : GemInfo
    {
        public static readonly FreeGemInfo instance = new FreeGemInfo();
        public override ItemType GetItemType() => ItemType.FreeGem;
    }

    /// <summary>
    /// コイン
    /// </summary>
    public class CoinInfo : IItemInfo
    {
        public static readonly CoinInfo instance = new CoinInfo();
        ItemType IItemInfo.GetItemType() => ItemType.Coin;
        bool IItemInfo.IsCommonSprite() => true;
        string IItemInfo.GetSpritePath() => "CoinIcon";
        Rank IItemInfo.GetRank() => Rank.None;
        string IItemInfo.GetName() => Masters.LocalizeTextDB.Get("Coin");
        string IItemInfo.GetDescription() => Masters.LocalizeTextDB.Get("CoinDescription");
    }
}
