using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ギア着脱確認ダイアログ内容
/// </summary>
public class CustomGearConfirmDialogContent : MonoBehaviour
{
    /// <summary>
    /// ギアパネル
    /// </summary>
    [Serializable]    
    public class GearPanel
    {
        /// <summary>
        /// アイコン
        /// </summary>
        [SerializeField]
        public CommonIcon icon = null;
        /// <summary>
        /// ギア名テキスト
        /// </summary>
        [SerializeField]
        public Text nameText = null;
        /// <summary>
        /// 未装着時テキスト
        /// </summary>
        [SerializeField]
        public Text notEquipedText = null;

        /// <summary>
        /// ギア情報のセット
        /// </summary>
        public void SetGearData(Master.GearData gearData)
        {
            if (gearData == null)
            {
                this.icon.gameObject.SetActive(false);
                this.nameText.gameObject.SetActive(false);
            }
            else
            {
                this.icon.SetGearCommonIcon(true);
;
                this.icon.SetRank((Rank)gearData.rarity);
                this.nameText.text = gearData.name;
                this.notEquipedText.gameObject.SetActive(false);

                // ギアスプライトセット
                var bgSprite = CommonIconUtility.GetGearBgSprite(gearData.partsType);
                var mainSprite = CommonIconUtility.GetGearMainImageSprite(gearData.key);
                var subSprite = CommonIconUtility.GetGearSubImageSprite(gearData.subKey);

                this.icon.SetGearSprite(bgSprite, mainSprite, subSprite);
            }
        }
    }

    /// <summary>
    /// 確認文言
    /// </summary>
    [SerializeField]
    private Text confirmText = null;
    /// <summary>
    /// Beforeギアパネル
    /// </summary>
    [SerializeField]
    private GearPanel beforeGearPanel = null;
    /// <summary>
    /// Afterギアパネル
    /// </summary>
    [SerializeField]
    private GearPanel afterGearPanel = null;
    /// <summary>
    /// コインエリア
    /// </summary>
    [SerializeField]
    private GameObject coinArea = null;
    /// <summary>
    /// Beforeコインテキスト
    /// </summary>
    [SerializeField]
    private Text beforeCoinText = null;
    /// <summary>
    /// Afterコインテキスト
    /// </summary>
    [SerializeField]
    private Text afterCoinText = null;
    /// <summary>
    /// 注意書きテキスト
    /// </summary>
    [SerializeField]
    private Text noteRemoveCostText = null;
    /// <summary>
    /// 無料、ギア外す表示テキスト
    /// </summary>
    [SerializeField]
    private Text freeGearRemoveText = null;

    /// <summary>
    /// 自身のダイアログ
    /// </summary>
    private SimpleDialog dialog = null;
    /// <summary>
    /// YesNoボタン
    /// </summary>
    private SimpleDialog.YesNoButtonGroup yesNo = null;
    /// <summary>
    /// 対象のパーツ
    /// </summary>
    private UserPartsData partsData = null;
    /// <summary>
    /// 変更前ギア情報
    /// </summary>
    private UserGearData beforeGear = null;
    /// <summary>
    /// 変更後ギア情報
    /// </summary>
    private UserGearData afterGear = null;
    /// <summary>
    /// リフレッシュ通知
    /// </summary>
    private Action onReflesh = null;
    /// <summary>
    /// キャンセル時コールバック
    /// </summary>
    private Action onCancel = null;
    // 無料、ギア外すカウンター
    public static uint freeGearRemoveCount = 0;

