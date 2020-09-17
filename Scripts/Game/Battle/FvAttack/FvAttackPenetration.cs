using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Battle {

/// <summary>
/// FVアタック：貫通跳弾
/// </summary>
public class FvAttackPenetration : FvAttackBase
{
    /// <summary>
    /// チャージエフェクト時間
    /// </summary>
    private float chargeEffectTime = 0f;
    /// <summary>
    /// ステート処理
    /// </summary>
    private Action<float> stateAction = null;

    /// <summary>
    /// セットアップ
    /// </summary>
    public override void Setup(Turret turret, Master.FvAttackData master)
    {
        base.Setup(turret, master);

        //チャージエフェクト生成
        var chargeEffect = this.turret.CreateFVAPenetrationChargeEffect(true);

        //チャージエフェクト時間
        this.chargeEffectTime = chargeEffect.GetComponent<ParticleSystem>().main.duration;

        //メインステートへ
        this.stateAction = this.MainState;
    }

    /// <summary>
    /// Run
    /// </summary>
    public override void Run(float deltaTime)
    {
        this.stateAction?.Invoke(deltaTime);
    }

    /// <summary>
    /// メインステート
    /// </summary>
    private void MainState(float deltaTime)
    {
        if (this.chargeEffectTime <= 0f)
        {
            //弾発射
            var bullet = this.turret.CreateBullet<PenetrationBullet>(this.GetBulletPrefab());
            bullet.bulletBase.landingEffectPrefab = this.turret.turretBase.bulletPrefab.landingEffectPrefab;
            bullet.Setup(
                isFvAttack: true,
                power: this.master.power,
                fvRate: 0,
                bulletData: this.turret.bulletData,
                skill: this.skillGroupManager,
                speed: this.turret.speed,
                duration: this.maxTime
            );
            this.turret.PlayBulletFiringAnimation();
            SoundManager.Instance.PlaySe(SeName.FVATTACK_PENETRATE);

            //弾発射通知
            this.turret.OnShoot(bullet);

            //終了
            this.TimeUp();
            Destroy(this.gameObject);
            this.stateAction = null;
        }

        this.chargeEffectTime -= deltaTime;
    }

}//class FvAttackPenetration

}//namespace Battle