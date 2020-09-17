using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Battle {

/// <summary>
/// 状態異常：召喚
/// </summary>
public class FishConditionSummon : FishConditionWithTime
{
    /// <summary>
    /// construct
    /// </summary>
    public FishConditionSummon(float time)
        : base(FishConditionType.Summon, time)
    {
    }

    /// <summary>
    /// construct
    /// </summary>
    public FishConditionSummon(System.IO.BinaryReader reader)
        : base(reader)
    {
    }

    /// <summary>
    /// Start
    /// </summary>
    public override void Start()
    {
        base.Start();
        this.manager.fish.model.SetActive(false);
        this.manager.fish.fishCollider2D.polygonCollider.enabled = false;
    }

    /// <summary>
    /// End
    /// </summary>
    public override void End()
    {
        base.End();
        this.manager.fish.model.SetActive(true);
        this.manager.fish.fishCollider2D.polygonCollider.enabled = true;
    }
}

}