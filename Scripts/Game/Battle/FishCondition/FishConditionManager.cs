using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;

namespace Battle {

/// <summary>
/// 魚の状態異常管理
/// </summary>
public class FishConditionManager : IBinary
{
    /// <summary>
    /// 魚
    /// </summary>
    public Fish fish { get; private set; }
    /// <summary>
    /// 状態異常リスト
    /// </summary>
    private List<FishCondition> conditionList = new List<FishCondition>();
    /// <summary>
    /// 指定の状態異常にかかっているときBitが立つ
    /// </summary>
    private BitVector32 hasConditionBit = new BitVector32();
    /// <summary>
    /// 指定の状態異常により動けないときBitが立つ
    /// </summary>
    private BitVector32 cantMoveBit = new BitVector32();
    /// <summary>
    /// 状態異常処理
    /// </summary>
    private event Action<float> conditionAction = null;

    /// <summary>
    /// construct
    /// </summary>
    public FishConditionManager(Fish fish)
    {
        this.fish = fish;
    }

    /// <summary>
    /// Update
    /// </summary>
    public void Update(float deltaTime)
    {
        this.conditionAction?.Invoke(deltaTime);
    }

    /// <summary>
    /// 状態異常追加
    /// </summary>
    public void Add(FishCondition condition)
    {
        //死んでいるので追加出来ない
        if (this.HasCondition(FishConditionType.Dead))
        {
            return;
        }

        //既に同じ状態異常にかかっているので可能なら上書き
        if (this.HasCondition(condition.type))
        {
            this.GetCondition(condition.type).Merge(condition);
            return;
        }

        //状態異常追加
        this.conditionList.Add(condition);
        this.hasConditionBit[1 << (int)condition.type] = true;
        this.cantMoveBit[1 << (int)condition.type] = !condition.canMove;
        this.conditionAction += condition.Update;

        //状態異常開始
        condition.manager = this;
        condition.onEnd += this.Remove;
        condition.Start();
    }

    /// <summary>
    /// 状態異常除去
    /// </summary>
    private void Remove(FishCondition condition)
    {
        this.conditionList.Remove(condition);
        this.hasConditionBit[1 << (int)condition.type] = false;
        this.cantMoveBit[1 << (int)condition.type] = false;
        this.conditionAction -= condition.Update;
    }

    /// <summary>
    /// 指定の状態異常にかかっているかどうか
    /// </summary>
    public bool HasCondition(FishConditionType type)
    {
        return this.hasConditionBit[1 << (int)type];
    }

    /// <summary>
    /// 指定の状態異常を取得する
    /// </summary>
    public FishCondition GetCondition(FishConditionType type)
    {
        return this.conditionList.Find(condition => condition.type == type);
    }

    /// <summary>
    /// 動けるかどうか
    /// </summary>
    public bool canMove
    {
        get { return this.cantMoveBit.Data == 0; }
    }

    /// <summary>
    /// バイナリデータ書き出し
    /// </summary>
    void IBinary.Write(System.IO.BinaryWriter writer)
    {
        writer.Write(this.conditionList.Count);
        for (int i = 0; i < this.conditionList.Count; i++)
        {
            writer.Write(this.conditionList[i].GetType().FullName);
            (this.conditionList[i] as IBinary).Write(writer);
        }
    }

    /// <summary>
    /// バイナリデータ読み込み
    /// </summary>
    void IBinary.Read(System.IO.BinaryReader reader)
    {
        int count = reader.ReadInt32();
        for (int i = 0; i < count; i++)
        {
            var type = Type.GetType(reader.ReadString());
            var condition = Activator.CreateInstance(type, reader) as FishCondition;
            this.Add(condition);
        }
    }
}

}