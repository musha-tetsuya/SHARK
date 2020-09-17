using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Battle {

/// <summary>
/// FVアタック：レーザービーム
/// </summary>
public class FvAttackLaserBeam : FvAttackBase
{
    /// <summary>
    /// 弾丸
    /// </summary>
    private LaserBeamBullet bullet = null;
    /// <summary>
    /// 弾丸のTransform
    /// </summary>
    private RectTransform bulletTransform = null;
    /// <summary>
    /// コントローラ
    /// </summary>
    private Controller controller = new Controller();
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

        //弾発射
        this.bullet = this.turret.CreateBullet<LaserBeamBullet>(this.GetBulletPrefab());
        this.bulletTransform = this.bullet.transform as RectTransform;
        this.bullet.bulletBase.landingEffectPrefab = this.turret.turretBase.bulletPrefab.landingEffectPrefab;
        this.bullet.Setup(true, this.master.power, 0, turret.bulletData, this.skillGroupManager);

        //弾発射通知
        this.turret.OnShoot(this.bullet);

        //制御開始
        this.controller.Start(false, this.turret, this.bullet.bulletBase, this.OnFinished);

        //メインステートへ
        this.stateAction = this.MainState;
    }

    /// <summary>
    /// OnDestroy
    /// </summary>
    protected override void OnDestroy()
    {
        if (this.bullet != null && this.bullet.gameObject != null)
        {
            Destroy(this.bullet.gameObject);
            this.bullet = null;
        }
        base.OnDestroy();
    }

    /// <summary>
    /// Run
    /// </summary>
    public override void Run(float deltaTime)
    {
        if (BattleGlobal.instance.turretEventTrigger.isTouch)
        {
            //タッチ位置に銃口を向ける
            this.turret.UpdateMuzzleDirection();
            this.bulletTransform.position = this.turret.turretBase.muzzle.position;
            this.bulletTransform.up = this.turret.turretBase.muzzle.up;
        }

        this.controller?.Update(deltaTime);

        this.stateAction?.Invoke(deltaTime);
    }

    /// <summary>
    /// メインステート
    /// </summary>
    private void MainState(float deltaTime)
    {
        if (this.isTimeUp)
        {
            //時間切れなのでレーザービーム終了へ
            this.controller.SetOut();
            this.stateAction = null;
        }

        this.UpdateTime(deltaTime);
    }

    /// <summary>
    /// 制御終了時
    /// </summary>
    private void OnFinished()
    {
        this.controller = null;
        Destroy(this.gameObject);
    }

    /// <summary>
    /// コントローラ
    /// </summary>
    public class Controller : IFVAController
    {
        /// <summary>
        /// 終了フラグ
        /// </summary>
        public bool isFinished { get; private set; }
        /// <summary>
        /// 自動時間切れフラグ
        /// </summary>
        private bool isAutoTimeUp = false;
        /// <summary>
        /// 寿命
        /// </summary>
        private float duration = 0f;
        /// <summary>
        /// 砲台
        /// </summary>
        private Turret turret = null;
        /// <summary>
        /// 弾丸
        /// </summary>
        private BulletBase bulletBase = null;
        /// <summary>
        /// 終了時コールバック
        /// </summary>
        private Action onFinished = null;

        /// <summary>
        /// 破棄
        /// </summary>
        public void Delete()
        {
            this.isFinished = true;
            Destroy(this.bulletBase.gameObject);
        }

        /// <summary>
        /// Start
        /// </summary>
        public void Start(bool isAutoTimeUp, Turret turret, BulletBase bulletBase, Action onFinished)
        {
            this.isAutoTimeUp = isAutoTimeUp;
            this.turret = turret;
            this.duration = this.turret.fvAttackData.time * Masters.MilliSecToSecond;
            this.bulletBase = bulletBase;
            this.onFinished = onFinished;

            //波動砲射出アニメーション
            this.turret.PlayLaserBeamAnimation();
            SoundManager.Instance.PlaySe(SeName.FVATTACK_LASERBEAM);

            //弾丸アニメーション更新は手動で行う
            this.bulletBase.animator.enabled = false;
            this.bulletBase.animator.Update(0);
            this.bulletBase.GetComponent<AnimationEventReceiver>().onFinished = this.OnFinishedAnimation;
        }

        /// <summary>
        /// Update
        /// </summary>
        public void Update(float deltaTime)
        {
            this.bulletBase.animator.Update(deltaTime);

            if (this.isAutoTimeUp)
            {
                if (this.duration <= 0f)
                {
                    //時間切れなのでOutアニメへ
                    this.SetOut();
                    this.isAutoTimeUp = false;
                }

                this.duration -= deltaTime;
            }
        }

        /// <summary>
        /// Outアニメーション開始
        /// </summary>
        public void SetOut()
        {
            //波動砲アニメーション終了
            this.turret.EndLaserBeamAnimation();
            this.bulletBase.animator.SetTrigger("out");
            this.bulletBase.bulletCollider.enabled = false;
        }

        /// <summary>
        /// アニメーション終了時
        /// </summary>
        private void OnFinishedAnimation(string tag)
        {
            this.onFinished?.Invoke();
            this.Delete();
        }
    }

}//class FvAttackLaserBeam

}//namespace Battle