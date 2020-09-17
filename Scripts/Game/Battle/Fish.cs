//#define USE_ONWILLRENDEROBJECT
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Battle {

/// <summary>
/// 魚
/// </summary>
public class Fish : TimePauseBehaviour, IFormationEventReceiver
{
    /// <summary>
    /// マスター
    /// </summary>
    public Master.FishData master { get; private set; }
    /// <summary>
    /// ステータスマスター
    /// </summary>
    public Master.ModelBase statusMaster { get; private set; }
    /// <summary>
    /// Transformキャッシュ
    /// </summary>
    public Transform cachedTransform { get; private set; }
    /// <summary>
    /// 生成したFBXモデル
    /// </summary>
    public GameObject model { get; private set; }
    /// <summary>
    /// モデルのTransformキャッシュ
    /// </summary>
    public Transform cachedModelTransform { get; private set; }
    /// <summary>
    /// アニメーター
    /// </summary>
    public Animator animator { get; private set; }
    /// <summary>
    /// 生成した当たり判定用コライダ
    /// </summary>
    public FishCollider2D fishCollider2D { get; private set; }
    /// <summary>
    /// 死んだかどうか
    /// </summary>
    public bool isDead => this.conditionManager.HasCondition(FishConditionType.Dead);
#if USE_ONWILLRENDEROBJECT
    /// <summary>
    /// メッシュがカメラに描画されているかどうか
    /// </summary>
    private bool isVisible = false;
#endif
    /// <summary>
    /// ID
    /// </summary>
    [NonSerialized]
    public ID id;
    /// <summary>
    /// 経過時間
    /// </summary>
    private float elapsedTime = 0f;
    /// <summary>
    /// HP
    /// </summary>
    public int hp { get; private set; } = 10;
    /// <summary>
    /// 移動速度（秒速）
    /// </summary>
    [NonSerialized]
    public float moveSpeed = 3f;
    /// <summary>
    /// 回転時間
    /// </summary>
    private float rotationTime = 6f;
    /// <summary>
    /// 方向転換時間カウント
    /// </summary>
    private float rotationTimeCount = 0;
    /// <summary>
    /// 1フレ毎の最大回転角度
    /// </summary>
    private float maxAddAngle = 1f;
    /// <summary>
    /// 到達判定用距離（回転半径の1.1倍にしておく）
    /// </summary>
    private float arrivedCheckSqrDistance = 1f;
    /// <summary>
    /// 回遊ルート
    /// </summary>
    private Queue<Vector3> migrationRoute = null;
    /// <summary>
    /// 目標地点
    /// </summary>
    private Vector3? targetPosition = null;
    /// <summary>
    /// ターゲットマーク
    /// </summary>
    private GameObject targetMark = null;
    /// <summary>
    /// 状態異常管理
    /// </summary>
    public FishConditionManager conditionManager { get; private set; }
    /// <summary>
    /// 状態異常予約リスト
    /// </summary>
    private List<FishConditionDto> conditionStack = new List<FishConditionDto>();
    /// <summary>
    /// 破棄時コールバック
    /// </summary>
    public event Action<Fish> onDestroy = null;

    /// <summary>
    /// OnDestroy
    /// </summary>
    protected override void OnDestroy()
    {
        if (this.fishCollider2D != null)
        {
            Destroy(this.fishCollider2D.gameObject);
            this.fishCollider2D = null;
        }

        base.OnDestroy();
    }

    /// <summary>
    /// Awake
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        this.cachedTransform = this.transform;
        this.conditionManager = new FishConditionManager(this);
    }

    /// <summary>
    /// モデル設定
    /// </summary>
    public void SetModel(Loader loader)
    {
        Animator _animator;
        this.model = CreateModel(loader, this.cachedTransform, out _animator);
        this.cachedModelTransform = this.model.transform;
        this.animator = _animator;
        this.animator.Play("oyogi", 0, (this.master.isBoss == 0) ? UnityEngine.Random.value : 0f);
#if USE_ONWILLRENDEROBJECT
        var renderers = this.model.GetComponentsInChildren<Renderer>();

        //メッシュ描画時イベントの登録
        foreach (var renderer in renderers)
        {
            var roe = renderer.gameObject.AddComponent<RenderObjectEvent>();
            roe.onWillRenderObject.AddListener(this.OnWillRenderObject);
        }
#endif
    }

    /// <summary>
    /// モデル生成
    /// </summary>
    public static GameObject CreateModel(Loader loader, Transform parent, out Animator animator)
    {
        var model = Instantiate(loader.fbx, parent, false);
        animator = model.GetComponent<Animator>();
        animator.runtimeAnimatorController = loader.anim;

        if (loader.particleDatas != null)
        {
            for (int i = 0; i < loader.particleDatas.Count; i++)
            {
                var particleData = loader.particleDatas[i];

                //設置場所の検索
                var particlePlacement = model
                    .GetComponentsInChildren<Transform>()
                    .FirstOrDefault(child => child.name == particleData.attachingPosition);

                if (particlePlacement == null)
                {
                    //無いならルートがデフォルト
                    particlePlacement = model.transform;
                }

                //パーティクル生成
                Instantiate(particleData.assetLoader.handle.asset, particlePlacement, false);
            }
        }

        return model;
    }

    /// <summary>
    /// コライダ設定
    /// </summary>
    public void SetCollider(FishCollider2D collider, FishColliderData colliderData)
    {
        this.fishCollider2D = collider;
        this.fishCollider2D.Setup(this, colliderData);
    }

    /// <summary>
    /// マスターセット
    /// </summary>
    public void SetMaster(Master.FishData master)
    {
        this.master = master;
    }

    /// <summary>
    /// ステータスマスターセット
    /// </summary>
    public void SetStatusMaster(Master.SingleStageFishData statusMaster)
    {
        this.statusMaster = statusMaster;
        this.hp = (int)statusMaster.hp;
    }

    /// <summary>
    /// ステータスマスターセット
    /// </summary>
    public void SetStatusMaster(Master.MultiStageFishData statusMaster)
    {
        this.statusMaster = statusMaster;
        this.hp = (int)statusMaster.hp;
    }

    /// <summary>
    /// 回遊ルート設定
    /// </summary>
    public void SetMigrationRoute(Vector3[] migrationRoute)
    {
        this.migrationRoute = new Queue<Vector3>(migrationRoute);

        //初期座標
        this.cachedTransform.position = this.migrationRoute.Dequeue();

        //目標地点設定
        this.targetPosition = this.migrationRoute.Dequeue();

        //目標地点の方向に向く
        this.cachedTransform.LookAt(this.targetPosition.Value, Vector3.up);
    }

    /// <summary>
    /// 移動・回転速度設定
    /// </summary>
    public void SetMoveValue(float moveSpeed, float rotationTime)
    {
        this.moveSpeed = moveSpeed;
        this.rotationTime = rotationTime;
        this.maxAddAngle = 360f / rotationTime * SharkDefine.DELTATIME;
        this.arrivedCheckSqrDistance = Mathf.Pow(360f / this.maxAddAngle * this.moveSpeed * SharkDefine.DELTATIME / Mathf.PI * 0.5f * 1.1f, 2);
    }

    /// <summary>
    /// Update
    /// </summary>
    public void ManualUpdate(float deltaTime)
    {
        if (this.enabled)
        {
            this.UpdateInternal(deltaTime);
        }
    }

    /// <summary>
    /// LateUpdate
    /// </summary>
    private void LateUpdate()
    {
        //コライダの更新
        this.fishCollider2D?.UpdateVertices();
#if USE_ONWILLRENDEROBJECT
        //フラグクリア
        this.isVisible = false;
#endif
    }

    /// <summary>
    /// UpdateInternal
    /// </summary>
    private void UpdateInternal(float deltaTime)
    {
        //状態異常予約の反映
        while (this.conditionStack.Count > 0 && this.conditionStack[0].time <= this.elapsedTime)
        {
            this.conditionManager.SetBinary(this.conditionStack[0].conditionBytes);
            this.conditionStack.RemoveAt(0);
        }

        this.elapsedTime += deltaTime;

        //状態異常の更新
        this.conditionManager.Update(deltaTime);

        //動けるなら
        if (this.conditionManager.canMove)
        {
            //到達判定
            if (this.IsArrived())
            {
                //まだ回遊ルートが残っているなら
                if (this.migrationRoute.Count > 0)
                {
                    //新しい目標地点を設定
                    this.targetPosition = this.migrationRoute.Dequeue();

                    //時間カウントをリセット
                    this.rotationTimeCount = 0;
                }
                //もう回遊ルートは残っていない
                else
                {
                    //目標地点クリア
                    this.targetPosition = null;

                    //画面範囲外に出たら
#if USE_ONWILLRENDEROBJECT
                    if (!this.isVisible)
#else
                    if (!this.fishCollider2D.IsInScreen())
#endif
                    {
                        //消滅
                        this.CallDestroy();
                        return;
                    }
                }
            }

            //方向転換
            this.Rotation(deltaTime);

            //移動
            this.cachedTransform.position += this.cachedTransform.forward * this.moveSpeed * deltaTime;
        }

        this.fishCollider2D?.UpdatePosition();
    }

    /// <summary>
    /// 到達判定
    /// </summary>
    private bool IsArrived()
    {
        //目標地点が無いので到達とみなす
        if (this.targetPosition == null)
        {
            return true;
        }

        //目的地までの距離
        float distance = Vector3.SqrMagnitude(this.targetPosition.Value - this.cachedTransform.position);

        //距離での到達判定
        if (distance < this.arrivedCheckSqrDistance)
        {
            return true;
        }

        //距離で到達する前に方向転換で1回転した場合も到達とみなす
        if (this.rotationTimeCount >= this.rotationTime)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// 方向転換
    /// </summary>
    private void Rotation(float deltaTime)
    {
        //目標地点が無いので回転不要
        if (this.targetPosition == null)
        {
            return;
        }

        Vector3 v1 = this.cachedTransform.forward;
        Vector3 v2 = this.targetPosition.Value - this.cachedTransform.position;
        float remainAngle = Vector3.Angle(v1, v2);

        //まだ回転が必要
        if (remainAngle > maxAddAngle)
        {
            //回転
            Quaternion q1 = this.cachedTransform.rotation;
            Quaternion q2 = Quaternion.LookRotation(v2, Vector3.up);
            float t = maxAddAngle * (deltaTime / SharkDefine.DELTATIME) / remainAngle;
            this.cachedTransform.rotation = Quaternion.Slerp(q1, q2, t);

            //時間カウント増加
            this.rotationTimeCount += deltaTime;
        }
        else
        {
            this.cachedTransform.LookAt(this.targetPosition.Value, Vector3.up);
        }

        //軸固定するなら
        if (this.master.isFixAxis != 0)
        {
            //常に頭を上に向かせる
            var modelAngles = this.cachedModelTransform.localEulerAngles;
            modelAngles.x = -this.cachedTransform.localEulerAngles.x;
            this.cachedModelTransform.localEulerAngles = modelAngles;
        }
    }

    /// <summary>
    /// 停止
    /// </summary>
    public override void Pause(BinaryWriter writer)
    {
        base.Pause(writer);
        writer.Write(this.animator.enabled);
        this.animator.enabled = false;
    }

    /// <summary>
    /// 再開
    /// </summary>
    public override void Play(BinaryReader reader)
    {
        base.Play(reader);
        this.animator.enabled = reader.ReadBoolean();
    }

    /// <summary>
    /// 再開
    /// </summary>
    void IFormationEventReceiver.OnPlay()
    {
        this.conditionManager.GetCondition(FishConditionType.PauseEvent)?.End();
    }

    /// <summary>
    /// 一時停止
    /// </summary>
    void IFormationEventReceiver.OnPause()
    {
        this.conditionManager.Add(new FishCondition(FishConditionType.PauseEvent));
    }

    /// <summary>
    /// 強制終了
    /// </summary>
    void IFormationEventReceiver.OnQuit()
    {
        //ルートを全部クリアして終わったことにする
        this.migrationRoute.Clear();
        this.targetPosition = null;
    }

    /// <summary>
    /// 画面からはける
    /// </summary>
    public void OffStage()
    {
        //目的地決定
        while (this.migrationRoute.Count > 0)
        {
            this.targetPosition = this.migrationRoute.Dequeue();
        }

        //瞬時に目的地の方向に向く
        if (this.targetPosition.HasValue)
        {
            this.cachedTransform.LookAt(this.targetPosition.Value, Vector3.up);
            this.targetPosition = null;
        }

        //軸固定するなら
        if (this.master.isFixAxis != 0)
        {
            //常に頭を上に向かせる
            var modelAngles = this.cachedModelTransform.localEulerAngles;
            modelAngles.x = -this.cachedTransform.localEulerAngles.x;
            this.cachedModelTransform.localEulerAngles = modelAngles;
        }

        //速いスピードで
        this.moveSpeed = 10f;
    }
#if USE_ONWILLRENDEROBJECT
    /// <summary>
    /// メッシュがカメラに描画されているときに呼ばれる
    /// </summary>
    private void OnWillRenderObject()
    {
        if (Camera.current == BattleGlobal.instance.fishCamera)
        {
            this.isVisible = true;
        }
    }
#endif
    /// <summary>
    /// ダメージ食らう
    /// </summary>
    public void OnDamaged(Bullet bullet)
    {
        if (this.isDead)
        {
            return;
        }

        //ダメージ
        int damage = BattleGlobal.GetDamage(this, bullet);

        //ダメージ食らう
        this.hp -= damage;
#if DEBUG
        if (BattleDebug.isOneKill)
        {
            this.hp = 0;
        }
#endif

        //被ダメ中状態になる
        this.conditionManager.Add(new FishConditionDamaged());

        //被ダメ時スキル効果発動
        bullet.skill.OnFishDamaged(this);

        //捕獲成功したかチェック
        if (SceneChanger.currentScene is BattleSceneBase && (SceneChanger.currentScene as BattleSceneBase).FishCatchingCalculation(this))
        {
            //死亡フラグON
            this.conditionManager.Add(new FishConditionDead());

            //捕獲通知
            (SceneChanger.currentScene as BattleSceneBase)?.OnFishCatched(this, bullet);
        }

        //被ダメ通知
        (SceneChanger.currentScene as MultiBattleScene)?.OnFishDamaged(this, damage);

        //状態変化通知
        (SceneChanger.currentScene as MultiBattleScene)?.OnChangeFishCondition(this);
    }

    /// <summary>
    /// 破棄処理を呼ぶ
    /// </summary>
    public void CallDestroy()
    {
        this.onDestroy?.Invoke(this);
        this.onDestroy = null;
    }

    /// <summary>
    /// 目標地点を持っているかどうか
    /// </summary>
    public bool HasTargetPosition()
    {
        return this.targetPosition.HasValue;
    }

    /// <summary>
    /// ターゲット中かどうか
    /// </summary>
    public bool IsTarget()
    {
        return this.targetMark != null          //ターゲットマークが付与されている
            && !this.isDead                     //生きている
            && this.fishCollider2D.IsInScreen();//画面内にいる
    }

    /// <summary>
    /// ターゲットマークを外す
    /// </summary>
    public void RemoveTargetMark()
    {
        if (this.targetMark != null)
        {
            Destroy(this.targetMark);
            this.targetMark = null;
        }
    }

    /// <summary>
    /// ターゲットマークを付ける
    /// </summary>
    public void SetTargetMark(GameObject targetMarkPrefab)
    {
        this.RemoveTargetMark();
        this.targetMark = Instantiate(targetMarkPrefab, this.fishCollider2D.rectTransform, false);
        this.fishCollider2D.UpdatePosition();
    }

    /// <summary>
    /// 氷結
    /// </summary>
    public void Freezing(float time)
    {
        this.conditionManager.Add(new FishConditionFreezing(time));
    }

    /// <summary>
    /// 見た目更新
    /// </summary>
    public void RefleshForm()
    {
        int layer = this.conditionManager.HasCondition(FishConditionType.Damaged)  ? BattleGlobal.instance.fishDamagedLayer
                  : this.conditionManager.HasCondition(FishConditionType.Dead)     ? BattleGlobal.instance.fishNormalLayer
                  : this.conditionManager.HasCondition(FishConditionType.Freezing) ? BattleGlobal.instance.fishIceLayer
                  : BattleGlobal.instance.fishNormalLayer;

        bool animatorEnabled = this.conditionManager.HasCondition(FishConditionType.Dead)     ? false
                             : this.conditionManager.HasCondition(FishConditionType.Freezing) ? false
                             : true;

        this.animator.enabled = animatorEnabled;
        this.gameObject.SetLayerRecursively(layer);
    }

    /// <summary>
    /// 復帰データ取得
    /// </summary>
    public FishResumeDto GetResumeDto()
    {
        return new FishResumeDto {
            elapsedTime = this.elapsedTime,
            hp = this.hp,

            rotationTimeCount = this.rotationTimeCount,
            localPosition = BattleGlobal.instance.viewRotation * this.cachedTransform.localPosition,
            forward = BattleGlobal.instance.viewRotation * this.cachedTransform.forward,

            remainRouteCount = this.migrationRoute.Count,
            hasTargetPosition = this.targetPosition.HasValue,

            conditionDto = this.GetConditionDto(),
        };
    }

    /// <summary>
    /// 復帰データセット
    /// </summary>
    public void SetResumeDto(FishResumeDto dto)
    {
        this.elapsedTime = dto.elapsedTime;
        this.hp = dto.hp;

        this.rotationTimeCount = dto.rotationTimeCount;
        this.cachedTransform.localPosition = BattleGlobal.instance.viewRotation * dto.localPosition;
        this.cachedTransform.localRotation = Quaternion.LookRotation(BattleGlobal.instance.viewRotation * dto.forward, Vector3.up);

        while (this.migrationRoute.Count > dto.remainRouteCount)
        {
            this.targetPosition = this.migrationRoute.Dequeue();
        }

        if (!dto.hasTargetPosition)
        {
            this.targetPosition = null;
        }

        this.SetConditionDto(dto.conditionDto);
    }

    /// <summary>
    /// ダメージデータセット
    /// </summary>
    public void SetDamagedDto(FishDamagedDto dto)
    {
        this.hp -= dto.damage;
    }

    /// <summary>
    /// 状態データ取得
    /// </summary>
    public FishConditionDto GetConditionDto()
    {
        return new FishConditionDto {
            id = this.id,
            time = this.elapsedTime,
            conditionBytes = this.conditionManager.GetBinary(),
        };
    }

    /// <summary>
    /// 状態データセット
    /// </summary>
    public void SetConditionDto(FishConditionDto dto)
    {
        this.conditionStack.Add(dto);
        this.conditionStack.Sort((a, b) => a.time < b.time ? -1 : 1);
    }

    /// <summary>
    /// ID
    /// </summary>
    public struct ID
    {
        public enum Type
        {
            Normal,
            Random,
            Summon,
        }

        public byte type;
        public byte waveId;
        public byte formationId;
        public byte routeId;
        public uint createId;
        public int summonId;

        public override int GetHashCode()
        {
            return this.type.GetHashCode()
                 ^ this.waveId.GetHashCode()
                 ^ this.formationId.GetHashCode()
                 ^ this.routeId.GetHashCode()
                 ^ this.createId.GetHashCode()
                 ^ this.summonId.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is ID)
            {
                var source = (ID)obj;
                return source.type == this.type
                    && source.waveId == this.waveId
                    && source.formationId == this.formationId
                    && source.routeId == this.routeId
                    && source.createId == this.createId
                    && source.summonId == this.summonId;
            }
            return false;
        }

        public override string ToString()
        {
            return string.Format(
                "type={0}, waveId={1}, formationId={2}, routeId={3}, createId={4}, summonId={5}",
                this.type,
                this.waveId,
                this.formationId,
                this.routeId,
                this.createId,
                this.summonId
            );
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(this.type);
            writer.Write(this.waveId);
            writer.Write(this.formationId);
            writer.Write(this.routeId);
            writer.Write(this.createId);
            writer.Write(this.summonId);
        }

        public void Read(BinaryReader reader)
        {
            this.type = reader.ReadByte();
            this.waveId = reader.ReadByte();
            this.formationId = reader.ReadByte();
            this.routeId = reader.ReadByte();
            this.createId = reader.ReadUInt32();
            this.summonId = reader.ReadInt32();
        }
    }

    /// <summary>
    /// 魚に必要なアセットをまとめてロードするやつ
    /// </summary>
    public class Loader : AssetListLoader
    {
        /// <summary>
        /// キー
        /// </summary>
        public string key { get; private set; }
        /// <summary>
        /// FBX
        /// </summary>
        public GameObject fbx => this[0].handle.asset as GameObject;
        /// <summary>
        /// アニメーターコントローラ
        /// </summary>
        public RuntimeAnimatorController anim => this[1].handle.asset as RuntimeAnimatorController;
        /// <summary>
        /// コライダデータ
        /// </summary>
        public FishColliderData colliderData => this[2].handle.asset as FishColliderData;
        /// <summary>
        /// パーティクルデータ
        /// </summary>
        public List<(string attachingPosition, IAssetLoader assetLoader)> particleDatas = null;

        /// <summary>
        /// construct
        /// </summary>
        public Loader(uint fishId, string key)
        {
            this.Add<GameObject>(SharkDefine.GetFishFbxPath(key));
            this.Add<RuntimeAnimatorController>(SharkDefine.GetFishAnimatorControllerPath(key));
            this.Add<FishColliderData>(SharkDefine.GetFishColliderDataPath(key));

            var particles = Masters.FishParticleDB.GetList().FindAll(x => x.fishId == fishId);
            if (particles != null && particles.Count > 0)
            {
                this.particleDatas = new List<(string, IAssetLoader)>();

                for (int i = 0; particles != null && i < particles.Count; i++)
                {
                    this.particleDatas.Add((
                        attachingPosition: particles[i].attachingPosition,
                        assetLoader: this.Add<GameObject>(SharkDefine.GetFishParticlePath(key, particles[i].particleName))
                    ));
                }
            }
        }

    }//class Loader

}//class Fish

}//namespace Battle