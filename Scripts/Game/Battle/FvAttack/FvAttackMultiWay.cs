using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Battle {

/// <summary>
/// FVアタック：多方面
/// </summary>
public class FvAttackMultiWay : FvAttackBase
{
    /// <summary>
    /// 発射角度
    /// </summary>
    private Transform[] muzzles = null;
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
        var root = this.GetBulletPrefab().transform;
        this.muzzles = new Transform[root.childCount];
        for (int i = 0; i < this.muzzles.Length; i++)
        {
            this.muzzles[i] = root.GetChild(i);
        }

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
        if (BattleGlobal.instance.turretEventTrigger.isTouch)
        {
            //タッチ位置に銃口を向ける
            this.turret.UpdateMuzzleDirection();

            //可能なら弾発射
            NormalBullet[] bullets = null;
            if (this.turret.CreateNormalBullet(out bullets, this.muzzles.Length))
            {
                for (int i = 0; i < this.muzzles.Length; i++)
                {
                    bullets[i].transform.up = Quaternion.Euler(this.muzzles[i].localEulerAngles) * bullets[i].transform.up;
                    bullets[i].Setup(
                        isFvAttack: true,
                        power: this.turret.power,
                        fvRate: 0,
                        bulletData: this.turret.bulletData,
                        skill: this.skillGroupManager,
                        speed: this.turret.speed
                    );
                    //弾発射通知
                    this.turret.OnShoot(bullets[i]);
                }
                this.turret.PlayBulletFiringAnimation();
                this.turret.ResetIntervalCount();
                SoundManager.Instance.PlaySe(this.turret.barrelData.seName);
            }
        }

        if (this.isTimeUp)
        {
            //寿命尽きたら自動で消滅
            Destroy(this.gameObject);
            this.stateAction = null;
        }

        this.UpdateTime(deltaTime);
    }

}//class FvAttackMultiWay

}//namespace Battle