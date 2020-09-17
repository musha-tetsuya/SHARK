using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPropertyKey
{
    public const string Coin = "Coin";
    public const string Bet = "Bet";
    public const string Turret = "Turret";
    public const string BulletList = "BulletList";
}

public class RoomPropertyKey
{
    public const string Sender = "Sender";
    public const string PositionIds = "PositionIds";
}

public enum MultiEventCode
{
    Init,
    CreateLandingEffect,
    CreateCoinEffect,
    FishDamaged,
    ChangeFishCondition,
    Summon,
    ShootLaserBeam,
    ShootBomb,
    ShootAllRange,
    CreateFVAPenetrationChargeEffect,
    Length
}

/// <summary>
/// 砲台データ
/// </summary>
public class TurretDto : IBinary
{
    public uint batteryId;
    public uint barrelId;
    public uint bulletId;

    void IBinary.Write(System.IO.BinaryWriter writer)
    {
        writer.Write(this.batteryId);
        writer.Write(this.barrelId);
        writer.Write(this.bulletId);
    }

    void IBinary.Read(System.IO.BinaryReader reader)
    {
        this.batteryId = reader.ReadUInt32();
        this.barrelId = reader.ReadUInt32();
        this.bulletId = reader.ReadUInt32();
    }
}

/// <summary>
/// 弾丸データ
/// </summary>
public class BulletDto : IBinary
{
    public BulletBase bulletBase;
    public bool isNormalBullet;
    public int id;
    public int timeStamp;
    public uint speed;
    public Vector3 bulletLocalPosition;
    public Vector3 bulletLocalEulerAngles;
    public Vector3 barrelLocalEulerAngles;

    void IBinary.Write(System.IO.BinaryWriter writer)
    {
        writer.Write(this.isNormalBullet);
        writer.Write(this.id);
        writer.Write(this.timeStamp);
        writer.Write(this.speed);
        writer.Write(this.bulletLocalPosition);
        writer.Write(this.bulletLocalEulerAngles);
        writer.Write(this.barrelLocalEulerAngles);
    }

    void IBinary.Read(System.IO.BinaryReader reader)
    {
        this.isNormalBullet = reader.ReadBoolean();
        this.id = reader.ReadInt32();
        this.timeStamp = reader.ReadInt32();
        this.speed = reader.ReadUInt32();
        this.bulletLocalPosition = reader.ReadVector3();
        this.bulletLocalEulerAngles = reader.ReadVector3();
        this.barrelLocalEulerAngles = reader.ReadVector3();
    }
}

/// <summary>
/// 範囲ボム弾データ
/// </summary>
public class BombBulletDto : IBinary
{
    public int timeStamp;
    public Vector2 dropPosition;
    public Vector3 barrelLocalEulerAngles;

    void IBinary.Write(System.IO.BinaryWriter writer)
    {
        writer.Write(this.timeStamp);
        writer.Write(this.dropPosition);
        writer.Write(this.barrelLocalEulerAngles);
    }

    void IBinary.Read(System.IO.BinaryReader reader)
    {
        this.timeStamp = reader.ReadInt32();
        this.dropPosition = reader.ReadVector2();
        this.barrelLocalEulerAngles = reader.ReadVector3();
    }
}

/// <summary>
/// 着弾エフェクトデータ
/// </summary>
public class LandingEffectDto : IBinary
{
    public Vector3 localPosition;

    void IBinary.Write(System.IO.BinaryWriter writer)
    {
        writer.Write(this.localPosition);
    }

    void IBinary.Read(System.IO.BinaryReader reader)
    {
        this.localPosition = reader.ReadVector3();
    }
}

/// <summary>
/// コインエフェクトデータ
/// </summary>
public class CoinEffectDto : IBinary
{
    public bool isSmall;
    public int getCoin;
    public Vector2 position;

    void IBinary.Write(System.IO.BinaryWriter writer)
    {
        writer.Write(this.isSmall);
        writer.Write(this.getCoin);
        writer.Write(this.position);
    }

    void IBinary.Read(System.IO.BinaryReader reader)
    {
        this.isSmall = reader.ReadBoolean();
        this.getCoin = reader.ReadInt32();
        this.position = reader.ReadVector2();
    }
}

/// <summary>
/// 魚の復帰データ
/// </summary>
public class FishResumeDto : IBinary
{
    public float elapsedTime;
    public int hp;

