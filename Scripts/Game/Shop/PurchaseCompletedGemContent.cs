using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 購入完了ダイアログのジェムコンテンツ
/// </summary>
public class PurchaseCompletedGemContent : MonoBehaviour
{
    /// <summary>
    /// 購入後のトータルジェムテキスト
    /// </summary>
    [SerializeField]
    private Text afterTotalGemText = null;
    /// <summary>
    /// 購入後の有償ジェムテキスト
    /// </summary>
    [SerializeField]
    private Text afterChargeGemText = null;

    public string AfterTotalGemText  { set => afterTotalGemText.text  = value; }
    public string AfterChargeGemText { set => afterChargeGemText.text = value; }
}
