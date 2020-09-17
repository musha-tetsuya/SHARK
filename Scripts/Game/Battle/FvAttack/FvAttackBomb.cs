using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Battle {

/// <summary>
/// FVアタック：ボム
/// </summary>
public class FvAttackBomb : FvAttackBase
{
    /// <summary>
    /// 投下位置
    /// </summary>
    private Vector2? dropPosition = null;
    /// <summary>
    /// 最大時間
    /// </summary>
    protected override float maxTime => 3f;
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
        this.stateAction = this.MainState;
    }

    /// <summary>
    /// Run
    /// </summary>
    public override void Run(float deltaTime)
    {
        this.stateAction?.Invoke();
        this.UpdateTime(deltaTime);
    }

    /// <summary>
    /// メインステート
    /// </summary>
    private void MainState()
    {
        //投下位置が決まった or 時間切れの場合
        if (this.dropPosition.HasValue || this.isTimeUp)
        {
            this.TimeUp();

            //投下位置を決めてなかったら
            if (!this.dropPosition.HasValue)
            {
                //自動で投下位置決定
                this.dropPosition = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            }

            //投下位置に銃口を向ける
            this.turret.SetMuzzleDirection(this.dropPosition.Value);

            //ボム弾発射
            var bullet = this.turret.CreateBullet<BombBullet>(this.GetBulletPrefab());
            bullet.transform.up = Vector2.up;
            bullet.bulletBase.landingEffectPrefab = this.turret.turretBase.bulletPrefab.landingEffectPrefab;
            bullet.Setup(
                isFvAttack: true,
                power: this.master.power,
                fvRate: 0,
                bulletData: this.turret.bulletData,
                skill: this.skillGroupManager,
                turret: this.turret,
                dropPosition: BattleGlobal.instance.ScreenToCanvasPoint(this.dropPosition.Value)
            );

            //弾発射通知
            this.turret.OnShoot(bullet);

            //あとはボムに処理を委ねる
            Destroy(this.gameObject);
            this.stateAction = null;
        }
    }

    /// <summary>
    /// 投下位置選択時
    /// </summary>
    public void OnSelectDropPosition(BaseEventData baseEventData)
    {
        this.dropPosition = (baseEventData as PointerEventData).position;
    }

}//class FvAttackBomb

}//namespace Battle