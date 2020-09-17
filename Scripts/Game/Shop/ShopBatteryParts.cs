using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ショップのダイアログで表示する台座パーツ
/// </summary>
public class ShopBatteryParts : ShopCannonParts
{
    /// <summary>
    /// フィーバーアタックの名称テキスト
    /// </summary>
    [SerializeField]
    private Text fvNameText = null;
    /// <summary>
    /// フィーバーアタックの説明テキスト
    /// </summary>
    [SerializeField]
    private Text fvDescriptionText = null;

    /// <summary>
    /// 台座の設定
    /// </summary>
    public void SetInfo(uint itemType, uint itemId, float nowParam, uint fvId)
    {
        base.SetInfo(itemType, itemId, nowParam);

        //フィーバーアタックの設定
        Master.FvAttackData fvAttack = Masters.FvAttackDB.FindById(fvId);
        if (fvAttack != null)
        {
            fvNameText.text = fvAttack.name;
            fvDescriptionText.text = fvAttack.description;
        }
    }
}
