using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Battle {

/// <summary>
/// 状態異常：死亡
/// </summary>
public sealed class FishConditionDead : FishCondition
{
    /// <summary>
    /// ステート処理
    /// </summary>
    private Action<float> stateAction = null;

    /// <summary>
    /// construct
    /// </summary>
    public FishConditionDead()
        : base(FishConditionType.Dead)
    {
    }

    /// <summary>
    /// construct
    /// </summary>
    public FishConditionDead(System.IO.BinaryReader reader)
        : base(reader)
    {
    }

    /// <summary>
    /// Start
    /// </summary>
    public override void Start()
    {
        base.Start();
        this.manager.fish.RefleshForm();

        //コライダOFFにして、これ以上当たり判定しないようにする
        this.manager.fish.fishCollider2D.polygonCollider.enabled = false;
        //ターゲットマークがあるなら消す
        this.manager.fish.RemoveTargetMark();
        //捕獲成功アニメ再生
        if (this.manager.fish.master.isBoss == 0)
        {
            this.manager.fish.animator.Play("hokakuseikou", 0, 0f);
        }
        else
        {
            this.manager.fish.animator.CrossFadeInFixedTime("hokakuseikou", 2f);
        }
        //アニメ完了待ちステートへ
        this.stateAction = this.WaitAnimationState;
    }

    /// <summary>
    /// Update
    /// </summary>
    public override void Update(float deltaTime)
    {
        this.stateAction?.Invoke(deltaTime);
    }

    /// <summary>
    /// 捕獲成功アニメーション待ちステート
    /// </summary>
    private void WaitAnimationState(float deltaTime)
    {
        this.manager.fish.animator.Update(deltaTime);

        var info = this.manager.fish.animator.GetCurrentAnimatorStateInfo(0);
        if (info.IsName("hokakuseikou") && info.normalizedTime >= 1f)
        {
            this.manager.fish.CallDestroy();
            this.stateAction = null;
        }
    }
}

}