using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Battle {

/// <summary>
/// HPゲージUI
/// </summary>
public class UIHpGauge : MonoBehaviour
{
    /// <summary>
    /// ゲージ
    /// </summary>
    [SerializeField]
    private Image gaugeImage = null;
    /// <summary>
    /// 残HPテキスト
    /// </summary>
    [SerializeField]
    private Text remainText = null;

    /// <summary>
    /// ゲージ値設定
    /// </summary>
    public void Set(int nowHp, int maxHp)
    {
        this.gaugeImage.fillAmount = (float)nowHp / maxHp;
        this.remainText.text = nowHp.ToString();
    }
}

}