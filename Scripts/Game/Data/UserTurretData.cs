using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;

/// <summary>
/// ユーザー砲台データ
/// </summary>
public class UserTurretData
{
    [JsonProperty("id")]
    public uint serverId;
    [JsonProperty("settingNumber")]
    public uint settingNumber;
    [JsonProperty("batteryCannonId")]
    public uint batteryServerId;
    [JsonProperty("barrelCannonId")]
    public uint barrelServerId;
    [JsonProperty("bulletCannonId")]
    public uint bulletServerId;
    [JsonProperty("accessoriesCannonId")]
    public uint accessoryServerId;
    [JsonProperty("useFlg")]
    public uint useFlg;

    //各パーツのマスターデータID
    private uint? m_batteryMasterId = null;
    private uint? m_barrelMasterId = null;
    private uint? m_bulletMasterId = null;
    private uint? m_accessoryMasterId = null;
    public uint batteryMasterId => this.m_batteryMasterId ?? (this.m_batteryMasterId = UserData.Get().batteryData.First(x => x.serverId == this.batteryServerId).itemId).Value;
    public uint barrelMasterId => this.m_barrelMasterId ?? (this.m_barrelMasterId = UserData.Get().barrelData.First(x => x.serverId == this.barrelServerId).itemId).Value;
    public uint bulletMasterId => this.m_bulletMasterId ?? (this.m_bulletMasterId = UserData.Get().bulletData.First(x => x.serverId == this.bulletServerId).itemId).Value;
    public uint accessoryMasterId => this.m_accessoryMasterId ?? (this.m_accessoryMasterId = UserData.Get().accessoriesData.First(x => x.serverId == this.accessoryServerId).itemId).Value;

    /// <summary>
    /// 台座サーバーIDセット
    /// </summary>
    public void SetBatteryServerId(uint serverId)
    {
        this.batteryServerId = serverId;
        this.m_batteryMasterId = null;
    }

    /// <summary>
    /// 砲身サーバーIDセット
    /// </summary>
    public void SetBarrelServerId(uint serverId)
    {
        this.barrelServerId = serverId;
        this.m_barrelMasterId = null;
    }

    /// <summary>
    /// 砲弾サーバーIDセット
    /// </summary>
    public void SetBulletServerId(uint serverId)
    {
        this.bulletServerId = serverId;
        this.m_bulletMasterId = null;
    }

    /// <summary>
    /// アクセサリサーバーIDセット
    /// </summary>
    public void SetAccessoryServerId(uint serverId)
    {
        this.accessoryServerId = serverId;
        this.m_accessoryMasterId = null;
    }

    /// <summary>
    /// まとめてセット
    /// </summary>
    public void Set(UserTurretData obj)
    {
        this.serverId = obj.serverId;
        this.settingNumber = obj.settingNumber;
        this.useFlg = obj.useFlg;
        this.SetBatteryServerId(obj.batteryServerId);
        this.SetBarrelServerId(obj.barrelServerId);
        this.SetBulletServerId(obj.bulletServerId);
        this.SetAccessoryServerId(obj.accessoryServerId);
    }

    /// <summary>
    /// 台座ギアのマスターデータID
    /// </summary>
    public uint[] GetBatteryGearMasterIds()
    {
        return UserData.Get().batteryData.First(x => x.serverId == this.batteryServerId).gearMasterIds;
    }

    /// <summary>
    /// 砲身ギアのマスターデータID
    /// </summary>
    public uint[] GetBarrelGearMasterIds()
    {
        return UserData.Get().barrelData.First(x => x.serverId == this.barrelServerId).gearMasterIds;
    }

    /// <summary>
    /// 砲弾ギアのマスターデータID
    /// </summary>
    public uint[] GetBulletGearMasterIds()
    {
        return UserData.Get().bulletData.First(x => x.serverId == this.bulletServerId).gearMasterIds;
    }

    /// <summary>
    /// 全ギアのマスターデータID
    /// </summary>
    public uint[] GetGearMasterIds()
    {
        return this.GetBatteryGearMasterIds().Concat(
               this.GetBarrelGearMasterIds()).Concat(
               this.GetBulletGearMasterIds()).ToArray();
    }
}

/// <summary>
/// ユーザーパーツデータ
/// </summary>
public abstract class UserPartsData
{
    [JsonProperty("id")]
    public uint serverId;
    //ギアスロット拡張回数
    [JsonProperty("extensionSlot")]
    public uint gearSlotExpandCount;
    // パーツのロック可否
    [JsonProperty("lockFlg")]
    public uint lockFlg;

    //アイテムタイプ・アイテムID・装着可否
    public abstract uint itemType { get; }
    public abstract uint itemId { get; }
    public abstract bool useFlg { get; }

    //ギアのマスターデータID
    private uint[] m_gearMasterIds = null;
    public uint[] gearMasterIds => this.m_gearMasterIds ?? (this.m_gearMasterIds = this.GetGearMasterIds());

