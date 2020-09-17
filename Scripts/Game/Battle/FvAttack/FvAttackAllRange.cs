using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Battle {

/// <summary>
/// FVアタック：全体攻撃
/// </summary>
public class FvAttackAllRange : FvAttackBase
{
    /// <summary>
    /// 最大時間
    /// </summary>
    protected override float maxTime => 1f;
    /// <summary>
    /// ステート処理
    /// </summary>
    private Action stateAction = null;

    /// <summary>
    /// セットアップ
    /// </summary>
    public override void Setup(Turret turret, Master.FvAttackData master)
    {
        base.Setup(turret, master);

        //弾発射
        var bullet = this.turret.CreateBullet<AllRangeBullet>(this.GetBulletPrefab());
        bullet.transform.localPosition = Vector3.zero;
        bullet.transform.up = Vector3.up;
        bullet.bulletBase.landingEffectPrefab = this.turret.turretBase.bulletPrefab.landingEffectPrefab;
        bullet.Setup(true, this.master.power, 0, this.turret.bulletData, this.skillGroupManager, this.turret);

        //弾発射通知
        this.turret.OnShoot(bullet);

        //メインステートへ
        this.stateAction = this.MainState;
    }

    /// <summary>
    /// Run
    /// </summary>
    public override void Run(float deltaTime)
    {
        this.stateAction?.Invoke();
    }

    /// <summary>
    /// メインステート
    /// </summary>
    private void MainState()
    {
        this.TimeUp();
        Destroy(this.gameObject);
        this.stateAction = null;
    }

}//class FvAttackAllRange

}//namespace Battle