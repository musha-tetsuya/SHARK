using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Battle {

/// <summary>
/// 全体弾
/// </summary>
public class AllRangeBullet : Bullet
{
    /// <summary>
    /// サーバータイムスタンプ
    /// </summary>
    [NonSerialized]
    public int timeStamp = 0;
    /// <summary>
    /// コントローラ
    /// </summary>
    private Controller controller = new Controller();

    /// <summary>
    /// セットアップ
    /// </summary>
    public void Setup(
        bool isFvAttack,
        uint power,
        uint fvRate,
        Master.BulletData bulletData,
        SkillGroupManager skill,
        Turret turret)
    {
        base.Setup(isFvAttack, power, fvRate, bulletData, skill);

        //タイムスタンプ取得
        this.timeStamp = BattleGlobal.GetTimeStamp();

        //制御開始
        this.controller.Start(turret, this.bulletBase, this.OnHit, this.OnFinished);
    }

    /// <summary>
    /// Update
    /// </summary>
    private void Update()
    {
        float deltaTime = BattleGlobal.GetDeltaTime(ref timeStamp);
        int loopCount = (int)(deltaTime / SharkDefine.DELTATIME);

        for (int i = 0; i < loopCount + 1; i++)
        {
            float _deltaTime = Mathf.Min(deltaTime, SharkDefine.DELTATIME);
            deltaTime -= _deltaTime;

            this.controller?.Update(_deltaTime);
        }
    }

    /// <summary>
    /// ヒット時
    /// </summary>
    protected override void OnHit(FishCollider2D fishCollider2D){}

    /// <summary>
    /// ヒット時
    /// </summary>
    private void OnHit()
    {
        var fishList = BattleGlobal.instance.fishList.ToArray();

        //全ての魚に対して
        foreach (var fish in fishList)
        {
            //生きてるなら
            if (!fish.isDead && fish.fishCollider2D.IsInScreen())
            {
                //着弾エフェクト再生
                this.CreateLandingEffect(fish.fishCollider2D.rectTransform.position);

                //魚にダメージ
                fish.OnDamaged(this);
            }
        }
    }

    /// <summary>
    /// 制御終了時
    /// </summary>
    private void OnFinished()
    {
        this.controller = null;
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
        /// 砲台
        /// </summary>
        private Turret turret = null;
        /// <summary>
        /// 弾丸
        /// </summary>
        private BulletBase bulletBase = null;
        /// <summary>
        /// パーティクル制御
        /// </summary>
        private ParticleController particleController = null;
        /// <summary>
        /// ヒット時コールバック
        /// </summary>
        private Action onHit = null;
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
            this.particleController = null;
            Destroy(this.bulletBase.gameObject);
        }

        /// <summary>
        /// Start
        /// </summary>
        public void Start(Turret turret, BulletBase bulletBase, Action onHit, Action onFinished)
        {
            this.turret = turret;
            this.bulletBase = bulletBase;
            this.particleController = this.bulletBase.transform.GetChild(0).GetComponent<ParticleController>();
            this.onHit = onHit;
            this.onFinished = onFinished;

            //射出アニメーション
            this.turret.PlayBulletFiringAnimation();
            SoundManager.Instance.PlaySe(SeName.FVATTACK_ALLRANGE);

            //パーティクル終了時コールバック登録
            this.particleController.onParticleSystemStopped = this.OnParticleSystemStopped;
        }

        /// <summary>
        /// Update
        /// </summary>
        public void Update(float deltaTime)
        {
            this.particleController?.Run(deltaTime);
        }

        /// <summary>
        /// アニメーション終了時
        /// </summary>
        private void OnParticleSystemStopped(string key)
        {
            //フラッシュパーティクル終了時
            if (key == "Flash")
            {
                //ヒット通知
                this.onHit?.Invoke();
            }
            //メインパーティクル終了時
            else if (key == "Main")
            {
                //終了通知
                this.onFinished?.Invoke();
                this.Delete();
            }
        }
    }

}//class AllRangeBullet

}//namespace Battle