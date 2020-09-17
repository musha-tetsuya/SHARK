using UnityEngine;

/// <summary>
/// ショップのダイアログで表示する砲台パーツ
/// </summary>
public class ShopCannonParts : MonoBehaviour
{
    /// <summary>
    /// パーツのアイコン
    /// </summary>
    [SerializeField]
    private CommonIcon icon = null;
    /// <summary>
    /// 能力値のゲージ
    /// </summary>
    [SerializeField]
    private CommonStatusGauge statusGauge = null;

    /// <summary>
    /// 砲台パーツの設定
    /// </summary>
    public void SetInfo(uint itemType, uint itemId, float nowParam)
    {
        //CommonIcon表示構築
        this.icon.Set(itemType, itemId, false);

        //アイコンの個数表示の設定
        this.icon.countText.text = null;

        //TODO.RYU ギアゲージセット
        statusGauge.SetGaugeValue(Mathf.Clamp01(nowParam));
    }
}
