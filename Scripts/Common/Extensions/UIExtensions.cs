using System;
using UnityEngine;
using UnityEngine.UI;

public static class UIExtensions
{
    /// <summary>
    /// 全体を満たすようにセットする
    /// </summary>
    public static void SetAsFill(this RectTransform rectTransform)
    {
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.pivot = Vector2.one * 0.5f;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }

    /// <summary>
    /// パーティクルのレイヤー設定
    /// </summary>
    public static void SetParticleLayer(this ParticleSystemRenderer[] renderers, Canvas canvas)
    {
        foreach (var renderer in renderers)
        {
            renderer.gameObject.layer = canvas.gameObject.layer;
            renderer.sortingOrder = canvas.sortingOrder + 1;
        }
    }
}
