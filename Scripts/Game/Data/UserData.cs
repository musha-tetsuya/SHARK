using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// ユーザーデータ
/// </summary>
public class UserData
{
    private static UserData instance = null;

    /// <summary>
    /// ユーザーID
    /// </summary>
    public int userId;
    /// <summary>
    /// ハッシュ
    /// </summary>
    public string hash;
    /// <summary>
    /// パスワード
    /// </summary>
    public string password;
    /// <summary>
    /// 名前
    /// </summary>
    public string name;
    /// <summary>
    /// レベル
    /// </summary>
    public uint lv;
    /// <summary>
    /// EXP
    /// </summary>
    public uint exp;
    /// <summary>
    /// コイン
    /// </summary>
    public ulong coin;
    /// <summary>
    /// 無償ジェム
    /// </summary>
    public ulong freeGem;
    /// <summary>
    /// 有償ジェム
    /// </summary>
    public ulong chargeGem;
    /// <summary>
    /// FVポイント
    /// </summary>
    public int fvPoint;
    /// <summary>
    /// VIPレベル
    /// </summary>
    public uint vipLevel;
    /// <summary>
    /// Vipレベル経験値
    /// </summary>
    public uint vipExp;
    /// <summary>
    /// 所持アイテムデータ
    /// </summary>
    public List<UserItemData> itemData;
    /// <summary>
    /// 砲台データ
    /// </summary>
    public UserTurretData[] turretData;
    public UserPartsData[] batteryData;
    public UserPartsData[] barrelData;
    public UserPartsData[] bulletData;
    public UserPartsData[] accessoriesData;
    public UserGearData[] gearData;
    /// <summary>
    /// tUtilityData、現在はパーツとギアの拡張に使用中
    /// </summary>
    public TutilityData[] tUtilityData;
    /// <summary>
    /// 有償+無償ジェム合計値
    /// </summary>
    public ulong totalGem => this.freeGem + this.chargeGem;

    /// <summary>
    /// データ取得
    /// </summary>
    public static UserData Get()
    {
        return instance;
    }

    /// <summary>
    /// データ設定：ログイン時に
    /// </summary>
    public static void Set(UserData userData)
    {
        instance = userData;
    }

    /// <summary>
    /// ログインに必要な情報の端末への保存
    /// </summary>
    public void Save()
    {
#if !SHARK_OFFLINE
        PlayerPrefs.SetInt("userId", this.userId);
        PlayerPrefs.SetString("hash", this.hash);
        PlayerPrefs.SetString("password", this.password);
#endif
    }

    /// <summary>
    /// ログインに必要な情報の端末からの読込
    /// </summary>
    public void Load()
    {
        if (!PlayerPrefs.HasKey("userId"))   return;
        if (!PlayerPrefs.HasKey("hash"))     return;
        if (!PlayerPrefs.HasKey("password")) return;

        this.userId = PlayerPrefs.GetInt("userId");
        this.hash = PlayerPrefs.GetString("hash");
        this.password = PlayerPrefs.GetString("password");
    }

#if UNITY_EDITOR
    /// <summary>
    /// ユーザーデータ削除
    /// </summary>
    [UnityEditor.MenuItem("Tools/Delete UserData")]
    private static void DeleteUserData()
    {
        PlayerPrefs.DeleteKey("userId");
        PlayerPrefs.DeleteKey("hash");
        PlayerPrefs.DeleteKey("password");
    }
#endif

    /// <summary>
    /// サーバーの情報で更新
    /// </summary>
    public void Set(TUsers tUsers)
    {
        this.name = tUsers.userName;
        this.lv = tUsers.level;
        this.exp = tUsers.exp;
        //this.chargeGem = tUsers.gem;
        this.freeGem = tUsers.freeGem;
        this.coin = tUsers.coin;
        this.fvPoint = tUsers.fv;
        this.vipLevel = tUsers.vipLevel;
        this.vipExp = tUsers.vipExp;
    }

    /// <summary>
    /// 有償石の情報を更新
    /// </summary>
    public void Set(UserGemData tGem)
    {
        this.chargeGem = tGem.gem;
    }

    /// <summary>
    /// ログイン時のuserIDとhashをサーバーの情報で更新
    /// </summary>
    public void Set(LoginApi.LoginUserData loginUserData)
    {
        this.userId = loginUserData.userId;
        this.hash = loginUserData.hash;
    }

    /// <summary>
    /// アイテムインベントリ拡張API実行時、データ更新
    /// </summary>
    public void Set(TutilityData utilityData)
    {
        var type = utilityData.utilityType;
        var list = this.tUtilityData.ToList();
        var index = list.FindIndex(x => x.utilityType == utilityData.utilityType);

        this.tUtilityData[index].utilityId = utilityData.utilityId;
    }

