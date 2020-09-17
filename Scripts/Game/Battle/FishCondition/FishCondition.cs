using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;

namespace Battle {

/// <summary>
/// 魚の状態異常基本クラス
/// </summary>
public class FishCondition : IBinary
{
    /// <summary>
    /// 状態異常タイプ
    /// </summary>
    public FishConditionType type { get; private set; }
    /// <summary>
    /// マネージャー
    /// </summary>
    public FishConditionManager manager = null;
    /// <summary>
    /// 動けるかどうか
    /// </summary>
    public bool canMove { get; protected set; }
    /// <summary>
    /// 終了時コールバック
    /// </summary>
    public Action<FishCondition> onEnd = null;

    /// <summary>
    /// construct
    /// </summary>
    public FishCondition(FishConditionType type)
    {
        this.type = type;
    }

    /// <summary>
    /// construct
    /// </summary>
    public FishCondition(System.IO.BinaryReader reader)
    {
        this.Read(reader);
    }

    /// <summary>
    /// Merge
    /// </summary>
    public virtual void Merge(FishCondition condition){}

    /// <summary>
    /// Start
    /// </summary>
    public virtual void Start(){}

    /// <summary>
    /// End
    /// </summary>
    public virtual void End()
    {
        this.onEnd?.Invoke(this);
    }

    /// <summary>
    /// Update
    /// </summary>
    public virtual void Update(float deltaTime){}

    /// <summary>
    /// バイナリデータ書き出し
    /// </summary>
    public virtual void Write(System.IO.BinaryWriter writer)
    {
        writer.Write((int)this.type);
        writer.Write(this.canMove);
    }

    /// <summary>
    /// バイナリデータ読み込み
    /// </summary>
    public virtual void Read(System.IO.BinaryReader reader)
    {
        this.type = (FishConditionType)reader.ReadInt32();
        this.canMove = reader.ReadBoolean();
    }
}

/// <summary>
/// 効果時間付き状態異常基底
/// </summary>
public class FishConditionWithTime : FishCondition
{
    /// <summary>
    /// 最大効果時間
    /// </summary>
    protected float maxTime = 0f;
    /// <summary>
    /// 効果時間
    /// </summary>
    protected float time = 0f;

    /// <summary>
    /// construct
    /// </summary>
    public FishConditionWithTime(FishConditionType type, float time)
        : base(type)
    {
        this.maxTime = time;
        this.time = time;
    }

    /// <summary>
    /// construct
    /// </summary>
    public FishConditionWithTime(System.IO.BinaryReader reader)
        : base(reader)
    {
    }

    /// <summary>
    /// Merge
    /// </summary>
    public override void Merge(FishCondition condition)
    {
        var x = condition as FishConditionWithTime;
        this.maxTime = x.maxTime;
        this.time = x.time;
    }

    /// <summary>
    /// Update
    /// </summary>
    public override void Update(float deltaTime)
    {
        if (this.time < 0f)
        {
            this.End();
        }

        this.time -= deltaTime;
    }

    /// <summary>
    /// バイナリデータ書き出し
    /// </summary>
    public override void Write(System.IO.BinaryWriter writer)
    {
        base.Write(writer);
        writer.Write(this.maxTime);
        writer.Write(this.time);
    }

    /// <summary>
    /// バイナリデータ読み込み
    /// </summary>
    public override void Read(System.IO.BinaryReader reader)
    {
        base.Read(reader);
        this.maxTime = reader.ReadSingle();
        this.time = reader.ReadSingle();
    }
}

}