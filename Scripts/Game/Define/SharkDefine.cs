using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SHARKにおける共通定義
/// </summary>
public static class SharkDefine
{
    public const int SCREEN_WIDTH = 1920;
    public const int SCREEN_HEIGHT = 1080;
    public const float SCREEN_ASPECT_WH = SCREEN_WIDTH / SCREEN_HEIGHT;
    public const float SCREEN_ASPECT_HW = SCREEN_HEIGHT / SCREEN_WIDTH;
    public const int FRAMERATE = 60;
    public const float DELTATIME = 1f / FRAMERATE;
    public const int MAX_GEAR_SLOT_SIZE = 3;
    public const uint MAX_POWER = 500;              //各能力のMAX値（仮）
    public const uint MAX_BULLET_SPEED = 2000;      //各能力のMAX値（仮）
    public const uint MAX_FV_POINT_GET_VALUE = 10;  //各能力のMAX値（仮）
    public const string YES_BTN_SPRITE_NAME = "CmBtn_010_0001";
    public const string NO_BTN_SPRITE_NAME = "CmBtn_010_0002";
#if UNITY_IOS && !UNITY_EDITOR
    public const int DEVICE_TYPE = 1;
#else
    public const int DEVICE_TYPE = 2;
#endif

    /// <summary>
    /// SEクリップのパスを取得する
    /// </summary>
    public static string GetSeClipPath(string key) => string.Format("Sound/Se/{0}", key);
    /// <summary>
    /// BGMクリップのパスを取得する
    /// </summary>
    public static string GetBgmClipPath(string key) => string.Format("Sound/Bgm/{0}", key);
    /// <summary>
    /// シングルステージアイコンスプライトパスを取得する
    /// </summary>
    public static string GetSingleStageIconSpritePath(string key) => string.Format("Textures/Thumbnail/SStageIcon/{0}", key);
    /// <summary>
    /// ステージセレクト背景スプライトのパスを取得する
    /// </summary>
    public static string GetStageSelectBgSpritePath(string key) => string.Format("Textures/StageSelectBg/{0}", key);
    /// <summary>
    /// バトルWAVEデータのパスを取得する
    /// </summary>
    public static string GetFishWaveDataPath(string key) => string.Format("ScriptableObject/FishWaveData/{0}", key);
    /// <summary>
    /// マルチバトル用WAVEデータのパスを取得する
    /// </summary>
    public static string GetMultiFishWaveGroupDataPath(string key) => string.Format("ScriptableObject/MultiFishWaveGroupData/{0}", key);
    /// <summary>
    /// ランダム回遊ルートデータのパスを取得する
    /// </summary>
    public static string GetRandomFishRouteDataPath(string key) => string.Format("ScriptableObject/RandomFishRouteData/{0}", key);
    /// <summary>
    /// バトル背景スプライトのパスを取得する
    /// </summary>
    public static string GetBattleBgSpritePath(string key) => string.Format("Textures/BattleBg/{0}", key);
    /// <summary>
    /// 魚サムネイルスプライトのパスを取得する
    /// </summary>
    public static string GetFishThumbnailSpritePath(string key) => string.Format("Textures/Thumbnail/Fish/{0}", key);
    /// <summary>
    /// 魚図鑑の背景スプライトのパスを取得する
    /// </summary>
    public static string GetZukanBgSpritePath(string key) => string.Format("Textures/Thumbnail/ZukanBg/{0}", key);
    /// <summary>
    /// 砲台セットスプライトのパスを取得する
    /// </summary>
    public static string GetTurretSetSpritePath(string key) => string.Format("Textures/Thumbnail/TurretSet/{0}", key);
    /// <summary>
    /// 台座スプライトのパスを取得する
    /// </summary>
    public static string GetBatterySpritePath(string key) => string.Format("Textures/Thumbnail/Battery/{0}", key);
    /// <summary>
    /// 台座プレハブのパスを取得する
    /// </summary>
    public static string GetBatteryPrefabPath(string key) => string.Format("Prefabs/Turret/{0}/{0}_Battery", key);
    /// <summary>
    /// 砲身スプライトのパスを取得する
    /// </summary>
    public static string GetBarrelSpritePath(string key) => string.Format("Textures/Thumbnail/Barrel/{0}", key);
    /// <summary>
    /// 砲身プレハブのパスを取得する
    /// </summary>
    public static string GetBarrelPrefabPath(string key) => string.Format("Prefabs/Turret/{0}/{0}_Barrel", key);
    /// <summary>
    /// 砲弾サムネイルのパスを取得する
    /// </summary>
    public static string GetBulletThumbnailPath(string key) => string.Format("Textures/Thumbnail/Bullet/{0}", key);
    /// <summary>
    /// 砲弾プレハブのパスを取得する
    /// </summary>
    public static string GetBulletPrefabPath(string key) => string.Format("Prefabs/Turret/{0}/{0}_Bullet", key);
    /// <summary>
    /// アクセサリサムネイルのパスを取得する
    /// </summary>
    public static string GetAccessoryThumbnailPath(string key) => string.Format("Textures/Thumbnail/Acce/{0}", key);
    /// <summary>
    /// FVアタックプレハブのパス
    /// </summary>
    public static string GetFvAttackPrefabPath(FvAttackType fvAttackType) => string.Format("Prefabs/FvAttack/FvAttack{0}", fvAttackType);
    /// <summary>
    /// FVアタック用砲弾プレハブのパスを取得する
    /// </summary>
    public static string GetFvAttackBulletPrefabPath(string key) => string.Format("Prefabs/FVA/{0}/{0}", key);
    /// <summary>
    /// FVアタック用砲弾のチャージエフェクトプレハブのパスを取得する
    /// </summary>
    public static string GetFVAChargeEffectPrefabPath(string key) => string.Format("Prefabs/FVA/{0}/Particle/{0}_Charge", key);
    /// <summary>
    /// FVアタックタイプアイコンスプライトのパス
    /// </summary>
    public static string GetFvAttackTypeIconSpritePath(FvAttackType fvAttackType) => string.Format("Textures/FvAttackTypeIcon/{0}", fvAttackType);
    /// <summary>
    /// シリーズスキルアイコンスプライトのパス
    /// </summary>
    public static string GetSeriesSkillIconSpritePath(string key) => string.Format("Textures/SetSkillThumbnail/{0}", key);
    /// <summary>
    /// バトルアイテムアイコンスプライトのパスを取得
    /// </summary>
    public static string GetBattleItemIconSpritePath(string key) => string.Format("Textures/BattleItemIcon/{0}", key);
    /// <summary>
    /// ギアスプライトのパスを取得する
    /// </summary>
    public static string GetGearItemIconSpritePath(string key) => string.Format("Textures/Gear/{0}", key);
    /// <summary>
    /// 魚FBXのパス
    /// </summary>
    public static string GetFishFbxPath(string key) => string.Format("Models/Fish/{0}/{0}", key);
    /// <summary>
    /// 魚アニメーターコントローラのパス
    /// </summary>
    public static string GetFishAnimatorControllerPath(string key) => string.Format("Models/Fish/{0}/{0}", key);
    /// <summary>
    /// 魚コライダデータのパス
    /// </summary>
    public static string GetFishColliderDataPath(string key) => string.Format("Models/Fish/{0}/{0}-colliderdata", key);
    /// <summary>
    /// 魚パーティクルのパス
    /// </summary>
    public static string GetFishParticlePath(string key, string particleName) => string.Format("Models/Fish/{0}/Particle/{1}", key, particleName);
}

