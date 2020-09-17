using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Battle {

/// <summary>
/// 魚ゲージUI
/// </summary>
public class UIFishGauge : MonoBehaviour
{
    /// <summary>
    /// ゲージイメージ
    /// </summary>
    [SerializeField]
    private Slider gauge = null;
    /// <summary>
    /// 数値テキスト
    /// </summary>
    [SerializeField]
    private Text numText = null;

    /// <summary>
    /// 最大値
    /// </summary>
    private int max = 0;
    /// <summary>
    /// 現在値
    /// </summary>
    [NonSerialized]
    public int now = 0;

    /// <summary>
    /// セットアップ
    /// </summary>
    public void Setup(FishWaveData data)
    {
        this.max = (int)data.formationDatas
            .Select(x => x.data)
            .SelectMany(formationData => formationData.routeDatas.Select(x => x.data))
            .Sum(x => x.amount);
        this.now = this.max;
        this.Reflesh();
    }

    /// <summary>
    /// 表示更新
    /// </summary>
    public void Reflesh()
    {
        this.gauge.value = (float)this.now / this.max;
        this.numText.text = this.now.ToString();
    }
}

}