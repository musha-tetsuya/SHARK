using System;
using UnityEngine;
using UnityEngine.UI;

public class ItemInventoryGearScrollViewItem : MonoBehaviour
{
    /// <summary>
    /// CommonIcon
    /// </summary>
    [SerializeField]
    private CommonIcon commonIcon = null;
    /// <summary>
    /// 装着中パンネル
    /// </summary>
    [SerializeField]
    private GameObject equippedMark = null;

    /// <summary>
    /// ロックイメージ
    /// </summary>
    [SerializeField]
    public Image lockImage = null;
    /// <summary>
    /// チェックボックス
    /// </summary>
    [SerializeField]
    private GameObject checkBox = null;
    /// <summary>
    /// チェックボックスの中のチェックイメージ
    /// </summary>
    [SerializeField]
    public Image checkImage = null;

    /// <summary>
    /// ギアイメージGraphic
    /// </summary>
    [SerializeField]
    private Graphic commonIconGearBgGraphic = null;
    [SerializeField]
    private Graphic commonIconGearMainGraphic = null;
    [SerializeField]
    private Graphic commonIconGearSubGraphic = null;
    
    /// <summary>
    /// ユーザーギアデータ
    /// </summary>
    public UserGearData gearData { get; private set; }

    /// <summary>
    /// 表示構築
    /// </summary>
    public void Set(UserGearData data, bool isEquipped, uint isLock, Action<ItemInventoryGearScrollViewItem> onClick)
    {
        this.gearData = data;
        // CommonIconをギアに変更
        this.commonIcon.SetGearCommonIcon(true);
        var gearMaster = Masters.GearDB.FindById(data.gearId);

        // ランクスプライト切替
        var rank = CommonIconUtility.GetRarity((uint)ItemType.Gear, this.gearData.gearId);
        this.commonIcon.SetRank(rank);

        // ギアCommonIconセット、アイコンスプライト切替
        var bgSprite = CommonIconUtility.GetGearBgSprite(data.gearType);
        var mainSprite = CommonIconUtility.GetGearMainImageSprite(gearMaster.key);
        var subSprite = CommonIconUtility.GetGearSubImageSprite(gearMaster.subKey);
        this.commonIcon.SetGearSprite(bgSprite, mainSprite, subSprite);

        // 装着中ギア装着中パンネル表示
        this.equippedMark.SetActive(isEquipped);

        // クリック時処理登録
        this.commonIcon.onClick = () => onClick(this);

        // ロック情報セット
        SetTemplockImage(isLock);
    }

    /// <summary>
    /// チェックボックスアイテムセット
    /// </summary>
    public void SetCheckBox(UserGearData data, bool isEquipped, uint isLock, uint checkFlg, Action<ItemInventoryGearScrollViewItem> onClick)
    {
        this.gearData = data;
        // CommonIconをギアに変更
        this.commonIcon.SetGearCommonIcon(true);
        var gearMaster = Masters.GearDB.FindById(data.gearId);

        // ランクスプライト切替
        var rank = CommonIconUtility.GetRarity((uint)ItemType.Gear, this.gearData.gearId);
        this.commonIcon.SetRank(rank);

        // ギアCommonIconセット
        var bgSprite = CommonIconUtility.GetGearBgSprite(data.gearType);
        var mainSprite = CommonIconUtility.GetGearMainImageSprite(gearMaster.key);
        var subSprite = CommonIconUtility.GetGearSubImageSprite(gearMaster.subKey);
        this.commonIcon.SetGearSprite(bgSprite, mainSprite, subSprite);

        // 装着中ギア装着中パンネル表示
        this.equippedMark.SetActive(isEquipped);

        // クリック時処理登録
        this.commonIcon.onClick = () => onClick(this);

        // // ロック情報セット
        SetTemplockImage(isLock);

        // ロックの場合はクリック禁止・イメージ暗く
        if(isEquipped || isLock == 1)
        {
            this.commonIcon.button.interactable = false;

            // 色暗く
            var newColor = new Color(130/255f, 130/255f, 130/255f);
            this.commonIconGearBgGraphic.color = newColor;
            this.commonIconGearMainGraphic.color = newColor;
            this.commonIconGearSubGraphic.color = newColor;

            this.checkBox.SetActive(false);
        }
        // チェックボックスセット
        else
        {
            this.commonIcon.button.interactable = true;

            // 色の原本で
            var newColor = new Color(255/255f, 255/255f, 255/255f);
            this.commonIconGearBgGraphic.color = newColor;
            this.commonIconGearMainGraphic.color = newColor;
            this.commonIconGearSubGraphic.color = newColor;
            
            this.checkBox.SetActive(true);
            //クリック時処理登録
            this.commonIcon.onClick = () => onClick(this);
        }

        // 仮選択フラッグチェックセット
        SetTempCheckImage(checkFlg);
    }

    /// <summary>
    /// 仮ロックフラッグチェック更新
    /// </summary>
    public void SetTemplockImage(uint flg)
    {
        this.lockImage.gameObject.SetActive(flg == 1);
    }

    /// <summary>
    /// 仮選択フラッグチェック更新
    /// </summary>
    public void SetTempCheckImage(uint flg)
    {
        this.checkImage.gameObject.SetActive(flg == 1);
    }
}