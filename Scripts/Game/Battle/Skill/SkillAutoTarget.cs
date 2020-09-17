using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using Battle;

/// <summary>
/// バトルアイテムスキル：自動照準
/// </summary>
public class SkillAutoTarget : SkillBattleItemBase
{
    /// <summary>
    /// 砲台制御するかどうか
    /// </summary>
    public override bool isTurretController => true;
    /// <summary>
    /// SE名
    /// </summary>
    public override string seName => SeName.ITEM_AUTO;

    /// <summary>
    /// バトルアイテム使用時
    /// </summary>
    public override void OnUseBattleItem()
    {
        var controller = new Controller(this, BattleGlobal.instance.userData.turret);
        controller.Start();
    }

    /// <summary>
    /// 自動照準処理
    /// </summary>
    public class Controller : ITurretController
    {
        /// <summary>
        /// スキル
        /// </summary>
        private SkillAutoTarget skill = null;
        /// <summary>
        /// 砲台
        /// </summary>
        private Turret turret = null;
        /// <summary>
        /// ターゲットの魚
        /// </summary>
        private Fish targetFish = null;
        /// <summary>
        /// 時間
        /// </summary>
        private float time = 0f;

        /// <summary>
        /// construct
        /// </summary>
        public Controller(SkillAutoTarget skill, Turret turret)
        {
            this.skill = skill;
            this.turret = turret;
        }

        /// <summary>
        /// Start
        /// </summary>
        public void Start()
        {
            //砲台の自動照準機能を有効化
            this.turret.SetTurretController(this);
            BattleGlobal.instance.turretEventTrigger.onClick += this.SelectTargetFish;
        }

        /// <summary>
        /// Run
        /// </summary>
        void ITurretController.Run(float deltaTime)
        {
            //既にターゲット済みかどうか
            bool isTarget = this.targetFish != null && this.targetFish.IsTarget();

            if (!isTarget)
            {
                //ターゲットマークを外す
                this.targetFish?.RemoveTargetMark();
                this.targetFish = null;

                //ターゲットが見つかったなら、魚にターゲットマークを付ける
                this.targetFish = (SceneChanger.currentScene as BattleSceneBase).FindTargetFish();
                this.targetFish?.SetTargetMark(BattleGlobal.instance.targetMarkPrefab);
            }

            if (this.targetFish != null)
            {
                //ターゲットに銃口を向ける
                var targetScreenPoint = BattleGlobal.instance.uiCamera.WorldToScreenPoint(this.targetFish.fishCollider2D.rectTransform.position);
                this.turret.SetMuzzleDirection(targetScreenPoint);
            }

            //可能なら弾発射
            NormalBullet[] bullets = null;
            if (this.turret.CreateNormalBullet(out bullets))
            {
                //弾発射
                bullets[0].Setup(
                    isFvAttack: false,
                    power: this.turret.power,
                    fvRate: this.turret.fvPointGetValue,
                    bulletData: this.turret.bulletData,
                    skill: BattleGlobal.instance.userData.skill,
                    speed: this.turret.speed,
                    targetFish: this.targetFish
                );
                this.turret.PlayBulletFiringAnimation();
                this.turret.ResetIntervalCount();
                SoundManager.Instance.PlaySe(this.turret.barrelData.seName);

                //コイン消費通知
                (SceneChanger.currentScene as MultiBattleScene)?.OnUseCoin(bullets[0].bet);

                //弾発射通知
                this.turret.OnShoot(bullets[0]);
            }

            if (this.time >= this.skill.duration * Masters.MilliSecToSecond)
            {
                //自動照準機能破棄
                this.turret.SetTurretController(null);
                BattleGlobal.instance.turretEventTrigger.onClick -= this.SelectTargetFish;

                //ターゲットマークを外す
                this.targetFish?.RemoveTargetMark();
            }

            //効果時間カウント
            this.time += deltaTime;
        }

        /// <summary>
        /// ターゲットとなる魚を決定する
        /// </summary>
        private void SelectTargetFish(PointerEventData eventData)
        {
            Ray ray = BattleGlobal.instance.fishCamera.ScreenPointToRay(eventData.position);
            RaycastHit[] hits = Physics.RaycastAll(ray);

            for (int i = 0; i < hits.Length; i++)
            {
                //ヒットしたオブジェクトの中に魚がいたら
                var fish = BattleGlobal.instance.fishList.Find(x => x.fishCollider2D.boxCollider == hits[i].collider);
                if (fish != null && !fish.isDead)
                {
                    if (this.targetFish != null)
                    {
                        //今ターゲットになっている魚からターゲットマークを外す
                        this.targetFish.RemoveTargetMark();
                    }

                    //選択した魚をターゲットにする
                    this.targetFish = fish;
                    this.targetFish.SetTargetMark(BattleGlobal.instance.targetMarkPrefab);
                }
            }
        }
    }
}
