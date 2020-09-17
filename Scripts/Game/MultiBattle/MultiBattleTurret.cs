using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using PhotonHashtable = ExitGames.Client.Photon.Hashtable;

namespace Battle {

/// <summary>
/// マルチバトル用砲台
/// </summary>
public class MultiBattleTurret : Turret
{
    /// <summary>
    /// コイン情報パネルUI
    /// </summary>
    [SerializeField]
    public UICoinInfoPanel coinInfoPanel = null;
    /// <summary>
    /// BETボタンUI
    /// </summary>
    [SerializeField]
    public UIBetButton uiBetButton = null;
    /// <summary>
    /// FVアタックアイコン位置
    /// </summary>
    [SerializeField]
    private RectTransform fvAttackIconPos = null;
    /// <summary>
    /// FVアタックゲージUI
    /// </summary>
    [SerializeField]
    public UIFvAttackGauge uiFvAttackGauge = null;
    /// <summary>
    /// クジラパネルUI
    /// </summary>
    [SerializeField]
    public UIWhalePanel uiWhalePanel = null;
    /// <summary>
    /// クジラパネルUIのY軸角度
    /// </summary>
    [SerializeField]
    public float whalePanelAngleY = 0f;

    /// <summary>
    /// Transformキャッシュ
    /// </summary>
    public RectTransform cachedTransform { get; private set; }
    /// <summary>
    /// セットアップ済みかどうか
    /// </summary>
    public bool isSetuped { get; private set; }
    /// <summary>
    /// ActorNo.
    /// </summary>
    public int actorNo { get; private set; } = -1;
    /// <summary>
    /// 操作プレイヤーかどうか
    /// </summary>
    private bool isLocalPlayer = false;
    /// <summary>
    /// 弾丸リスト
    /// </summary>
    private List<BulletDto> bulletList = new List<BulletDto>();
    /// <summary>
    /// 砲身回転コルーチン
    /// </summary>
    private Coroutine barrelRotationCoroutine = null;
    /// <summary>
    /// FVAコントローラ
    /// </summary>
    private List<IFVAController> fvaControllers = new List<IFVAController>();
    /// <summary>
    /// 発射回数
    /// </summary>
    private int shootCount = 0;

    /// <summary>
    /// Awake
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        this.cachedTransform = this.transform as RectTransform;
        this.coinInfoPanel.gameObject.SetActive(false);
        this.uiBetButton.gameObject.SetActive(false);
    }

    /// <summary>
    /// カスタムプロパティ取得
    /// </summary>
    public PhotonHashtable GetCustomProperties()
    {
        return PhotonNetwork.CurrentRoom.Players[this.actorNo].CustomProperties;
    }

    /// <summary>
    /// Init
    /// </summary>
    public void Init(int actorNo)
    {
        Debug.Assert(actorNo >= 0);

        this.actorNo = actorNo;
        this.isLocalPlayer = (actorNo == PhotonNetwork.LocalPlayer.ActorNumber);

        //自分の場合
        if (this.isLocalPlayer)
        {
            this.Init(BattleGlobal.instance.userData);

            //FVアタックアイコン位置調整
            this.uiFvAttackGauge.transform.SetParent(this.fvAttackIconPos);
            (this.uiFvAttackGauge.transform as RectTransform).anchoredPosition = Vector2.zero;

            //クジラパネルUI角度調整
            this.uiWhalePanel.SetAxisAngleY(this.whalePanelAngleY);
        }
        //自分じゃない場合
        else
        {
            var properties = this.GetCustomProperties();
            var dto = new TurretDto();
            dto.SetBinary((byte[])properties[PlayerPropertyKey.Turret]);
            this.Init(dto.batteryId, dto.barrelId, dto.bulletId);
        }
    }

