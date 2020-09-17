using UnityEngine;
using UnityEngine.UI;

public class VipExpGauge : MonoBehaviour
{
    /// <summary>
    /// Vipランク表示のテキスト
    /// </summary>
    [SerializeField]
    private Text rankText = null;
    /// <summary>
    /// 経験値表示のテキスト
    /// </summary>
    [SerializeField]
    private Text expText = null;
    /// <summary>
    /// 経験値ゲージの画像
    /// </summary>
    [SerializeField]
    private Image gaugeImage = null;


    /// <summary>
    /// ランクの設定
    /// </summary>
    public void SetRank(uint rank)
    {
        rankText.text = rank.ToString();
    }

    /// <summary>
    /// 経験値の設定
    /// </summary>
    public void SetExp(uint nowExp, uint maxExp)
    {
        gaugeImage.fillAmount = nowExp / (float)maxExp;
        expText.text = Masters.LocalizeTextDB.GetFormat("NextVipIs", nowExp, maxExp);
    }
}
