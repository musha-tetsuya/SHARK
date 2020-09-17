using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Battle {

/// <summary>
/// バトル管理
/// </summary>
public class BattleGlobal : MonoBehaviour
{
    /// <summary>
    /// インスタンス
    /// </summary>
    public static BattleGlobal instance { get; private set; }

    /// <summary>
    /// 魚用カメラ
    /// </summary>
    [SerializeField]
    public Camera fishCamera = null;
    /// <summary>
    /// UIカメラ
    /// </summary>
    [SerializeField]
    public Camera uiCamera = null;
    /// <summary>
    /// 弾丸キャンバス
    /// </summary>
    [SerializeField]
    public Canvas bulletCanvas = null;
    /// <summary>
    /// UIキャンバス
    /// </summary>
    [SerializeField]
    public Canvas uiCanvas = null;
    /// <summary>
    /// 魚エリアの最前Z値
    /// </summary>
    [SerializeField]
    private float fishAreaNear = -3f;
    /// <summary>
    /// 魚エリアの最奥Z値
    /// </summary>
    [SerializeField]
    private float fishAreaFar = 3f;
    /// <summary>
    /// 魚プレハブ
    /// </summary>
    [SerializeField]
    private Fish fishPrefab = null;
    /// <summary>
    /// 魚当たり判定用コライダプレハブ
    /// </summary>
    [SerializeField]
    private FishCollider2D fishColliderPrefab = null;
    /// <summary>
    /// 魚生成場所
    /// </summary>
    [SerializeField]
    public Transform fishArea = null;
    /// <summary>
    /// 魚当たり判定用コライダ生成場所
    /// </summary>
    [SerializeField]
    private RectTransform fishColliderArea = null;
    /// <summary>
    /// 砲台イベントトリガ
    /// </summary>
    [SerializeField]
    public TurretEventTrigger turretEventTrigger = null;
    /// <summary>
    /// 弾丸生成先
    /// </summary>
    [SerializeField]
    public RectTransform bulletArea = null;
    /// <summary>
    /// 着弾エフェクト生成先
    /// </summary>
    [SerializeField]
    public RectTransform landingEffectArea = null;
    /// <summary>
    /// FVアタック生成先
    /// </summary>
    [SerializeField]
    public RectTransform fvAttackArea = null;
    /// <summary>
    /// 自動照準マーカープレハブ
    /// </summary>
    [SerializeField]
    public GameObject targetMarkPrefab = null;
    /// <summary>
    /// 魚基本レイヤー
    /// </summary>
    [SerializeField, Layer]
    public int fishNormalLayer = 0;
    /// <summary>
    /// 魚氷結レイヤー
    /// </summary>
    [SerializeField, Layer]
    public int fishIceLayer = 0;
    /// <summary>
    /// 魚被ダメ時レイヤー
    /// </summary>
    [SerializeField, Layer]
    public int fishDamagedLayer = 0;

    /// <summary>
    /// UIキャンバスのRectTransform
    /// </summary>
    private RectTransform uiCanvasRectTransform = null;
    /// <summary>
    /// 想定画面アスペクト比
    /// </summary>
    public float virtualAspect { get; private set; }
    /// <summary>
    /// 魚用カメラZ位置
    /// </summary>
    private float fishCameraPosZ = 0f;
    /// <summary>
    /// 魚用カメラの画角tan値
    /// </summary>
    private float fishCameraTan = 1f;
    /// <summary>
    /// 魚リスト
    /// </summary>
    [NonSerialized]
    public List<Fish> fishList = new List<Fish>();
    private List<Fish> updateFishList = new List<Fish>();
    /// <summary>
    /// バトル用ユーザーデータ
    /// </summary>
    public BattleUserData userData = null;
    /// <summary>
    /// 見え方回転値
    /// </summary>
    [NonSerialized]
    public Quaternion viewRotation = Quaternion.identity;

    /// <summary>
    /// 魚ユニットの位置補正値
    /// </summary>
    public Vector3[][] fishUnitPositions = new Vector3[][]
    {
        //1匹のとき
        new Vector3[]
        {
            Vector3.zero,
        },
        //2匹のとき
        new Vector3[]
        {
            Vector3.left,
            Vector3.right,
        },
        //3匹のとき
        new Vector3[]
        {
            Vector3.up,
            Quaternion.Euler(0, 0, 120) * Vector3.up,
            Quaternion.Euler(0, 0,-120) * Vector3.up,
        },
        //4匹のとき
        new Vector3[]
        {
            Vector3.up,
            Vector3.left,
            Vector3.right,
            Vector3.down,
        },
        //5匹のとき
        new Vector3[]
        {
            Vector3.up,
            Vector3.left,
            Vector3.zero,
            Vector3.right,
            Vector3.down,
        },
    };

    /// <summary>
    /// OnDestroy
    /// </summary>
    private void OnDestroy()
    {
        instance = null;
    }

    /// <summary>
    /// Awake
    /// </summary>
    private void Awake()
    {
        instance = this;
        Application.targetFrameRate = SharkDefine.FRAMERATE;
        this.uiCanvasRectTransform = this.uiCanvas.transform as RectTransform;
        var referenceResolution = this.uiCanvas.GetComponent<CanvasScaler>().referenceResolution;
        this.virtualAspect = referenceResolution.x / referenceResolution.y;
        this.fishCameraPosZ = this.fishCamera.transform.position.z;
        this.fishCameraTan = Mathf.Tan(this.fishCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
    }

    /// <summary>
    /// タイムスタンプ取得
    /// </summary>
    public static int GetTimeStamp()
    {
        if (SceneChanger.currentScene is MultiBattleScene)
        {
            return Photon.Pun.PhotonNetwork.ServerTimestamp;
        }
        return 0;
    }

    /// <summary>
    /// DeltaTime取得
    /// </summary>
    public static float GetDeltaTime(ref int timeStamp)
    {
        if (Photon.Pun.PhotonNetwork.IsConnected)
        {
            int oldTime = timeStamp;
            timeStamp = Photon.Pun.PhotonNetwork.ServerTimestamp;
            return unchecked(timeStamp - oldTime) * Masters.MilliSecToSecond;
        }
        return Time.deltaTime;
    }

    /// <summary>
    /// DeltaTime取得
    /// </summary>
    public static float GetDeltaTime(int timeStamp)
    {
        if (Photon.Pun.PhotonNetwork.IsConnected)
        {
            return unchecked(Photon.Pun.PhotonNetwork.ServerTimestamp - timeStamp) * Masters.MilliSecToSecond;
        }
        return Time.deltaTime;
    }

    /// <summary>
    /// 実機画面サイズの取得
    /// </summary>
    private static Vector2 GetRealScreenSize()
    {
        var size = Vector2.zero;
#if UNITY_EDITOR
        var screenRes = UnityEditor.UnityStats.screenRes.Split('x');
        size.x = int.Parse(screenRes[0]);
        size.y = int.Parse(screenRes[1]);
#else
        size.x = Screen.width;
        size.y = Screen.height;
#endif
        return size;
    }

    /// <summary>
    /// 魚用カメラの描画範囲設定
    /// </summary>
    public void SetFishCameraViewport()
    {
        //実機画面サイズ
        var realScreenSize = GetRealScreenSize();
        //実機画面比率
        float realAspect = realScreenSize.x / realScreenSize.y;
        //魚用カメラの描画範囲
        Rect viewportRect = new Rect(0f, 0f, 1f, 1f);

        //想定画面より実機画面が縦長の場合
        if (realAspect < this.virtualAspect)
        {
            viewportRect.height = realAspect / this.virtualAspect;
            viewportRect.y = (1f - viewportRect.height) * 0.5f;
        }
        //想定画面より実機画面が横長の場合
        else
        {
            viewportRect.width = this.virtualAspect / realAspect;
            viewportRect.x = (1f - viewportRect.width) * 0.5f;
        }

        //魚用カメラの描画範囲設定
        this.fishCamera.rect = viewportRect;
    }

    /// <summary>
    /// スクリーン座標をUIキャンバス内での座標に変換
    /// </summary>
    public Vector2 ScreenToCanvasPoint(Vector2 screenPoint)
    {
        Vector2 canvasPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(this.uiCanvasRectTransform, screenPoint, this.uiCamera, out canvasPoint);
        return canvasPoint;
    }

    /// <summary>
    /// ワールド座標をUIキャンバス内での座標に変換
    /// </summary>
    public Vector2 WorldToCanvasPoint(Vector3 worldPoint)
    {
        Vector2 screenPoint = this.fishCamera.WorldToScreenPoint(worldPoint);
        return this.ScreenToCanvasPoint(screenPoint);
    }

    /// <summary>
    /// 魚エリア内ランダム位置取得
    /// </summary>
    public Vector3 GetRandomFishAreaPosition()
    {
        Vector3 pos;

        pos.z = UnityEngine.Random.Range(this.fishAreaNear, this.fishAreaFar);

        float w = this.fishCameraTan * Mathf.Abs(this.fishCameraPosZ - pos.z) * 2 * this.virtualAspect;
        float h = w / this.virtualAspect;

        pos.x = UnityEngine.Random.Range(-w, w) * 0.5f;
        pos.y = UnityEngine.Random.Range(-h, h) * 0.5f;

        return pos;
    }

    /// <summary>
    /// 魚エリア内位置に変換
    /// <param name="normalized">手前左下を(0,0,0), 奥右上を(1,1,1)とする</param>
    public Vector3 ToFishAreaPosition(Vector3 normalized)
    {
        Vector3 pos;

        //ZはNearからFarの間の値
        pos.z = Mathf.Lerp(this.fishAreaNear, this.fishAreaFar, normalized.z);

        //Zの時、カメラの画角から画面幅と高さを計算
        float w = this.fishCameraTan * Mathf.Abs(this.fishCameraPosZ - pos.z) * 2 * this.virtualAspect;
        float h = w / this.virtualAspect;

        //X,Y値を決定
        pos.x = w * (normalized.x - 0.5f);
        pos.y = h * (normalized.y - 0.5f);

        return pos;
    }

    /// <summary>
    /// 魚生成
    /// </summary>
    public Fish CreateFish(Fish.Loader fishLoader, Master.FishData fishMaster, Fish.ID id)
    {
        //魚（外側）生成
        var fish = Instantiate(this.fishPrefab, this.fishArea, false);
        //IDセット
        fish.id = id;
        //マスターセット
        fish.SetMaster(fishMaster);
        //モデルアタッチ
        fish.SetModel(fishLoader);

        if (this.fishColliderPrefab != null)
        {
            //魚当たり判定用コライダ生成
            var collider = Instantiate(this.fishColliderPrefab, this.fishColliderArea, false);
            //コライダアタッチ
            fish.SetCollider(collider, fishLoader.colliderData);
        }

        //破棄時コールバック設定
        fish.onDestroy += this.OnDestroyFish;

        this.fishList.Add(fish);

        //魚を生成したことを通知
        (SceneChanger.currentScene as BattleSceneBase)?.OnCreateFish(fish);

        return fish;
    }

    /// <summary>
    /// 魚破棄時
    /// </summary>
    private void OnDestroyFish(Fish fish)
    {
        this.fishList.Remove(fish);
        Destroy(fish.gameObject);
    }

    /// <summary>
    /// 魚Update
    /// </summary>
    public void UpdateFish(float deltaTime)
    {
        int imax = this.fishList.Count;

        for (int i = 0; i < imax; i++)
        {
            if (i < this.updateFishList.Count)
            {
                this.updateFishList[i] = this.fishList[i];
            }
            else
            {
                this.updateFishList.Add(this.fishList[i]);
            }
        }

        for (int i = 0; i < imax; i++)
        {
            this.updateFishList[i].ManualUpdate(deltaTime);
        }
    }

    /// <summary>
    /// 基礎回遊ルート取得
    /// </summary>
    public Vector3[] GetBaseMigrationRoute(FishRouteData routeData)
    {
        int pointSize = routeData.pathCreator.path.NumPoints;
        var baseRoute = new Vector3[pointSize * (routeData.loopCount + 1)];

        //基本回遊ルートの作成
        for (int i = 0; i < pointSize; i++)
        {
            int j = routeData.isReverse ? pointSize - 1 - i : i;

            //この時点では-0.5～+0.5の範囲の値
            baseRoute[i] = routeData.GetPoint(j);
            //0f～1fの間に補正
            baseRoute[i] += Vector3.one * 0.5f;
            //エリア内補正
            baseRoute[i] = this.ToFishAreaPosition(baseRoute[i]);
            //見え方回転補正
            baseRoute[i] = this.viewRotation * baseRoute[i];
        }

        //ループ回数分、回遊ルートを追加
        for (int i = 0; i < routeData.loopCount; i++)
        {
            int offset = pointSize * (i + 1);

            for (int j = 0; j < pointSize; j++)
            {
                baseRoute[offset + j] = (i % 2 == 0)
                    ? baseRoute[pointSize - 1 - j]
                    : baseRoute[j];
            }
        }

        return baseRoute;
    }

    /// <summary>
    /// 回遊ルート取得
    /// </summary>
    public Vector3[][] GetMigrationRoute(Vector3[] baseRoute, int unitSize, float unitSpread)
    {
        unitSize = Mathf.Clamp(unitSize, 1, 5);
        var routes = new Vector3[unitSize][];

        //基本回遊ルートの向きからユニット別位置補正値の回転値を決める
        Vector3 forward = baseRoute[1] - baseRoute[0];
        float angle = Vector2.SignedAngle(Vector2.up, forward);
        Quaternion rot = Quaternion.Euler(0f, 0f, angle);

        //ユニット別回遊ルートの作成
        for (int i = 0; i < unitSize; i++)
        {
            //ユニット位置
            Vector3 unitPosition = rot * this.fishUnitPositions[unitSize - 1][i];

            //基本回遊ルートをユニット位置で補正する
            routes[i] = baseRoute
                .Select(p => p + unitPosition * unitSpread)
                .ToArray();
        }

        return routes;
    }

    /// <summary>
    /// 回遊ルート取得
    /// </summary>
    public Vector3[][] GetMigrationRoute(FishRouteData routeData)
    {
        var baseRoute = this.GetBaseMigrationRoute(routeData);
        var routes = this.GetMigrationRoute(baseRoute, (int)routeData.unitSize, routeData.unitSpread);
        return routes;
    }

    /// <summary>
    /// 魚へのダメージ計算
    /// </summary>
    public static int GetDamage(Fish fish, Bullet bullet)
    {
        //基本攻撃力がそのままダメージ
        int damage = (int)bullet.power;

        //スキル効果によるダメージ増加
        damage += (int)bullet.skill.DamageUp(bullet.power, fish);

        return damage;
    }

}//class BattleGlobal

}//namespace Battle