    /// <summary>
    /// セットアップ
    /// </summary>
    public override void Setup()
    {
        if (this.actorNo < 0)
            return;

        if (this.loader.GetStatus() != AssetListLoader.Status.Loaded)
            return;

        this.isSetuped = true;
        this.coinInfoPanel.gameObject.SetActive(true);
        this.uiBetButton.gameObject.SetActive(true);

        var properties = this.GetCustomProperties();
        object value;

        if (properties.TryGetValue(PlayerPropertyKey.Coin, out value))
        {
            //コイン数表示
            this.coinInfoPanel.SetCoinNum((long)value);
        }

        if (properties.TryGetValue(PlayerPropertyKey.Bet, out value))
        {
            //BET数表示
            this.uiBetButton.SetBetNum((int)value);
        }

        if (this.isLocalPlayer)
        {
            var userData = BattleGlobal.instance.userData as MultiBattleUserData;

            base.Setup();

            //コインフレーム形状変更
            this.coinInfoPanel.SetForm(true);

            //BET操作ボタンの表示ON
            this.uiBetButton.SetButtonVisible(true);

            //FVAアイコン差し替え
            this.uiFvAttackGauge.Setup();

            //龍玉魂セット
            this.uiWhalePanel.SetBallNum(userData.ballNum);
            this.uiWhalePanel.SetSoulNum(userData.soulNum);
        }
        else
        {
            //ロード済みのはずのデータで見た目を更新
            this.turretBase.Reflesh();

            //BET操作ボタンの表示はOFF
            this.uiBetButton.SetButtonVisible(false);

            //弾丸リスト取得
            if (properties.TryGetValue(PlayerPropertyKey.BulletList, out value))
            {
                var list = BinaryUtility.CreateArray<BulletDto>((byte[])value);

                for (int i = 0; i < list.Length; i++)
                {
                    //弾丸生成
                    var data = list[i];
                    this.CreateBullet(data);
                    this.turretBase.rotationParts.localEulerAngles = data.barrelLocalEulerAngles;
                }
            }
        }
    }

    /// <summary>
    /// アンロード
    /// </summary>
    public void Unload()
    {
        this.loader.Unload();
        this.loader.Clear();

        //発射済み砲弾の破棄
        this.ClearBulletList();

        //コイン情報非表示に
        this.coinInfoPanel.gameObject.SetActive(false);

        //BET情報非表示に
        this.uiBetButton.gameObject.SetActive(false);

        this.isSetuped = false;
        this.actorNo = -1;
        this.shootCount = 0;
        this.turretBase.ClearBattery();
        this.turretBase.ClearBarrel();
    }

    /// <summary>
    /// 弾丸生成
    /// </summary>
    private void CreateBullet(BulletDto data)
    {
        var prefab = data.isNormalBullet
            ? this.turretBase.bulletPrefab
            : AssetManager.FindHandle<BulletBase>(SharkDefine.GetFvAttackBulletPrefabPath(this.fvAttackData.key)).asset as BulletBase;

        data.bulletBase = this.turretBase.CreateBullet(prefab, BattleGlobal.instance.bulletArea);
        data.bulletBase.gameObject.AddComponent<BulletBouncing>();
        data.bulletBase.movement.timeStamp = data.timeStamp;
        data.bulletBase.movement.speed = data.speed;
        data.bulletBase.transform.localPosition = this.cachedTransform.localRotation * data.bulletLocalPosition;
        data.bulletBase.transform.rotation = this.cachedTransform.localRotation * Quaternion.Euler(data.bulletLocalEulerAngles);
        data.bulletBase.SetParticleLayer(BattleGlobal.instance.bulletCanvas);
        this.bulletList.Add(data);
    }

    /// <summary>
    /// 通常弾生成しても良いかどうか
    /// </summary>
    protected override bool CanCreateNormalBullet(int count)
    {
        return base.CanCreateNormalBullet(count)
            && (SceneChanger.currentScene as MultiBattleScene).CanCreateNormalBullet(count);
    }

    /// <summary>
    /// 弾丸発射時
    /// </summary>
    public override void OnShoot(Bullet bullet)
    {
        bullet.SetTimeStamp(BattleGlobal.GetTimeStamp());
        bullet.SetOnDestroyCallback(this.OnDestroyBullet);

        //発射した弾丸の速度や角度の情報
        var data = new BulletDto();
        data.bulletBase = bullet.bulletBase;
        data.isNormalBullet = bullet is NormalBullet;
        data.id = this.shootCount;
        data.timeStamp = bullet.bulletBase.movement.timeStamp;
        data.speed = bullet.bulletBase.movement.speed;
        data.bulletLocalPosition = bullet.transform.localPosition;
        data.bulletLocalEulerAngles = bullet.transform.localEulerAngles;
        data.barrelLocalEulerAngles = this.turretBase.rotationParts.localEulerAngles;
        this.bulletList.Add(data);

        //プロパティ更新
        PhotonNetwork.LocalPlayer.SetCustomProperties(new PhotonHashtable{
            { PlayerPropertyKey.BulletList, this.bulletList.ToBinary() }
        });

        this.shootCount++;
    }

