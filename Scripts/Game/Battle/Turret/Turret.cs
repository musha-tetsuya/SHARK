using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Battle {

/// <summary>
/// 砲塔
/// </summary>
public class Turret : TimePauseBehaviour
{
    /// <summary>
    /// 砲台基礎
    /// </summary>
    [SerializeField]
    public TurretBase turretBase = null;

    /// <summary>
    /// バトルユーザーデータ
    /// </summary>
    private BattleUserData userData = null;
    /// <summary>
    /// マスター
    /// </summary>
    public Master.BatteryData batteryData { get; private set; }
    public Master.BarrelData barrelData { get; private set; }
    public Master.BulletData bulletData { get; private set; }
    public Master.FvAttackData fvAttackData { get; private set; }
    /// <summary>
    /// 発射間隔カウント
    /// </summary>
    public float intervalCount { get; private set; }
    /// <summary>
    /// 攻撃力
    /// </summary>
    public uint power { get; private set; }
    /// <summary>
    /// 発射速度
    /// </summary>
    public uint speed { get; private set; }
    /// <summary>
    /// FVポイント獲得値
    /// </summary>
    public uint fvPointGetValue { get; private set; }
    /// <summary>
    /// 弾丸数
    /// </summary>
    private int bulletCount = 0;
    /// <summary>
    /// スクリーンでの位置
    /// </summary>
    private Vector2 screenPosition = Vector2.zero;
    /// <summary>
    /// 砲台制御
    /// </summary>
    public ITurretController turretController { get; private set; }
    /// <summary>
    /// アセットまとめてロードするやつ
    /// </summary>
    public AssetListLoader loader { get; private set; } = new AssetListLoader();
    /// <summary>
    /// FVアタックプレハブアセット
    /// </summary>
    private IAssetLoader fvaAsset = null;
    /// <summary>
    /// FVA弾アセット
    /// </summary>
    public IAssetLoader fvaBulletAsset { get; private set; }
    /// <summary>
    /// FVA貫通弾チャージエフェクトアセット
    /// </summary>
    private IAssetLoader fvaPenetrationChargeEffectAsset = null;
    /// <summary>
    /// ステート管理
    /// </summary>
    private StateManager stateManager = new StateManager();

    /// <summary>
    /// Init
    /// </summary>
    protected void Init(uint batteryId, uint barrelId, uint bulletId)
    {
        this.batteryData = Masters.BatteryDB.FindById(batteryId);
        this.barrelData = Masters.BarrelDB.FindById(barrelId);
        this.bulletData = Masters.BulletDB.FindById(bulletId);
        this.fvAttackData = Masters.FvAttackDB.FindById(this.batteryData.fvAttackId);

        this.turretBase.batteryKey = this.batteryData.key;
        this.turretBase.barrelKey = this.barrelData.key;
        this.turretBase.bulletKey = this.bulletData.key;

        this.loader.Add<GameObject>(SharkDefine.GetBatteryPrefabPath(this.turretBase.batteryKey));
        this.loader.Add<GameObject>(SharkDefine.GetBarrelPrefabPath(this.turretBase.barrelKey));
        this.loader.Add<BulletBase>(SharkDefine.GetBulletPrefabPath(this.turretBase.bulletKey));
        this.fvaAsset = this.loader.Add<FvAttackBase>(SharkDefine.GetFvAttackPrefabPath((FvAttackType)this.fvAttackData.type));
        this.fvaBulletAsset = this.loader.Add<BulletBase>(SharkDefine.GetFvAttackBulletPrefabPath(this.fvAttackData.key));
        this.loader.Add<Sprite>(SharkDefine.GetFvAttackTypeIconSpritePath((FvAttackType)this.fvAttackData.type));
        this.loader.Add<AudioClip>(SharkDefine.GetSeClipPath(this.barrelData.seName));
        this.loader.Add<AudioClip>(SharkDefine.GetSeClipPath(this.bulletData.seName));

        string fvAttackSeName = SeName.FvAttackSeName[(int)this.fvAttackData.type];
        if (!string.IsNullOrEmpty(fvAttackSeName))
        {
            this.loader.Add<AudioClip>(SharkDefine.GetSeClipPath(fvAttackSeName));
        }

        if (this.fvAttackData.type == (uint)FvAttackType.Penetration)
        {
            this.fvaPenetrationChargeEffectAsset = this.loader.Add<GameObject>(SharkDefine.GetFVAChargeEffectPrefabPath(this.fvAttackData.key));
        }
    }

    /// <summary>
    /// Init
    /// </summary>
    public void Init(BattleUserData userData)
    {
        this.userData = userData;

        this.Init(
            this.userData.turretData.batteryMasterId,
            this.userData.turretData.barrelMasterId,
            this.userData.turretData.bulletMasterId
        );

        //基礎能力 = 各パーツ能力 + ギア能力
        this.power = this.bulletData.power + (uint)this.userData.gears.Sum(x => x.power);
        this.speed = this.barrelData.speed + (uint)this.userData.gears.Sum(x => x.speed);
        this.fvPointGetValue = this.batteryData.fvPoint + (uint)userData.gears.Sum(x => x.fvPoint);

        //ステート管理
        this.stateManager = new StateManager();
        this.stateManager.AddState(new NormalState{ turret = this });
        this.stateManager.AddState(new TurretControllerState{ turret = this });
    }

    /// <summary>
    /// セットアップ
    /// </summary>
    public virtual void Setup()
    {
        this.screenPosition = BattleGlobal.instance.uiCamera.WorldToScreenPoint(this.turretBase.rotationParts.position);

        //ロード済みのはずのデータで見た目を更新
        this.turretBase.Reflesh();

        this.stateManager.ChangeState<NormalState>();
    }

    /// <summary>
    /// Update
    /// </summary>
    public void Run(float deltaTime)
    {
        (this.stateManager.currentState as MyState)?.Update(deltaTime);
    }

    /// <summary>
    /// タッチ位置の取得（スクリーン座標で返す）
    /// </summary>
    private bool GetTouchPosition(out Vector2 touchPosition)
    {
        touchPosition = Vector2.zero;
#if false
        //タッチサポートしているならタッチ位置を返す
        if (Input.touchSupported)
        {
            if (Input.touchCount > 0)
            {
                touchPosition = Input.GetTouch(0).position;
                return true;
            }
        }
        //タッチサポートしていないならマウス位置を返す
        else
#endif
        {
            if (Input.GetMouseButton(0))
            {
                touchPosition.x = Input.mousePosition.x;
                touchPosition.y = Input.mousePosition.y;
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 銃口の向き先を設定
    /// </summary>
    public void SetMuzzleDirection(Vector2 targetScreenPoint)
    {
        var direction = targetScreenPoint - this.screenPosition;
        direction.y = Mathf.Max(0f, direction.y);//下向きにならないよう制限
        this.turretBase.rotationParts.up = direction;
    }

    /// <summary>
    /// 銃口の向き更新
    /// </summary>
    public void UpdateMuzzleDirection()
    {
        Vector2 touchPosition;
        if (GetTouchPosition(out touchPosition))
        {
            //タッチ位置に銃口を向ける
            this.SetMuzzleDirection(touchPosition);
        }
    }

    /// <summary>
    /// 弾丸生成
    /// </summary>
    public T CreateBullet<T>(BulletBase bulletBasePrefab) where T : Bullet
    {
        //弾丸生成
        var bulletBase = this.turretBase.CreateBullet(bulletBasePrefab, BattleGlobal.instance.bulletArea);
        //バトル用弾丸コンポーネント付与
        return bulletBase.gameObject.AddComponent<T>();
    }

    /// <summary>
    /// 通常弾生成
    /// </summary>
    public bool CreateNormalBullet(out NormalBullet[] bullets, int count = 1)
    {
        bullets = null;

        //発射間隔と弾丸数をチェック
        if (this.CanCreateNormalBullet(count))
        {
            bullets = new NormalBullet[count];
            for (int i = 0; i < count; i++)
            {
                //弾丸生成
                var bulletBase = this.turretBase.CreateBullet(BattleGlobal.instance.bulletArea);
                //バトル用弾丸コンポーネント付与
                bullets[i] = bulletBase.gameObject.AddComponent<NormalBullet>();
                //弾丸数増加
                this.bulletCount++;
                //破棄されたら弾丸数減少
                bullets[i].onDestroy += (x) => this.bulletCount--;
            }
            return true;
        }

        return false;
    }

    /// <summary>
    /// 通常弾生成しても良いかどうか
    /// </summary>
    protected virtual bool CanCreateNormalBullet(int count)
    {
        return this.intervalCount <= 0f && this.bulletCount + count <= 20; 
    }

    /// <summary>
    /// 弾丸発射アニメーション再生
    /// </summary>
    public void PlayBulletFiringAnimation()
    {
        this.turretBase.PlayFiringAnimation();
    }

    /// <summary>
    /// 波動砲発射アニメーション再生
    /// </summary>
    public void PlayLaserBeamAnimation()
    {
        this.turretBase.PlayLaserBeamAnimation();
    }

    /// <summary>
    /// 波動砲発射アニメーション終了
    /// </summary>
    public void EndLaserBeamAnimation()
    {
        this.turretBase.EndLaserBeamAnimation();
    }

    /// <summary>
    /// 発射間隔リセット
    /// </summary>
    public void ResetIntervalCount()
    {
        this.intervalCount = this.barrelData.interval * Masters.MilliSecToSecond;
    }

    /// <summary>
    /// FVアタック発動
    /// </summary>
    public void FvAttackFiring()
    {
        var fvAttack = Instantiate(this.fvaAsset.handle.asset as FvAttackBase, BattleGlobal.instance.fvAttackArea, false);
        fvAttack.Setup(this, this.fvAttackData);
    }

    /// <summary>
    /// 砲台制御機能切り替え
    /// </summary>
    public void SetTurretController(ITurretController turretController)
    {
        this.turretController = turretController;
        (SceneChanger.currentScene as BattleSceneBase)?.OnSetTurretController(this.turretController);

        if (this.turretController == null)
        {
            this.stateManager.ChangeState<NormalState>();
        }
        else
        {
            this.stateManager.ChangeState<TurretControllerState>();
        }
    }

    /// <summary>
    /// FVA貫通弾チャージエフェクト生成
    /// </summary>
    public virtual GameObject CreateFVAPenetrationChargeEffect(bool raiseEvent)
    {
        var prefab = this.fvaPenetrationChargeEffectAsset.handle.asset as GameObject;
        var effect = Instantiate(prefab, this.turretBase.muzzle, false);
        var renderers = effect.GetComponentsInChildren<ParticleSystemRenderer>();
        var canvas = this.GetComponentInParent<Canvas>();
        renderers.SetParticleLayer(canvas);
        return effect;
    }

    /// <summary>
    /// 弾丸発射時
    /// </summary>
    public virtual void OnShoot(Bullet bullet){}

    /// <summary>
    /// 波動砲発射時
    /// </summary>
    public virtual void OnShoot(LaserBeamBullet bullet){}

    /// <summary>
    /// 範囲ボム弾発射時
    /// </summary>
    public virtual void OnShoot(BombBullet bullet){}

    /// <summary>
    /// 全体弾発射時
    /// </summary>
    public virtual void OnShoot(AllRangeBullet bullet){}

    /// <summary>
    /// メインステート
    /// </summary>
    private abstract class MyState : StateBase
    {
        /// <summary>
        /// 砲台
        /// </summary>
        public Turret turret = null;

        /// <summary>
        /// Update
        /// </summary>
        public virtual void Update(float deltaTime)
        {
            if (this.turret.intervalCount > 0f)
            {
                //発射間隔のカウント
                this.turret.intervalCount -= deltaTime;
            }
        }
    }

    /// <summary>
    /// 通常時ステート
    /// </summary>
    private class NormalState : MyState
    {
        /// <summary>
        /// Update
        /// </summary>
        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);

            if (BattleGlobal.instance.turretEventTrigger.isTouch)
            {
                //タッチ位置に銃口を向ける
                this.turret.UpdateMuzzleDirection();

                //可能なら弾発射
                NormalBullet[] bullets = null;
                if (this.turret.CreateNormalBullet(out bullets))
                {
                    bullets[0].Setup(
                        isFvAttack: false,
                        power: this.turret.power,
                        fvRate: this.turret.fvPointGetValue,
                        bulletData: this.turret.bulletData,
                        skill: this.turret.userData.skill,
                        speed: this.turret.speed
                    );
                    this.turret.PlayBulletFiringAnimation();
                    this.turret.ResetIntervalCount();
                    SoundManager.Instance.PlaySe(this.turret.barrelData.seName);

                    //コイン消費通知
                    (SceneChanger.currentScene as MultiBattleScene)?.OnUseCoin(bullets[0].bet);

                    //弾発射通知
                    this.turret.OnShoot(bullets[0]);
                }
            }
        }
    }

    /// <summary>
    /// 砲台制御機能時ステート
    /// </summary>
    private class TurretControllerState : MyState
    {
        /// <summary>
        /// Update
        /// </summary>
        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);

            //砲台制御機能が働いている場合
            if (this.turret.turretController != null)
            {
                this.turret.turretController.Run(deltaTime);
            }
        }
    }

}//class Turret

}//namespace Battle