/// <summary>
/// 言語
/// </summary>
public enum Language
{
    Ja,
    Zh,
    Tw,
    En,
}

/// <summary>
/// アイテムタイプ
/// </summary>
public enum ItemType
{
    Battery     = 1,    //台座
    Barrel      = 2,    //砲身
    Bullet      = 3,    //砲弾
    Accessory   = 4,    //アクセサリー
    CannonSet   = 5,    //砲台 (台座, 砲身, 砲弾をまとめたもの)
    Gear        = 6,    //ギア
    ChargeGem   = 7,    //有償ジェム
    FreeGem     = 8,    //無償ジェム
    Coin        = 9,    //コイン
    //Utility   = 10,
    BattleItem  = 11,   //バトル用アイテム
    //Material  = 12,
    //MaterialOption = 13,
    //GashaTicket = 14,
    //GashaTicketFragment = 15,
    JackpotTotalCoin = 16,
    //JackpotBall = 17,
    //JackpotSoul = 18,
}

/// <summary>
/// ギアタイプ
/// </summary>
public enum GearType
{
    Battery = 1,    //台座
    Barrel  = 2,    //砲身
    Bullet  = 3,    //砲弾
}

/// <summary>
/// バトルアイテムタイプ
/// </summary>
public enum BattleItemType
{
    None        = 0,
    AutoTarget  = 1,    //自動照準
    Freezing    = 2,    //氷結
    Summon      = 3,    //召喚
}