    /// <summary>
    /// 波動砲発射時
    /// </summary>
    public override void OnShoot(LaserBeamBullet bullet)
    {
        //自分しかいないのでイベント送信の必要なし
        if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
        {
            return;
        }

        var dto = new BulletDto();
        dto.timeStamp = BattleGlobal.GetTimeStamp();
        dto.bulletLocalPosition = bullet.transform.localPosition;
        dto.bulletLocalEulerAngles = bullet.transform.localEulerAngles;
        dto.barrelLocalEulerAngles = this.turretBase.rotationParts.localEulerAngles;

        //イベント送信
        PhotonNetwork.RaiseEvent(
            (byte)MultiEventCode.ShootLaserBeam,
            dto.GetBinary(),
            RaiseEventOptions.Default,
            SendOptions.SendReliable
        );
    }

    /// <summary>
    /// 範囲ボム弾発射時
    /// </summary>
    public override void OnShoot(BombBullet bullet)
    {
        //自分しかいないのでイベント送信の必要なし
        if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
        {
            return;
        }

        var dto = new BombBulletDto();
        dto.timeStamp = bullet.timeStamp;
        dto.dropPosition = BattleGlobal.instance.viewRotation * bullet.dropPosition;
        dto.barrelLocalEulerAngles = this.turretBase.rotationParts.localEulerAngles;

        PhotonNetwork.RaiseEvent(
            (byte)MultiEventCode.ShootBomb,
            dto.GetBinary(),
            RaiseEventOptions.Default,
            SendOptions.SendReliable
        );
    }

    /// <summary>
    /// 全体弾発射時
    /// </summary>
    public override void OnShoot(AllRangeBullet bullet)
    {
        //自分しかいないのでイベント送信の必要なし
        if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
        {
            return;
        }

        PhotonNetwork.RaiseEvent(
            (byte)MultiEventCode.ShootAllRange,
            0,
            RaiseEventOptions.Default,
            SendOptions.SendReliable
        );
    }

    /// <summary>
    /// 弾丸破棄時
    /// </summary>
    private void OnDestroyBullet(Bullet bullet)
    {
        //リストから除去
        this.bulletList.RemoveAll(x => x.bulletBase == bullet.bulletBase);

        //プロパティ更新
        PhotonNetwork.LocalPlayer.SetCustomProperties(new PhotonHashtable{
            { PlayerPropertyKey.BulletList, this.bulletList.ToBinary() }
        });
    }

    /// <summary>
    /// 弾丸リスト更新時
    /// </summary>
    public void OnUpdateBulletDataList(object value)
    {
        //弾丸リスト
        var list = BinaryUtility.CreateArray<BulletDto>((byte[])value);

        //発射
        for (int i = 0; i < list.Length; i++)
        {
            var data = list[i];
            if (!this.bulletList.Exists(x => x.id == data.id))
            {
                this.CreateBullet(data);
                this.PlayBulletFiringAnimation();
                this.StartBarrelRotation(data.barrelLocalEulerAngles);
            }
        }

        //破棄
        for (int i = 0; i < this.bulletList.Count; i++)
        {
            var data = this.bulletList[i];
            if (!list.Any(x => x.id == data.id))
            {
                Destroy(data.bulletBase.gameObject);
                this.bulletList.RemoveAt(i);
                i--;
            }
        }
    }

    /// <summary>
    /// FVA貫通弾チャージエフェクト生成時
    /// </summary>
    public override GameObject CreateFVAPenetrationChargeEffect(bool raiseEvent)
    {
        if (raiseEvent && PhotonNetwork.CurrentRoom.PlayerCount > 1)
        {
            //イベント送信
            PhotonNetwork.RaiseEvent(
                (byte)MultiEventCode.CreateFVAPenetrationChargeEffect,
                0,
                RaiseEventOptions.Default,
                SendOptions.SendReliable
            );
        }

        return base.CreateFVAPenetrationChargeEffect(false);
    }

    /// <summary>
    /// 波動砲発射イベント受信時
    /// </summary>
    public void OnShootLaserBeam(object value)
    {
        var dto = new BulletDto();
        dto.SetBinary((byte[])value);

        //弾丸生成
        var bulletPrefab = this.fvaBulletAsset.handle.asset as BulletBase;
        var bulletBase = this.turretBase.CreateBullet(bulletPrefab, BattleGlobal.instance.bulletArea);
        bulletBase.transform.localPosition = this.cachedTransform.localRotation * dto.bulletLocalPosition;
        bulletBase.transform.rotation = this.cachedTransform.localRotation * Quaternion.Euler(dto.bulletLocalEulerAngles);
        bulletBase.SetParticleLayer(BattleGlobal.instance.bulletCanvas);

        //砲身回転
        this.StartBarrelRotation(dto.barrelLocalEulerAngles);

        //制御開始
        var controller = new FvAttackLaserBeam.Controller();
        this.fvaControllers.Add(controller);
        controller.Start(true, this, bulletBase, null);
    }

