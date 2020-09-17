using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// BETボタンUI
/// </summary>
public class UIBetButton : MonoBehaviour
{
    /// <summary>
    /// プラスボタン
    /// </summary>
    [SerializeField]
    private Button plusButton = null;
    /// <summary>
    /// マイナスボタン
    /// </summary>
    [SerializeField]
    private Button minusButton = null;
    /// <summary>
    /// BET数テキスト
    /// </summary>
    [SerializeField]
    private Text betNumText = null;
    /// <summary>
    /// セーフエリア調節するかどうか
    /// </summary>
    [SerializeField]
    private bool isSafeAreaAdjust = false;

    /// <summary>
    /// Start
    /// </summary>
    private void Start()
    {
        if (this.isSafeAreaAdjust)
        {
#if !UNITY_EDITOR && UNITY_IOS
            var rectTransform = this.transform as RectTransform;
            var canvas = Battle.BattleGlobal.instance.uiCanvas;

            //スクリーン座標系でのセーフエリア幅をキャンバス座標系での幅に変換
            Vector2 screenDistance = new Vector2(0f, Screen.safeArea.min.y) - new Vector2(0f, Screen.height * -0.5f);
            Vector2 canvasDistance;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, screenDistance, canvas.worldCamera, out canvasDistance);

            //キャンバス座標系でのセーフエリア幅分上にずらす
            var pos = rectTransform.anchoredPosition;
            pos.y += canvasDistance.y;
            rectTransform.anchoredPosition = pos;
#endif
        }
    }

    /// <summary>
    /// ボタン表示切り替え
    /// </summary>
    public void SetButtonVisible(bool visible)
    {
        this.plusButton.gameObject.SetActive(visible);
        this.minusButton.gameObject.SetActive(visible);
    }

    /// <summary>
    /// BET数設定
    /// </summary>
    public void SetBetNum(int bet)
    {
        this.betNumText.text = bet.ToString();
    }
}
