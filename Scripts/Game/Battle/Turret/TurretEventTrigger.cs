using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Battle {

/// <summary>
/// 砲台イベントトリガ
/// </summary>
public class TurretEventTrigger : EventTrigger
{
    /// <summary>
    /// タッチしているかどうか
    /// </summary>
    public bool isTouch { get; private set; }

    /// <summary>
    /// クリック時コールバック
    /// </summary>
    public event Action<PointerEventData> onClick = null;

    /// <summary>
    /// 画面押下時
    /// </summary>
    public override void OnPointerDown(PointerEventData eventData)
    {
        this.isTouch = true;
    }

    /// <summary>
    /// 画面から指を離した時
    /// </summary>
    public override void OnPointerUp(PointerEventData eventData)
    {
        this.isTouch = false;
    }

    /// <summary>
    /// クリック時
    /// </summary>
    public override void OnPointerClick(PointerEventData eventData)
    {
        this.onClick?.Invoke(eventData);
    }

}//class TurretEventTrigger

}//namespace Battle