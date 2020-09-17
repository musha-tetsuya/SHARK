using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CannonWorkspaceExpansionDialogContent : MonoBehaviour
{
    /// <summary>
    /// 消費確認テキスト
    /// </summary>
    [SerializeField]
    private Text confirmText = null;
    /// <summary>
    /// ジェムデータ
    /// </summary>
    [SerializeField]
    private PurchaseConfirmGemContent gemContent = null;
    /// <summary>
    /// 現在の砲台枠
    /// </summary>
    [SerializeField]
    private Text beforeCannonCount = null;
    /// <summary>
    /// 購入後、砲台枠
    /// </summary>
    [SerializeField]
    private Text afterCannonCount = null;

    /// <summary>
    /// 砲台枠拡張、ダイアログセット
    /// </summary>
    public void Set(SimpleDialog dialog, SimpleDialogButton expansionButton, uint needGem, uint maxPossession, uint nextMaxPossession)
    {
        // 消費確認テキスト
        this.confirmText.text = Masters.LocalizeTextDB.GetFormat("PlusCannonPossessionQuestion", needGem, nextMaxPossession);
        // 現在の砲台枠
        this.beforeCannonCount.text = maxPossession.ToString();
        // 購入後、砲台枠
        this.afterCannonCount.text = nextMaxPossession.ToString();

        UserData userData = UserData.Get();
        // Beforeジェム
        gemContent.BeforeTotalGemText = userData.totalGem.ToString("#,0");
        gemContent.BeforeChargeGemText = userData.chargeGem.ToString("#,0");
        // Afterジェム
        // gemContent.AfterTotalGemText = (userData.totalGem - needGem).ToString("#,0");
        // gemContent.AfterChargeGemText = userData.chargeGem.ToString("#,0");

        PaymentGem(userData, needGem, 0);

        if(needGem > userData.totalGem)
        {
            //ボタンの設定
            expansionButton.text.text = Masters.LocalizeTextDB.Get("Back");
            expansionButton.onClick = dialog.Close;
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
