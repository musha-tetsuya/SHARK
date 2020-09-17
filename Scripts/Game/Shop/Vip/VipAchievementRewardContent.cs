using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class VipAchievementRewardContent : MonoBehaviour
{
    /// <summary>
    /// アイテムアイコン
    /// </summary>
    [SerializeField]
    private CommonIcon commonIcon = null;
    /// <summary>
    /// アイテム名のテキスト
    /// </summary>
    [SerializeField]
    private Text nameText = null;
    /// <summary>
    /// 砲台関連のコンテンツ
    /// </summary>
    [SerializeField]
    private GameObject cannonContent = null;
    /// <summary>
    /// 攻撃力ゲージ
    /// </summary>
    [SerializeField]
    private CommonStatusGauge atkStatusGauge = null;
    /// <summary>
    /// 速度ゲージ
    /// </summary>
    [SerializeField]
    private CommonStatusGauge spdStatusGauge = null;
    /// <summary>
    /// FVポイントの上がりやすさゲージ
    /// </summary>
    [SerializeField]
    private CommonStatusGauge fvpStatusGauge = null;


    /// <summary>
    /// 報酬情報の設定
    /// </summary>
    public void SetInfo(uint itemType, uint itemId, uint itemNum)
    {
        var itemInfo = CommonIconUtility.GetItemInfo(itemType, itemId);
        bool isCannon = true;
        var config = Masters.ConfigDB.FindById(1);

        switch ((ItemType)itemType)
        {
            case ItemType.CannonSet:
                var cannonSet = itemInfo as Master.CannonSetData;
                Master.BatteryData batteryData = Masters.BatteryDB.FindById(cannonSet.batteryId);
                Master.BarrelData  barrelData  = Masters.BarrelDB.FindById(cannonSet.barrelId);
                Master.BulletData  bulletData  = Masters.BulletDB.FindById(cannonSet.bulletId);
                Master.TurretSerieseData cannonSeries = Masters.TurretSerieseDB.FindById(batteryData.seriesId);
                
                this.nameText.text = cannonSeries.name;
                
                // ゲージセット
                SetStatusGauge(
                    itemType,
                    bulletData.power, config.maxBulletPower,
                    barrelData.speed, config.maxBarrelSpeed,
                    batteryData.fvPoint, config.maxBatteryFvPoint);
                isCannon = true;
                break;
            case ItemType.Gear:
                nameText.text = itemInfo.GetName();

                // ゲージセット
                var gear = itemInfo as Master.GearData;
                SetStatusGauge(
                    itemType,
                    gear.power, config.maxGearPower,
                    gear.speed, config.maxGearSpeed,
                    gear.fvPoint, config.maxGearFvPoint);
                isCannon = true;
                break;
            case ItemType.BattleItem:
                this.nameText.text = itemInfo.GetName() + string.Format("×{0}", itemNum);
                isCannon = false;
                break;
            case ItemType.FreeGem:
                this.nameText.text = Masters.LocalizeTextDB.GetFormat("UnitGem", itemNum);
                isCannon = false;
                break;
            case ItemType.Coin:
                this.nameText.text = Masters.LocalizeTextDB.GetFormat("UnitCoin", itemNum);
                isCannon = false;
                break;
            default:
                Debug.LogError("到達報酬に想定外のItemTypeが指定されています   ItemType = " + (ItemType)itemType);
                return;
        }

        this.commonIcon.Set(itemInfo, false);
        //砲台系のコンテンツの表示・非表示切り替え
        cannonContent.SetActive(isCannon);
    }

    /// <summary>
    /// ステータスゲージの設定
    /// </summary>
    private void SetStatusGauge(uint itemType, uint nowAtk, uint maxAtk, uint nowSpd, uint maxSpd, uint nowFvp, uint maxFvp)
    {
        atkStatusGauge.SetGaugeValue(GetStatusGaugeValue(itemType, (int)nowAtk, (int)maxAtk));
        spdStatusGauge.SetGaugeValue(GetStatusGaugeValue(itemType, (int)nowSpd, (int)maxSpd));
        fvpStatusGauge.SetGaugeValue(GetStatusGaugeValue(itemType, (int)nowFvp, (int)maxFvp));
    }

    /// <summary>
    /// ステータスゲージの数値を取得
    /// </summary>
    private float GetStatusGaugeValue(uint itemType, int nowParam, int maxParam)
    {
        // ギアのゲージの場合
        if(itemType == (uint)ItemType.Gear)
        {
            //0除算防止
            if (maxParam == 0)
            {
                return 0.0f;
            }
            return Mathf.Clamp01(((float)nowParam / (float)maxParam));
        }
        // 砲台のゲージの場合
        else
        {
            //0除算防止
            if (maxParam == 0)
            {
                return 0.0f;
            }
            return Mathf.Clamp01(((float)nowParam / (float)maxParam) + 0.2f);
        }
    }
}
