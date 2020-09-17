using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 共通ステータスゲージUI
/// </summary>
public class CommonStatusGauge : MonoBehaviour
{
    /// <summary>
    /// ゲージイメージ
    /// </summary>
    [SerializeField]
    private Image gaugeImage = null;

    /// <summary>
    /// ゲージ値設定
    /// </summary>
    public void SetGaugeValue(float value)
    {
        this.gaugeImage.fillAmount = value;
    }
}
