using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// コイン情報パネルUI
/// </summary>
public class UICoinInfoPanel : MonoBehaviour
{
    /// <summary>
    /// コイン数テキスト
    /// </summary>
    [SerializeField]
    private Text coinNumText = null;
    /// <summary>
    /// コインフレーム
    /// </summary>
    [SerializeField]
    private RectTransform coinFrameRect = null;
    /// <summary>
    /// コインフレームカスタム形状
    /// </summary>
    [SerializeField]
    private Rect customCoinFrameRect = Rect.zero;
    /// <summary>
    /// 顔アイコン
    /// </summary>
    [SerializeField]
    private GameObject faceIcon = null;

    /// <summary>
    /// コインフレームデフォルト形状
    /// </summary>
    private Rect defaultCoinFramRect = Rect.zero;

    /// <summary>
    /// Awake
    /// </summary>
    private void Awake()
    {
        this.defaultCoinFramRect.position = this.coinFrameRect.anchoredPosition;
        this.defaultCoinFramRect.size = this.coinFrameRect.sizeDelta;
    }

    /// <summary>
    /// 形状設定
    /// </summary>
    public void SetForm(bool isLocalPlayer)
    {
        this.faceIcon.SetActive(!isLocalPlayer);

        if (isLocalPlayer)
        {
            this.coinFrameRect.anchoredPosition = customCoinFrameRect.position;
            this.coinFrameRect.sizeDelta = customCoinFrameRect.size;
        }
        else
        {
            this.coinFrameRect.anchoredPosition = defaultCoinFramRect.position;
            this.coinFrameRect.sizeDelta = defaultCoinFramRect.size;
        }
    }

    /// <summary>
    /// コイン数設定
    /// </summary>
    public void SetCoinNum(long num)
    {
        int i = 0;

        while (num >= 1000000)
        {
            num /= 10000;
            i++;
        }

        this.coinNumText.text = (i < 4)
            ? Masters.LocalizeTextDB.GetFormat("CoinCount" + (i + 1), num)
            : Masters.LocalizeTextDB.GetFormat("CoinCount4", 999999); 
    }
}
