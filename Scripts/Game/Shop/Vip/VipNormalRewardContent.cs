using System;
using UnityEngine;
using UnityEngine.UI;

public class VipNormalRewardContent : MonoBehaviour
{
    /// <summary>
    /// 受け取りボタンのテキスト
    /// </summary>
    [SerializeField]
    private Text titleText = null;

    /// <summary>
    /// 情報の設定
    /// </summary>
    public void SetInfo(string benefitTypeName)
    {
        titleText.text = benefitTypeName;
    }
}