/// <summary>
/// ランク/レアリティ
/// </summary>
public enum Rank
{
    None = 0,
    D    = 1,
    C    = 2,
    B    = 3,
    A    = 4,
    S    = 5,
}

/// <summary>
/// FVアタックタイプ
/// </summary>
public enum FvAttackType
{
    None        = 0,
    AllRange    = 1,    //全体攻撃
    Bomb        = 2,    //ボム
    LaserBeam   = 3,    //レーザービーム
    MultiWay    = 4,    //多方面
    Penetration = 5,    //貫通跳弾
}

/// <summary>
/// スキルタイプ
/// </summary>
public enum SkillType
{
    None            = 0,
    AutoTarget      = 1,    //自動照準
    Freezing        = 2,    //氷結
    Summon          = 3,    //召喚
    TypeKiller      = 4,    //{0}タイプの魚に{1}%ダメージ増加
    SizeKiller      = 5,    //{0}サイズの魚に{1}%ダメージ増加
    BossKiller      = 6,    //BOSS魚に{1}%ダメージ増加
    FishSpeedDown   = 7,    //ダメージを与えた魚を{0}%の確率で{1}%スピードダウン
    FishFreezing    = 8,    //ダメージを与えた魚を{0}%の確率で{1}秒間の氷結状態にする
    DamageCut       = 9,    //被ダメージ{0}%カット
    ShortenCoolTime = 10,   //アイテム再使用までの時間を{0}秒短縮
    InitFvGaugeUp   = 11,   //開戦時FVゲージが{0}%溜まった状態から開始する
    CoinGetUp       = 12,   //コイン獲得量{0}%UP
    ExpGetUp        = 13,   //経験値獲得量{0}%UP
    FvGetUp         = 14,   //FVゲージ獲得上昇量{0}%UP（マルチのみ）
}

/// <summary>
/// テキストのカラータイプ
/// </summary>
public enum TextColorType
{
    None            = 0,
    IncreaseParam   = 1,    //パラメータの増加時のカラー
    DecreaseParam   = 2,    //パラメータの減少時のカラー
    Alert           = 3,    //警告文のカラー
}

/// <summary>
/// エラーコード
/// </summary>
public enum ErrorCode
{
    BillingFatalError       = 40005,    //レシートは正しいが、マスターかクライアントが原因っぽいエラー
    BillingTimeError        = 40006,    //レシートは正しいが、マスターかクライアントが原因っぽいエラー
    ProductIdNotFound       = 40007,    //レシートは正しいが、プロダクトIDが見つからない（マスターかクライアントが原因っぽいエラー）
    AlreadyCheckReceiptId   = 40008,    //レシートは正しいが、すでに検証済みのレシート

    PresentBoxError         = 70002,    //プレゼントBox関連のとりあえずエラー
    PresentBoxNotFound      = 70003,    //プレゼントBoxから受け取るアイテムのIDに対応するものが見つからない (不正なアイテム等)
    PresentBoxClosed        = 70004,    //プレゼントBoxから受け取るアイテムの期限切れてしまっている
    PresentBoxReceived      = 70005,    //プレゼントBoxから受け取るアイテムが既に受取済み
    PresentBoxEmpty         = 70006,    //プレゼントBoxが空っぽの状態で受け取ろうとしている
    PresentBoxMaxPossession = 70007     //プレゼントBoxの所持数が上限を超えている
}

/// <summary>
/// tUtilitのタイプ
/// </summary>
public enum UtilityType
{
    None = 0,
    MaxParts = 1,
    MaxGear = 2,
    MaxCannon = 3
}
