using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemInventoryPlusPossessionDialogContent : MonoBehaviour
{
    [Header("テキスト")]
    /// <summary>
    /// 質問テキスト
    /// </summary>
    [SerializeField]
    private Text questionText = null;
    /// <summary>
    /// アイテムタイプ別名テキスト
    /// </summary>
    [SerializeField]
    private Text itemTypeNametext = null;
    /// <summary>
    /// 現在の所持制限テキスト
    /// </summary>
    [SerializeField]
    private Text beforePossessionCount = null;
    /// <summary>
    /// 拡張後の所持制限テキスト
    /// </summary>
    [SerializeField]
    private Text afterPossessionCount = null;
    /// <summary>
    /// ジェムデータ
    /// </summary>
    [SerializeField]
    private PurchaseConfirmGemContent gemContent = null;

    /// <summary>
    /// タイプ別マスター
    /// </summary>
    Master.GearExpansionData gearMaster;
    Master.PartsExpansionData partsMaster;

    /// <summary>
    /// データセット
    /// </summary>
    public void Set(uint partType, uint utilityId, SimpleDialog dialog, SimpleDialogButton expansionButton)
    {
        // 拡張時必要ジェム
        uint needGem = 0;
        // 拡張されるスロットの数
        uint expansionValue = 0;
        // ローカライジング·テキスト
        var localize = Masters.LocalizeTextDB;
        // ユーザーデータ
        UserData userData = UserData.Get();
        
        // タイプがギアの場合
        if(partType == (uint)ItemType.Gear)
        {
            // 現在ID、マスター
            this.gearMaster = Masters.GearExpansionDB.FindById(utilityId);
            // 次のID、マスター
            var nextGearMatser = Masters.GearExpansionDB.FindById(this.gearMaster.nextId);

            // 拡張時必要ジェム
            needGem = this.gearMaster.needGem;
            // 拡張されるスロットの数
            expansionValue = nextGearMatser.maxPossession - this.gearMaster.maxPossession;
            
            // 質問テキスト
            this.questionText.text = localize.GetFormat("PlusGearPossessionQuestion", needGem, expansionValue);
            // ギアタイプ名前
            this.itemTypeNametext.text = localize.Get("PlusGearPossession");
            
            // 現在の所持制限テキスト
            this.beforePossessionCount.text = this.gearMaster.maxPossession.ToString();
            // 拡張後の所持制限テキスト
            this.afterPossessionCount.text = nextGearMatser.maxPossession.ToString();

            // Beforeジェム
            this.gemContent.BeforeTotalGemText = userData.totalGem.ToString("#,0");
            this.gemContent.BeforeChargeGemText = userData.chargeGem.ToString("#,0");
            // Afterジェム
            this.PaymentGem(userData, needGem, 0);

            // ジェムがない場合戻る
            if (needGem > userData.totalGem)
            {
                //ボタンの設定
                expansionButton.text.text = Masters.LocalizeTextDB.Get("Back");
                expansionButton.onClick = dialog.Close;
            }
        }
        // タイプがパーツの場合
        else
        {
            // 現在ID、マスター
            this.partsMaster = Masters.PartsExpansionDB.FindById(utilityId);
            // 次のID、マスター
            var nextPartMaster = Masters.PartsExpansionDB.FindById(this.partsMaster.nextId);

            // 拡張時必要ジェム
            needGem = this.partsMaster.needGem;
            // 拡張されるスロットの数
            expansionValue = nextPartMaster.maxPossession - this.partsMaster.maxPossession;

            // パーツタイプ名前
            this.itemTypeNametext.text = localize.Get("PlusPartsPossession");
            // 質問テキスト
            this.questionText.text = localize.GetFormat("PlusPartsPossessionQuestion", needGem, expansionValue);

            // 現在の所持制限テキスト
            this.beforePossessionCount.text = this.partsMaster.maxPossession.ToString();
            // 拡張後の所持制限テキスト
            this.afterPossessionCount.text = nextPartMaster.maxPossession.ToString();
            
            // Beforeジェム
            this.gemContent.BeforeTotalGemText = userData.totalGem.ToString("#,0");
            this.gemContent.BeforeChargeGemText = userData.chargeGem.ToString("#,0");
            // Afterジェム
            this.PaymentGem(userData, needGem, 0);

            // ジェムがない場合戻る
            if (needGem > userData.totalGem)
            {
                //ボタンの設定
                expansionButton.text.text = Masters.LocalizeTextDB.Get("Back");
                expansionButton.onClick = dialog.Close;
            }
        }
    }

    /// <summary>
    /// ジェムによる支払時の処理
    /// </summary>
    private void PaymentGem(UserData userData, uint needFreeGem, uint needChargeGem)
    {
        //購入により減少するジェムの量を取得
        ulong subChargeGem = 0;
        ulong subFreeGem = 0;

        if (needFreeGem > 0)
        {
            if (userData.freeGem < needFreeGem)
            {
                subChargeGem = needFreeGem - userData.freeGem;
                subFreeGem = userData.freeGem;
            }
            else
            {
                subChargeGem = 0;
                subFreeGem = needFreeGem;
            }
        }
        else
        {
            subChargeGem = needChargeGem;
            subFreeGem = 0;
        }

        long afterChargeGem = (long)(userData.chargeGem - subChargeGem);
        gemContent.AfterChargeGemText = UIUtility.GetColorText(
            (afterChargeGem >= 0) ? TextColorType.None : TextColorType.DecreaseParam,
            afterChargeGem.ToString("#,0"));

        long afterTotalGem = (long)(userData.totalGem - (subChargeGem + subFreeGem));
        gemContent.AfterTotalGemText = UIUtility.GetColorText(
            (afterTotalGem >= 0) ? TextColorType.None : TextColorType.DecreaseParam,
            afterTotalGem.ToString("#,0"));
    }
}
