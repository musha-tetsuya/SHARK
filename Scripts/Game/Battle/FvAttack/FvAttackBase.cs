using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Battle {

/// <summary>
/// FVアタック基底
/// </summary>
public abstract class FvAttackBase : MonoBehaviour, ITurretController
{
    /// <summary>
    /// 砲台
    /// </summary>
    protected Turret turret { get; private set; }
    /// <summary>
    /// マスター
    /// </summary>
    protected Master.FvAttackData master { get; private set; }
    /// <summary>
    /// 時間
    /// </summary>
    private float time = 0f;
    /// <summary>
    /// 時間更新時コールバック
    /// </summary>
    public event Action<float, float> onUpdateTime = null;
    /// <summary>
    /// スキル管理
    /// </summary>
    protected SkillGroupManager skillGroupManager = new SkillGroupManager();

    /// <summary>
    /// 最大時間
    /// </summary>
    protected virtual float maxTime
    {
        get { return this.master.time * Masters.MilliSecToSecond; }
    }

    /// <summary>
    /// 時間切れかどうか
    /// </summary>
    protected bool isTimeUp
    {
        get { return this.time >= this.maxTime; }
    }

    /// <summary>
    /// セットアップ
    /// </summary>
    public virtual void Setup(Turret turret, Master.FvAttackData master)
    {
        this.turret = turret;
        this.turret.SetTurretController(this);
        this.master = master;
        this.skillGroupManager.AddSkillGroup(this.master.skillGroupId);
    }

    /// <summary>
    /// OnDestroy
    /// </summary>
    protected virtual void OnDestroy()
    {
        if (this.turret != null)
        {
            this.turret.SetTurretController(null);
            this.turret = null;
        }
    }

    /// <summary>
    /// 弾丸プレハブ取得
    /// </summary>
    protected BulletBase GetBulletPrefab()
    {
        return this.turret.fvaBulletAsset.handle.asset as BulletBase;
    }

    /// <summary>
    /// 時間更新
    /// </summary>
    protected void UpdateTime(float deltaTime)
    {
        this.time += deltaTime;
        this.onUpdateTime?.Invoke(this.time, this.maxTime);
    }

    /// <summary>
    /// 時間切れにする
    /// </summary>
    protected void TimeUp()
    {
        this.time = this.maxTime;
        this.onUpdateTime?.Invoke(this.time, this.maxTime);
    }

    /// <summary>
    /// Run
    /// </summary>
    public abstract void Run(float deltaTime);

}//class FvAttackBase

}//namespace Battle