using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Battle {

/// <summary>
/// ボム弾
/// </summary>
public class BombBullet : Bullet
{
    /// <summary>
    /// サーバータイムスタンプ
    /// </summary>
    [NonSerialized]
    public int timeStamp = 0;
    /// <summary>
    /// 投下位置
    /// </summary>
    public Vector2 dropPosition { get; private set; }
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
        Turret turret,
        Vector2 dropPosition)
    {
        base.Setup(isFvAttack, power, fvRate, bulletData, skill);

        this.timeStamp = BattleGlobal.GetTimeStamp();
        this.dropPosition = dropPosition;

        //制御開始
        this.controller.Start(turret, this.bulletBase, dropPosition, this, this.OnFinished);
    }

    /// <summary>
    /// Update
    /// </summary>
    private void Update()
    {
        //Stayイベントは1フレームだけしか聞かない
        this.bulletBase.bulletCollider.stayEventReceiver = null;

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
    protected override void OnHit(FishCollider2D fishCollider2D)
    {
        //着弾エフェクト再生
        this.CreateLandingEffect(fishCollider2D.rectTransform.position);

        //魚にダメージ
        fishCollider2D.fish.OnDamaged(this);
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
        /// 移動時間
        /// </summary>
        private float moveTime = 0f;
        /// <summary>
        /// 砲台
        /// </summary>
        private Turret turret = null;
        /// <summary>
        /// 砲弾
        /// </summary>
        private BulletBase bulletBase = null;
        /// <summary>
        /// RectTransform
        /// </summary>
        private RectTransform rectTransform = null;
        /// <summary>
        /// 開始位置
        /// </summary>
        private Vector2 startPosition = Vector2.zero;
        /// <summary>
        /// 投下位置
        /// </summary>
        private Vector2 dropPosition = Vector2.zero;
        /// <summary>
        /// Stayイベント受信機
        /// </summary>
        private BulletCollider.IReceiver stayEventReceiver = null;
        /// <summary>
        /// ステート処理
        /// </summary>
        private Action<float> stateAction = null;
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
            this.stateAction = null;
            Destroy(this.bulletBase.gameObject);
        }

        /// <summary>
        /// Start
        /// </summary>
        public void Start(
            Turret turret,
            BulletBase bulletBase,
            Vector2 dropPosition,
            BulletCollider.IReceiver stayEventReceiver,
            Action onFinished)
        {
            this.turret = turret;
            this.bulletBase = bulletBase;
            this.rectTransform = this.bulletBase.transform as RectTransform;
            this.startPosition = this.rectTransform.anchoredPosition;
            this.dropPosition = dropPosition;
            this.stayEventReceiver = stayEventReceiver;
            this.onFinished = onFinished;

            //爆発するまでコライダを切っておく
            this.bulletBase.bulletCollider.enabled = false;

            //射出アニメーション
            this.turret.PlayBulletFiringAnimation();
            SoundManager.Instance.PlaySe(this.turret.barrelData.seName);

            //砲弾アニメーション更新は手動で行う
            this.bulletBase.animator.enabled = false;
            this.bulletBase.animator.Update(0);
            this.bulletBase.GetComponent<AnimationEventReceiver>().onFinished = this.OnFinishedAnimation;

            //移動開始
            this.stateAction = this.MoveState;
        }

        /// <summary>
        /// Update
        /// </summary>
        public void Update(float deltaTime)
        {
            this.bulletBase.animator.Update(deltaTime);
            this.stateAction?.Invoke(deltaTime);
        }

        /// <summary>
        /// 移動ステート
        /// </summary>
        private void MoveState(float deltaTime)
        {
            //移動
            this.rectTransform.anchoredPosition = Vector2.Lerp(this.startPosition, this.dropPosition, this.moveTime);

            if (this.moveTime >= 1f)
            {
                //爆破
                this.bulletBase.animator.Play("explosion", 0, 0f);
                this.bulletBase.bulletCollider.enabled = true;
                this.bulletBase.bulletCollider.stayEventReceiver = this.stayEventReceiver;
                this.stateAction = null;
                SoundManager.Instance.PlaySe(SeName.FVATTACK_BOMB);
            }

            this.moveTime += deltaTime;
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

}//class BombBullet

}//namespace Battle