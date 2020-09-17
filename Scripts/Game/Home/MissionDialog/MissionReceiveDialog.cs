using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ミッション報酬受け取りダイアログ
/// </summary>
public class MissionReceiveDialog : MonoBehaviour
{
    /// <summary>
    /// アイテムの名称Text
    /// </summary>
    [SerializeField]
    private Text itemNameText = null;
    /// <summary>
    /// アイテムの説明Text
    /// </summary>
    [SerializeField]
    private Text itemDescriptionText = null;
    /// <summary>
    /// アイコンイメージ
    /// </summary>
    [SerializeField]
    private Image iconImage = null;

    /// <summary>
    /// 表示構築
    /// </summary>
    public void BuildView(MissionContent content)
    {
        var rewardData = Masters.MissionRewardDB.FindById(content.server.missionRewardId);
        var rewardItemInfo = CommonIconUtility.GetItemInfo(rewardData.itemType, rewardData.itemId);

        //アイコンイメージ設定
        this.iconImage.sprite = content.iconImage.sprite;

        //報酬名
        this.itemNameText.text = string.Format("{0}×{1:#,0}", rewardItemInfo.GetName(), rewardData.itemNum);

        //報酬説明文
        this.itemDescriptionText.text = rewardItemInfo.GetDescription();
    }
}
