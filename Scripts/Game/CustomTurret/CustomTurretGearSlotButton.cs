using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CustomTurretGearSlotButton : MonoBehaviour
{
    /// <summary>
    /// ギアアイコン
    /// </summary>
    [SerializeField]
    private CommonIcon gearIcon = null;
    /// <summary>
    /// ギア名テキスト
    /// </summary>
    [SerializeField]
    private Text gearNameText = null;
    /// <summary>
    /// 説明文テキスト
    /// </summary>
    [SerializeField]
    private Text info = null;
    /// <summary>
    /// ギアが装着している場合のボタン
    /// </summary>
    [SerializeField]
    public GameObject AfterGearEquipSlot = null;
    /// <summary>
    /// ギアを装着していない場合のボタン
    /// </summary>
    [SerializeField]
    public GameObject BeforeGearEquipSlot = null;

    /// <summary>
    /// ギアが装着されているパーツ
    /// </summary>
    public UserPartsData partsData { get; private set; }
    /// <summary>
    /// 装着中ギアのマスタID
    /// </summary>
    public uint gearId { get; private set; }
    /// <summary>
    /// 拡張済みスロットかどうか
    /// </summary>
    public bool isExtended { get; private set; }
    /// <summary>
    /// 名前、説明文の時間切替用
    /// </summary>
    private float timeCount = 0f;
    /// <summary>
    /// クリック時コールバック
    /// </summary>
    private Action<CustomTurretGearSlotButton> onClick = null;


    /// <summary>
    /// ギアスロットボタンセット
    /// </summary>
    public void SetUp(
        UserPartsData data,
        uint gearId,
        bool isExtended,
        Action<CustomTurretGearSlotButton> onClick)
    {
        this.partsData = data;
        this.gearId = gearId;
        this.isExtended = isExtended;
        this.onClick = onClick;

        //ギア装着・スロット解除の情報チェック
        this.BeforeGearEquipSlot.SetActive(isExtended);
        this.AfterGearEquipSlot.SetActive(isExtended && gearId > 0);

        //名前、説明文表示切替のリセット
        this.timeCount = 0f;
        this.gearNameText.gameObject.SetActive(true);
        this.info.gameObject.SetActive(false);

        //未装着
        if (gearId == 0)
        {
            this.gearNameText.text = null;
            this.info.text = null;
        }
        //装備中
        else
        {
            var master = Masters.GearDB.FindById(gearId);
            this.gearIcon.SetRank((Rank)master.rarity);
            
            // CommonIconをギアに変更
            this.gearIcon.SetGearCommonIcon(true);
            
            // TODO. ギアデータセット
            var partsType = master.partsType;
            // TODO. ギアCommonIconセット
            var bgSprite = CommonIconUtility.GetGearBgSprite(partsType);
            var mainSprite = CommonIconUtility.GetGearMainImageSprite(master.key);
            var subSprite = CommonIconUtility.GetGearSubImageSprite(master.subKey);
            this.gearIcon.SetGearSprite(bgSprite, mainSprite, subSprite);

            this.gearNameText.text = master.name;
            this.info.text = master.description;
        }
    }

    /// <summary>
    /// Update
    /// </summary>
    private void Update()
    {
        if (this.gearId > 0)
        {
            //3秒毎に名前と説明文の表示を切り替える
            if (this.timeCount >= 3f)
            {
                this.gearNameText.gameObject.SetActive(!this.gearNameText.gameObject.activeSelf);
                this.info.gameObject.SetActive(!this.info.gameObject.activeSelf);
                this.timeCount = 0f;
            }

            this.timeCount += Time.deltaTime;
        }
    }

    /// <summary>
    /// クリック時
    /// </summary>
    public void OnClick()
    {
        this.onClick?.Invoke(this);
        SoundManager.Instance.PlaySe(SeName.YES);
    }
}