    /// <summary>
    /// セットアップ
    /// </summary>
    public void Setup(
        SimpleDialog dialog,
        UserPartsData partsData,
        UserGearData beforeGear,
        UserGearData afterGear,
        Action onReflesh,
        Action onCancel)
    {
        this.dialog = dialog;
        this.yesNo = this.dialog.AddYesNoButton();
        this.partsData = partsData;
        this.beforeGear = beforeGear;
        this.afterGear = afterGear;
        this.onReflesh = onReflesh;
        this.onCancel = onCancel;

        //キャンセル時処理登録
        this.yesNo.no.onClick = this.Cancel;

        Master.GearData beforeGearData = (this.beforeGear == null) ? null : Masters.GearDB.FindById(beforeGear.gearId);
        Master.GearData afterGearData = (this.afterGear == null) ? null : Masters.GearDB.FindById(afterGear.gearId);
        this.beforeGearPanel.SetGearData(beforeGearData);
        this.afterGearPanel.SetGearData(afterGearData);

        //所持コイン数テキスト
        long beforeCoin = (long)UserData.Get().coin;
        this.beforeCoinText.text = string.Format("{0:#,0}", beforeCoin);

        //装着
        if (beforeGearData == null)
        {
            this.coinArea.SetActive(false);
            this.confirmText.text = Masters.LocalizeTextDB.Get("ConfirmGearEquip");
            this.noteRemoveCostText.text = Masters.LocalizeTextDB.GetFormat("NoteGearRemoveCost", afterGearData.rejectCoin);
            this.freeGearRemoveText.text = null;
            this.yesNo.yes.onClick = this.CallGearSetApi;
        }
        else
        {
            this.noteRemoveCostText.gameObject.SetActive(false);
            
            // vipでない場合
            if(UserData.Get().vipLevel == 0)
            {
                this.freeGearRemoveText.gameObject.SetActive(false);
            }
            else
            {
               this.freeGearRemoveText.gameObject.SetActive(true);
            }

            if(CustomGearConfirmDialogContent.freeGearRemoveCount > 0)
            {
                // 無料カウントがある場合、beforeCoinはafterCoinTextと同じ
                this.afterCoinText.text = string.Format("{0:#,0}", beforeCoin);
            }
            else
            {
                this.afterCoinText.text = string.Format("{0:#,0}", beforeCoin - beforeGearData.rejectCoin);

                if (beforeCoin - beforeGearData.rejectCoin < 0)
                {
                    this.afterCoinText.color = UIUtility.decreaseColor;
                }
            }

            //外す
            if (afterGearData == null)
            {
                this.confirmText.text = Masters.LocalizeTextDB.Get("ConfirmGearRemove");
                this.yesNo.yes.onClick = this.CallGearUnsetApi;
                this.freeGearRemoveText.text = Masters.LocalizeTextDB.GetFormat("FreeGearRemove", CustomGearConfirmDialogContent.freeGearRemoveCount);
            }
            //変更
            else
            {
                this.confirmText.text = Masters.LocalizeTextDB.Get("ConfirmGearChange");
                this.yesNo.yes.onClick = this.CallGearChageApi;
                this.freeGearRemoveText.text = Masters.LocalizeTextDB.GetFormat("FreeGearRemove", CustomGearConfirmDialogContent.freeGearRemoveCount);
            }

            //コイン不足・無料回数が0
            if (beforeGearData.rejectCoin > beforeCoin && CustomGearConfirmDialogContent.freeGearRemoveCount <= 0)
            {
                //ボタン押せなくしてグレースケールに
                this.yesNo.yes.button.interactable = false;
                this.yesNo.yes.image.material = SharedUI.Instance.grayScaleMaterial;
                this.yesNo.yes.text.material = SharedUI.Instance.grayScaleMaterial;
                this.freeGearRemoveText.text = Masters.LocalizeTextDB.GetFormat("FreeGearRemove", CustomGearConfirmDialogContent.freeGearRemoveCount);
            }
        }
    }

    /// <summary>
    /// ギア装着通信
    /// </summary>
    private void CallGearSetApi()
    {
        SoundManager.Instance.PlaySe(SeName.YES);
        TurretApi.CallGearSetApi(this.afterGear.serverId, this.partsData.serverId, () =>
        {
            this.yesNo.yes.button.interactable = false;
            this.yesNo.no.button.interactable = false;
            this.onReflesh?.Invoke();
            this.dialog.Close();
        });
    }

    /// <summary>
    /// ギア取り外し通信
    /// </summary>
    private void CallGearUnsetApi()
    {
        SoundManager.Instance.PlaySe(SeName.YES);
        TurretApi.CallGearUnsetApi(this.beforeGear.serverId, () =>
        {
            this.yesNo.yes.button.interactable = false;
            this.yesNo.no.button.interactable = false;
            this.onReflesh?.Invoke();
            this.dialog.Close();
        });
    }

    /// <summary>
    /// ギア変更通信
    /// </summary>
    private void CallGearChageApi()
    {
        SoundManager.Instance.PlaySe(SeName.YES);
        TurretApi.CallGearUnsetApi(this.beforeGear.serverId, () =>
        {
            TurretApi.CallGearSetApi(this.afterGear.serverId, this.partsData.serverId, () =>
            {
                this.yesNo.yes.button.interactable = false;
                this.yesNo.no.button.interactable = false;
                this.onReflesh?.Invoke();
                this.dialog.Close();
            });
        });
    }

    /// <summary>
    /// キャンセル
    /// </summary>
    private void Cancel()
    {
        SoundManager.Instance.PlaySe(SeName.NO);
        this.yesNo.yes.button.interactable = false;
        this.yesNo.no.button.interactable = false;
        this.dialog.onClose = () => this.onCancel?.Invoke();
        this.dialog.Close();
    }
}
