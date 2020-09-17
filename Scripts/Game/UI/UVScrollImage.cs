using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// RawImageのUVスクロール
/// </summary>
/// <para>
/// SpriteのWrapModeをRepeatにする必要がある
/// </para>
[RequireComponent(typeof(RawImage))]
public class UVScrollImage : MonoBehaviour
{
    /// <summary>
    /// イメージ
    /// </summary>
    [SerializeField]
    private RawImage rawImage = null;
    /// <summary>
    /// X方向スクロール速度
    /// </summary>
    [SerializeField]
    private float scrollSpeedX = 0f;
    /// <summary>
    /// Y方向スクロール速度
    /// </summary>
    [SerializeField]
    private float scrollSpeedY = 0f;

    /// <summary>
    /// Reset
    /// </summary>
    private void Reset()
    {
        this.rawImage = GetComponent<RawImage>();
    }

    /// <summary>
    /// Update
    /// </summary>
    private void Update()
    {
        var uvRect = this.rawImage.uvRect;
        uvRect.x += this.scrollSpeedX;
        uvRect.y += this.scrollSpeedY;
        this.rawImage.uvRect = uvRect;
    }
}
