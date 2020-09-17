using UnityEngine;
using System;
using UnityEngine.UI;
using System.Linq;

public class ItemInventoryPartsScrollViewItem : MonoBehaviour
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
    /// イメージ暗く
    /// </summary>
    [SerializeField]
    private Image darkBoxImage = null;
    /// <summary>
    /// 初期砲台削除できない、テキスト
    /// </summary>
    [SerializeField]
    private Text defaultCanonText = null;
    
    /// <summary>
    /// ユーザーパーツデータ
    /// </summary>
    public UserPartsData partsData { get; private set; }

    /// <summary>
    /// 表示構築
    /// </summary>
    public void Set(UserPartsData data, bool isEquipped, uint isLock, Action<ItemInventoryPartsScrollViewItem> onClick)
    {
        this.partsData = data;
        this.commonIcon.countText.text = null;

        if(data.itemType == (uint)ItemType.Accessory)
        {
            this.commonIcon.gearArea.SetActive(false);
        }
        
        // 装着中パンネル表示
        this.equippedMark.SetActive(isEquipped);
        
        //アイコンスプライト切替
        string key = CommonIconUtility.GetSpriteKey(this.partsData.itemType, this.partsData.itemId);
        string path = CommonIconUtility.GetSpritePath(this.partsData.itemType, key);
        var handle = AssetManager.FindHandle<Sprite>(path);
        this.commonIcon.SetIconSprite(handle.asset as Sprite);

        //ランクスプライト切替
        var rank = CommonIconUtility.GetRarity(this.partsData.itemType, this.partsData.itemId);
        this.commonIcon.SetRank(rank);

        this.commonIcon.SetGearSlot(data);

        //クリック時処理登録
        this.commonIcon.onClick = () => onClick(this);
        
        // ロック情報
        bool isLockCheck = false;

        if(isLock == 1)
        {
            isLockCheck = true;
        }
        else
        {
            isLockCheck = false;
        }

        this.lockImage.gameObject.SetActive(isLockCheck);
    }

    /// <summary>
    /// チェックボックスアイテムセット
    /// </summary>
    public void SetCheckBox(UserPartsData data, bool isEquipped, uint isLock, uint checkFlg, Action<ItemInventoryPartsScrollViewItem> onClick)
    {
        this.partsData = data;
        this.commonIcon.countText.text = null;
        var itemSellId = GetItemSellId();

        if(data.itemType == (uint)ItemType.Accessory)
        {
            this.commonIcon.gearArea.SetActive(false);
        }
        
        // 装着中パンネル表示
        this.equippedMark.SetActive(isEquipped);
        
        //アイコンスプライト切替
        string key = CommonIconUtility.GetSpriteKey(this.partsData.itemType, this.partsData.itemId);
        string path = CommonIconUtility.GetSpritePath(this.partsData.itemType, key);
        var handle = AssetManager.FindHandle<Sprite>(path);
        this.commonIcon.SetIconSprite(handle.asset as Sprite);

        //ランクスプライト切替
        var rank = CommonIconUtility.GetRarity(this.partsData.itemType, this.partsData.itemId);
        this.commonIcon.SetRank(rank);

        this.commonIcon.SetGearSlot(data);

        // ロック情報
        this.SetTemplockImage(isLock);

        /// 装着ギアがロックの場合、カウント
        uint[] gearLockCount = UserData.Get().gearData
        .Where(x => x.partsServerId == data.serverId)
        .Select(x => x.lockFlg)
        .ToArray();

        uint count = 0;

        for(int i = 0; i < gearLockCount.Length; i++)
        {
            count += gearLockCount[i];
        }

        // ロックの場合はクリック禁止・イメージ暗く
        if(isLock == 1 || isEquipped || count > 0 || itemSellId == 0)
        {
            this.darkBoxImage.gameObject.SetActive(true);
            this.commonIcon.button.interactable = false;
            this.checkBox.SetActive(false);
        }
        // チェックボックスセット
        else
        {
            this.darkBoxImage.gameObject.SetActive(false);
            this.commonIcon.button.interactable = true;

            this.checkBox.SetActive(true);
            //クリック時処理登録
            this.commonIcon.onClick = () => onClick(this);
        }

        // 初期砲台の場合、分解不可能の案内メッセージ、テキストセット
        if (itemSellId == 0)
        {
            this.defaultCanonText.text = Masters.LocalizeTextDB.Get("CannotDisassembledDefaultCanon");
        }
        else
        {
            this.defaultCanonText.text = null;
        }

        SetTempCheckImage(checkFlg);
    }

    /// <summary>
    /// パーツ別、マスターから、ItemSellId修得
    /// </summary>
    private uint GetItemSellId()
    {
        uint partsType = this.partsData.itemType;
        uint partsId = this.partsData.itemId;
        uint itemSellId = 0;

        if (partsType == (uint)ItemType.Battery)
        {
            itemSellId = Masters.BatteryDB.FindById(partsId).itemSellId;
        }
        else if (partsType == (uint)ItemType.Barrel)
        {
            itemSellId = Masters.BarrelDB.FindById(partsId).itemSellId;
        }
        else if (partsType == (uint)ItemType.Bullet)
        {
            itemSellId = Masters.BulletDB.FindById(partsId).itemSellId;
        }
        else if (partsType == (uint)ItemType.Accessory)
        {
            itemSellId = Masters.AccessoriesDB.FindById(partsId).itemSellId;
        }

        return itemSellId;
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