    /// <summary>
    /// セットされているギアのマスターデータIDを取得
    /// </summary>
    private uint[] GetGearMasterIds()
    {
        return UserData.Get().gearData
            .Where(gear => gear.partsServerId == this.serverId && gear.gearType == this.itemType)
            .Select(gear => gear.gearId)
            .ToArray();
    }

    /// <summary>
    /// ギアのマスターデータIDをクリア
    /// </summary>
    public void ClearGearMasterIds()
    {
        this.m_gearMasterIds = null;
    }
}

/// <summary>
/// ユーザーパーツデータ：台座
/// </summary>
public class UserBatteryPartsData : UserPartsData
{
    [JsonProperty("batteryId")]
    public uint batteryId;

    //アイテムタイプ・アイテムID
    public override uint itemType => (uint)ItemType.Battery;
    public override uint itemId => this.batteryId;
    public override bool useFlg => this.UseFlg();

    // 装着可否
    private bool UseFlg()
    {
        //return UserData.Get().turretData
            // .Select(x => x.batteryServerId == this.serverId)
            // .First();

        var batteryServerId = UserData.Get().turretData
        .Where(x => x.batteryServerId == this.serverId)
        .Select(x => x.batteryServerId)
        .FirstOrDefault();

        return batteryServerId == this.serverId;
    }
}

/// <summary>
/// ユーザーパーツデータ：砲身
/// </summary>
public class UserBarrelPartsData : UserPartsData
{
    [JsonProperty("barrelId")]
    public uint barrelId;

    //アイテムタイプ・アイテムID
    public override uint itemType => (uint)ItemType.Barrel;
    public override uint itemId => this.barrelId;
    public override bool useFlg => this.UseFlg();

    // 装着可否
    private bool UseFlg()
    {
        // return UserData.Get().turretData
        //     .Select(x => x.barrelServerId == this.serverId)
        //     .First();

        var barrelServerId = UserData.Get().turretData
        .Where(x => x.barrelServerId == this.serverId)
        .Select(x => x.barrelServerId)
        .FirstOrDefault();

        return barrelServerId == this.serverId;
    }
}

/// <summary>
/// ユーザーパーツデータ：砲弾
/// </summary>
public class UserBulletPartsData : UserPartsData
{
    [JsonProperty("bulletId")]
    public uint bulletId;

    //アイテムタイプ・アイテムID
    public override uint itemType => (uint)ItemType.Bullet;
    public override uint itemId => this.bulletId;
    public override bool useFlg => this.UseFlg();

    // 装着可否
    private bool UseFlg()
    {
        // return UserData.Get().turretData
        //     .Select(x => x.bulletServerId == this.serverId)
        //     .First();

        var bulletServerId = UserData.Get().turretData
        .Where(x => x.bulletServerId == this.serverId)
        .Select(x => x.bulletServerId)
        .FirstOrDefault();

        return bulletServerId == this.serverId;
    }
}

/// <summary>
/// ユーザーパーツデータ：アクセサリ
/// </summary>
public class UserAccessoryPartsData : UserPartsData
{
    [JsonProperty("accessoriesId")]
    public uint accessoriesId;

    //アイテムタイプ・アイテムID
    public override uint itemType => (uint)ItemType.Accessory;
    public override uint itemId => this.accessoriesId;
    public override bool useFlg => this.UseFlg();

    // 装着可否
    private bool UseFlg()
    {
        return UserData.Get().turretData
            .Select(x => x.accessoryServerId == this.serverId)
            .First();
    }
}

/// <summary>
/// ユーザーギアデータ
/// </summary>
public class UserGearData
{
    [JsonProperty("id")]
    public uint serverId;
    [JsonProperty("gearId")]
    public uint gearId;
    [JsonProperty("gearType")]
    public uint gearType;
    [JsonProperty("setId")]
    public uint? partsServerId;
    [JsonProperty("setDatetime")]
    public DateTime? setDateTime;
    [JsonProperty("lockFlg")]
    public uint lockFlg;

    /// <summary>
    /// パーツサーバーIDセット
    /// </summary>
    public void SetPartsServerId(uint? newId, DateTime? setDateTime)
    {
        //タイプ別パーツデータリスト
        var partsDataList = (this.gearType == (int)GearType.Battery) ? UserData.Get().batteryData
                          : (this.gearType == (int)GearType.Barrel)  ? UserData.Get().barrelData
                          : (this.gearType == (int)GearType.Bullet)  ? UserData.Get().bulletData
                          : null;

        if (this.partsServerId.HasValue)
        {
            //古いパーツのギア情報のクリア
            partsDataList.First(x => x.serverId == this.partsServerId).ClearGearMasterIds();
        }

        if (newId.HasValue)
        {
            //新しいパーツのギア情報のクリア
            partsDataList.First(x => x.serverId == newId).ClearGearMasterIds();
        }

        //パーツサーバーIDセット
        this.partsServerId = newId;
        this.setDateTime = setDateTime;
    }
}