    /// <summary>
    /// ボム発射イベント受信時
    /// </summary>
    public void OnShootBomb(object value)
    {
        var dto = new BombBulletDto();
        dto.SetBinary((byte[])value);

        //弾丸生成
        var bulletPrefab = this.fvaBulletAsset.handle.asset as BulletBase;
        var bulletBase = this.turretBase.CreateBullet(bulletPrefab, BattleGlobal.instance.bulletArea);
        bulletBase.transform.up = this.cachedTransform.localRotation * Vector2.up;
        bulletBase.SetParticleLayer(BattleGlobal.instance.bulletCanvas);

        //砲身回転
        this.StartBarrelRotation(dto.barrelLocalEulerAngles);

        //制御開始
        var controller = new BombBullet.Controller();
        this.fvaControllers.Add(controller);
        controller.Start(this, bulletBase, BattleGlobal.instance.viewRotation * dto.dropPosition, null, null);
    }

    /// <summary>
    /// 全体弾発射イベント受信時
    /// </summary>
    public void OnShootAllRange()
    {
        //弾丸生成
        var bulletPrefab = this.fvaBulletAsset.handle.asset as BulletBase;
        var bulletBase = this.turretBase.CreateBullet(bulletPrefab, BattleGlobal.instance.bulletArea);
        bulletBase.transform.localPosition = Vector3.zero;
        bulletBase.transform.up = Vector3.up;
        bulletBase.SetParticleLayer(BattleGlobal.instance.bulletCanvas);

        //制御開始
        var controller = new AllRangeBullet.Controller();
        this.fvaControllers.Add(controller);
        controller.Start(this, bulletBase, null, null);
    }

    /// <summary>
    /// 砲身回転処理開始
    /// </summary>
    private void StartBarrelRotation(Vector3 endAngles)
    {
        //既に処理中だったら止める
        if (this.barrelRotationCoroutine != null)
        {
            StopCoroutine(this.barrelRotationCoroutine);
            this.barrelRotationCoroutine = null;
        }

        //回転処理開始
        this.barrelRotationCoroutine = StartCoroutine(this.BarrelRotaion(endAngles));
    }

    /// <summary>
    /// 砲身回転処理
    /// </summary>
    private IEnumerator BarrelRotaion(Vector3 endAngles)
    {
        Quaternion q1 = Quaternion.AngleAxis(this.turretBase.rotationParts.localEulerAngles.z, Vector3.forward);
        Quaternion q2 = Quaternion.AngleAxis(endAngles.z, Vector3.forward);
        float time = 0f;
        float maxTime = 0.1f;

        while (time < maxTime)
        {
            this.turretBase.rotationParts.localRotation = Quaternion.Slerp(q1, q2, time / maxTime);
            time += Time.deltaTime;
            yield return null;
        }

        this.turretBase.rotationParts.localEulerAngles = endAngles;
        this.barrelRotationCoroutine = null;
    }

    /// <summary>
    /// 着弾エフェクト生成
    /// </summary>
    public void CreateLandingEffect(object value)
    {
        var dto = new LandingEffectDto();
        dto.SetBinary((byte[])value);

        var prefab = this.turretBase.bulletPrefab.landingEffectPrefab;
        var effect = Instantiate(prefab, BattleGlobal.instance.landingEffectArea, false);
        effect.transform.localPosition = this.cachedTransform.localRotation * dto.localPosition;
    }

    /// <summary>
    /// 発射済み砲弾の破棄
    /// </summary>
    private void ClearBulletList()
    {
        for (int i = 0; i < this.bulletList.Count; i++)
        {
            if (this.bulletList[i].bulletBase != null)
            {
                Destroy(this.bulletList[i].bulletBase.gameObject);
                this.bulletList[i].bulletBase = null;
            }
        }
        this.bulletList.Clear();

        for (int i = 0; i < this.fvaControllers.Count; i++)
        {
            if (!this.fvaControllers[i].isFinished)
            {
                this.fvaControllers[i].Delete();
            }
        }
        this.fvaControllers.Clear();
    }

    /// <summary>
    /// FVA更新
    /// </summary>
    public void UpdateFVA(float deltaTime)
    {
        for (int i = 0; i < this.fvaControllers.Count; i++)
        {
            this.fvaControllers[i].Update(deltaTime);

            if (this.fvaControllers[i].isFinished)
            {
                this.fvaControllers.RemoveAt(i);
                i--;
            }
        }
    }
}

/// <summary>
/// FVA制御インターフェース
/// </summary>
public interface IFVAController
{
    bool isFinished { get; }
    void Delete();
    void Update(float deltaTime);
}

}