    public float rotationTimeCount;
    public Vector3 localPosition;
    public Vector3 forward;

    public int remainRouteCount;
    public bool hasTargetPosition;

    public FishConditionDto conditionDto;

    void IBinary.Write(System.IO.BinaryWriter writer)
    {
        writer.Write(this.elapsedTime);
        writer.Write(this.hp);

        writer.Write(this.rotationTimeCount);
        writer.Write(this.localPosition);
        writer.Write(this.forward);

        writer.Write(this.remainRouteCount);
        writer.Write(this.hasTargetPosition);

        (this.conditionDto as IBinary).Write(writer);
    }

    void IBinary.Read(System.IO.BinaryReader reader)
    {
        this.elapsedTime = reader.ReadSingle();
        this.hp = reader.ReadInt32();

        this.rotationTimeCount = reader.ReadSingle();
        this.localPosition = reader.ReadVector3();
        this.forward = reader.ReadVector3();

        this.remainRouteCount = reader.ReadInt32();
        this.hasTargetPosition = reader.ReadBoolean();

        this.conditionDto = new FishConditionDto();
        (this.conditionDto as IBinary).Read(reader);
    }
}

/// <summary>
/// 魚へのデータ
/// </summary>
public abstract class IFishDto : IBinary
{
    public Battle.Fish.ID id;

    public abstract void Set(Battle.Fish fish);

    public virtual void Write(System.IO.BinaryWriter writer)
    {
        this.id.Write(writer);
    }

    public virtual void Read(System.IO.BinaryReader reader)
    {
        this.id.Read(reader);
    }
}

/// <summary>
/// 魚へのダメージデータ
/// </summary>
public class FishDamagedDto : IFishDto
{
    public int damage;

    public override void Set(Battle.Fish fish)
    {
        fish.SetDamagedDto(this);
    }

    public override void Write(System.IO.BinaryWriter writer)
    {
        base.Write(writer);
        writer.Write(this.damage);
    }

    public override void Read(System.IO.BinaryReader reader)
    {
        base.Read(reader);
        this.damage = reader.ReadInt32();
    }
}

/// <summary>
/// 魚の状態データ
/// </summary>
public class FishConditionDto : IFishDto
{
    public float time;
    public byte[] conditionBytes;

    public override void Set(Battle.Fish fish)
    {
        fish.SetConditionDto(this);
    }

    public override void Write(System.IO.BinaryWriter writer)
    {
        base.Write(writer);
        writer.Write(this.time);
        writer.Write(this.conditionBytes.Length);
        writer.Write(this.conditionBytes);
    }

    public override void Read(System.IO.BinaryReader reader)
    {
        base.Read(reader);
        this.time = reader.ReadSingle();
        this.conditionBytes = reader.ReadBytes(reader.ReadInt32());
    }
}

/// <summary>
/// 魚召喚データ
/// </summary>
public class SummonDto : IBinary
{
    public Battle.Fish.ID id;
    public int timeStamp;
    public byte keyNo;
    public byte fishNo;
    public byte routeNo;

    void IBinary.Write(System.IO.BinaryWriter writer)
    {
        this.id.Write(writer);
        writer.Write(this.timeStamp);
        writer.Write(this.keyNo);
        writer.Write(this.fishNo);
        writer.Write(this.routeNo);
    }

    void IBinary.Read(System.IO.BinaryReader reader)
    {
        this.id.Read(reader);
        this.timeStamp = reader.ReadInt32();
        this.keyNo = reader.ReadByte();
        this.fishNo = reader.ReadByte();
        this.routeNo = reader.ReadByte();
    }
}

/// <summary>
/// Wave復帰データ
/// </summary>
public class WaveResumeDto : IBinary
{
    public int timeStamp;
    public byte[] waveBytes;
    public byte[] summonBytes;

    void IBinary.Write(System.IO.BinaryWriter writer)
    {
        writer.Write(this.timeStamp);
        writer.Write(this.waveBytes.Length);
        writer.Write(this.waveBytes);
        writer.Write(this.summonBytes.Length);
        writer.Write(this.summonBytes);
    }

    void IBinary.Read(System.IO.BinaryReader reader)
    {
        this.timeStamp = reader.ReadInt32();
        this.waveBytes = reader.ReadBytes(reader.ReadInt32());
        this.summonBytes = reader.ReadBytes(reader.ReadInt32());
    }
}
