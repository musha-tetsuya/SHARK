using UnityEngine;
using UnityEngine.UI;

public class InventoryDecompositionNameScrollViewItem : MonoBehaviour
{
    /// <summary>
    /// パーツやギア名テキスト
    /// </summary>
    [SerializeField]
    private Text partsName = null;

    /// <summary>
    /// データロード
    /// </summary>
    public void Set(string partsName)
    {
        // パーツやギア名セット
        this.partsName.text = partsName;
    }
}
