using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Battle {

/// <summary>
/// 状態異常：移動速度低下
/// </summary>
public sealed class FishConditionSpeedDown : FishConditionWithTime
{
    /// <summary>
    /// 無限かどうか
    /// </summary>
    private bool isInfinite = false;
    /// <summary>
    /// 魚の移動速度デフォルト値
    /// </summary>
    private float defaultSpeed = 0f;
    /// <summary>
    /// 速度低下率
    /// </summary>
    private uint speedDown = 0;

    /// <summary>
    /// construct
    /// </summary>
    public FishConditionSpeedDown(float time, uint speedDown)
        : base(FishConditionType.SpeedDown, time)
    {
        this.canMove = true;
        this.isInfinite = (this.maxTime <= 0f);
        this.speedDown = speedDown;
    }

    /// <summary>
    /// construct
    /// </summary>
    public FishConditionSpeedDown(System.IO.BinaryReader reader)
        : base(reader)
    {
    }

    /// <summary>
    /// Merge
    /// </summary>
    public override void Merge(FishCondition condition)
    {
        var x = condition as FishConditionSpeedDown;

        //自分の方が効果が高いので上書き不可
        if (this.speedDown > x.speedDown)
        {
            return;
        }

        //低下率が同値の場合
        if (this.speedDown == x.speedDown)
        {
            //自分の方が効果時間が長いので上書き不可
            if (this.maxTime > x.maxTime && !x.isInfinite)
            {
                return;
            }
        }

        //上書き
        base.Merge(condition);
        this.isInfinite = x.isInfinite;
        this.speedDown = x.speedDown;
        this.manager.fish.moveSpeed = this.CalcMoveSpeed();
    }

    /// <summary>
    /// Start
    /// </summary>
    public override void Start()
    {
        base.Start();
        this.defaultSpeed = this.manager.fish.moveSpeed;
        this.manager.fish.moveSpeed = this.CalcMoveSpeed();
        //Debug.LogFormat("移動速度{0}%低下:{1} -> {2}:効果時間{3}秒", this.speedDown, this.defaultSpeed, this.fish.moveSpeed, this.maxTime);
    }

    /// <summary>
    /// End
    /// </summary>
    public override void End()
    {
        base.End();
        this.manager.fish.moveSpeed = this.defaultSpeed;
    }

    /// <summary>
    /// Update
    /// </summary>
    public override void Update(float deltaTime)
    {
        if (!this.isInfinite)
        {
            base.Update(deltaTime);
        }
    }

    /// <summary>
    /// 移動速度計算
    /// </summary>
    private float CalcMoveSpeed()
    {
        return this.defaultSpeed * Mathf.Clamp01((100 - this.speedDown) * Masters.PercentToDecimal);
    }
}

}