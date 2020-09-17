using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 購入確認ダイアログのコインコンテンツ
/// </summary>
public class PurchaseConfirmCoinContent : MonoBehaviour
{
    /// <summary>
    /// 購入前のコインテキスト
    /// </summary>
    [SerializeField]
    private Text beforeCoinText = null;
    /// <summary>
    /// 購入後のコインテキスト
    /// </summary>
    [SerializeField]
    private Text afterCoinText = null;

    public string BeforeCoinText { set => beforeCoinText.text = value; }
    public string AfterCoinText  { set => afterCoinText.text  = value; }
}
