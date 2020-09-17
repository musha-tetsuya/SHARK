using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 購入確認ダイアログのジェムコンテンツ
/// </summary>
public class PurchaseConfirmGemContent : MonoBehaviour
{
    /// <summary>
    /// 購入前のトータルジェムテキスト
    /// </summary>
    [SerializeField]
    private Text beforeTotalGemText = null;
    /// <summary>
    /// 購入後のトータルジェムテキスト
    /// </summary>
    [SerializeField]
    private Text afterTotalGemText = null;
    /// <summary>
    /// 購入前の有償ジェムテキスト
    /// </summary>
    [SerializeField]
    private Text beforeChargeGemText = null;
    /// <summary>
    /// 購入後の有償ジェムテキスト
    /// </summary>
    [SerializeField]
    private Text afterChargeGemText = null;

    public string BeforeTotalGemText { set => beforeTotalGemText.text = value; }
    public string AfterTotalGemText  { set => afterTotalGemText.text  = value; }

    public string BeforeChargeGemText { set => beforeChargeGemText.text = value; }
    public string AfterChargeGemText  { set => afterChargeGemText.text  = value; }
}
