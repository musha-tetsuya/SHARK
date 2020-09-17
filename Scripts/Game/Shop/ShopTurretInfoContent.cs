using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ショップのダイアログで表示する砲台情報
/// </summary>
public class ShopTurretInfoContent : MonoBehaviour
{
    /// <summary>
    /// アイコン画像
    /// </summary>
    [SerializeField]
    private Image iconImage = null;
    /// <summary>
    /// 名前テキスト
    /// </summary>
    [SerializeField]
    private Text nameText = null;
    /// <summary>
    /// 説明テキスト
    /// </summary>
    [SerializeField]
    private Text descriptionText = null;

    /// <summary>
    /// シリーズスキル情報の設定
    /// </summary>
    public void SetSerieseSkillInfo(uint seriesId)
    {
        var serieseData = Masters.TurretSerieseDB.FindById(seriesId);
        var serieseSkillData = Masters.SerieseSkillDB.FindById(serieseData.seriesSkillId);

        //砲台ページのセットスキル名、説明文、アイコン画像設定
        this.iconImage.sprite = AssetManager.FindHandle<Sprite>(SharkDefine.GetSeriesSkillIconSpritePath(serieseSkillData.key)).asset as Sprite;
        this.nameText.text = serieseSkillData.name;
        this.descriptionText.text = serieseSkillData.description;
    }

    /// <summary>
    /// FVA情報の設定
    /// </summary>
    public void SetFVAInfo(uint fvaId)
    {
        var fvaData = Masters.FvAttackDB.FindById(fvaId);
        this.iconImage.sprite = AssetManager.FindHandle<Sprite>(SharkDefine.GetFvAttackTypeIconSpritePath((FvAttackType)fvaData.type)).asset as Sprite;
        this.nameText.text = fvaData.name;
        this.descriptionText.text = fvaData.description;
    }
}
