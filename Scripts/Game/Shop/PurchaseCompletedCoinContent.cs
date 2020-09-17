using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 購入完了ダイアログのコインコンテンツ
/// </summary>
public class PurchaseCompletedCoinContent : MonoBehaviour
{
    /// <summary>
    /// 購入後のコインテキスト
    /// </summary>
    [SerializeField]
    private Text afterCoinText = null;

    public string AfterCoinText { set => afterCoinText.text = value; }
}
