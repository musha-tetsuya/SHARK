using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Battle {

/// <summary>
/// 状態異常：氷結
/// </summary>
public sealed class FishConditionFreezing : FishConditionWithTime
{
    /// <summary>
    /// construct
    /// </summary>
    public FishConditionFreezing(float time)
        : base(FishConditionType.Freezing, time)
    {
    }

    /// <summary>
    /// construct
    /// </summary>
    public FishConditionFreezing(System.IO.BinaryReader reader)
        : base(reader)
    {
    }

    /// <summary>
    /// Merge
    /// </summary>
    public override void Merge(FishCondition condition)
    {
        var x = condition as FishConditionFreezing;
        
        //自分の方が効果時間が長いので上書き不可
        if (this.time >= x.time)
        {
            return;
        }

        //上書き
        base.Merge(condition);
    }

    /// <summary>
    /// Start
    /// </summary>
    public override void Start()
    {
        base.Start();
        this.manager.fish.RefleshForm();
    }

    /// <summary>
    /// End
    /// </summary>
    public override void End()
    {
        base.End();
        this.manager.fish.RefleshForm();
    }
}

}