    /// <summary>
    /// 砲台系データセット
    /// </summary>
    public void SetTurretData(
        UserTurretData[] turretData,
        UserBatteryPartsData[] batteryData,
        UserBarrelPartsData[] barrelData,
        UserBulletPartsData[] bulletData,
        UserAccessoryPartsData[] accessoryData,
        UserGearData[] gearData)
    {
        this.turretData = turretData;
        this.batteryData = batteryData;
        this.barrelData = barrelData;
        this.bulletData = bulletData;
        this.gearData = gearData;
        this.accessoriesData = accessoryData;
    }

    /// <summary>
    /// ログイン時のユーザーデータを更新
    /// </summary>
    public void Set(FirstApi.FirstUserResponseData firstUserData)
    {
        Set(firstUserData.tUsers);
        Set(firstUserData.tGem);
        itemData = firstUserData.tItem;
        tUtilityData = firstUserData.tUtility;

        //砲台パーツ系セット
        this.SetTurretData(
            firstUserData.tCannonSetting,
            firstUserData.tCannonBattery,
            firstUserData.tCannonBarrel,
            firstUserData.tCannonBullet,
            firstUserData.tCannonAccessories,
            firstUserData.tGear
        );
    }

    /// <summary>
    /// アイテム付与
    /// </summary>
    public void AddItem(ItemType itemType, uint itemId, ulong amount)
    {
        switch (itemType)
        {
            case ItemType.Battery:
            case ItemType.Barrel:
            case ItemType.Bullet:
            case ItemType.Accessory:
            case ItemType.CannonSet:
            case ItemType.Gear:
                //ここで増やすことは出来ないので、通信で更新をかけるようフラグを立てておく
                TurretApi.isNeedRefleshCannonUser = true;
                break;

            case ItemType.ChargeGem:
                this.chargeGem += amount;
                break;

            case ItemType.FreeGem:
                this.freeGem += amount;
                break;

            case ItemType.Coin:
                this.coin += amount;
                break;

            case ItemType.BattleItem:
                var item = this.itemData.Find(x => x.itemId == itemId);
                if (item == null)
                {
                    item = new UserItemData();
                    item.itemType = itemType;
                    item.itemId = itemId;
                    item.stockCount = (uint)amount;
                    this.itemData.Add(item);
                }
                else
                {
                    item.stockCount += (uint)amount;
                }
                break;

            case ItemType.JackpotTotalCoin:
                //スルー
                break;

            default:
                Debug.LogErrorFormat("未対応のアイテムタイプ: {0}", itemType);
                break;
        }
    }

    /// <summary>
    /// アイテムセット
    /// </summary>
    public void SetItem(UserItemData tItem)
    {
        var item = this.itemData.Find(x => x.itemId == tItem.itemId);
        if (item == null)
        {
            this.itemData.Add(tItem);
        }
        else
        {
            item.stockCount = tItem.stockCount;
        }
    }

    /// <summary>
    /// 使用中砲台の取得
    /// </summary>
    public UserTurretData GetSelectedTurretData()
    {
        return this.turretData.FirstOrDefault(x => x.useFlg > 0);
    }

    /// <summary>
    /// 現在のコイン枚数取得
    /// </summary>
    public long GetCurrentCoin()
    {
        return (SceneChanger.currentScene is Battle.MultiBattleScene)
             ? (SceneChanger.currentScene as Battle.MultiBattleScene).coin
             : (long)this.coin;
    }

    /// <summary>
    /// 言語取得
    /// </summary>
    public static Language GetLanguage()
    {
#if LANGUAGE_ZH
        Language language = Language.Zh;
#elif LANGUAGE_TW
        Language language = Language.Tw;
#elif LANGUAGE_EN
        Language language = Language.En;
#else
        Language language = Language.Ja;
#endif
        if (PlayerPrefs.HasKey("language"))
        {
            language = (Language)PlayerPrefs.GetInt("language");
        }
        else
        {
            PlayerPrefs.SetInt("language", (int)language);
        }

        return language;
    }

    /// <summary>
    /// BGM音量
    /// </summary>
    public static int bgmVolume
    {
        get => PlayerPrefs.GetInt("bgmValue", 4);
        set => PlayerPrefs.SetInt("bgmValue", value);
    }

    /// <summary>
    /// SE音量
    /// </summary>
    public static int seVolume
    {
        get => PlayerPrefs.GetInt("seValue", 4);
        set => PlayerPrefs.SetInt("seValue", value);
    }
}
