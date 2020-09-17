using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Battle {

/// <summary>
/// 跳弾機能
/// </summary>
public class BulletBouncing : MonoBehaviour
{
    /// <summary>
    /// RectTransform
    /// </summary>
    private RectTransform rectTransform = null;
    /// <summary>
    /// エリア範囲
    /// </summary>
    private Rect areaRect => BattleGlobal.instance.bulletArea.rect;

    /// <summary>
    /// Awake
    /// </summary>
    private void Awake()
    {
        this.rectTransform = this.transform as RectTransform;
    }

    /// <summary>
    /// Update
    /// </summary>
    private void Update()
    {
        //壁に当たったら跳弾
        this.BouncingIfHitTheWall();
    }
    
    /// <summary>
    /// 壁に当たったら跳弾
    /// </summary>
    protected void BouncingIfHitTheWall()
    {
        var pos = this.rectTransform.anchoredPosition;
        var direction = this.rectTransform.up;

        while (true)
        {
            //左の壁に当たった
            if (pos.x < this.areaRect.xMin)
            {
                pos.x = this.areaRect.xMin + (this.areaRect.xMin - pos.x);
                direction.x *= -1;
            }

            //右の壁に当たった
            if (pos.x > this.areaRect.xMax)
            {
                pos.x = this.areaRect.xMax - (pos.x - this.areaRect.xMax);
                direction.x *= -1;
                continue;
            }

            break;
        }

        while (true)
        {
            //下の壁に当たった
            if (pos.y < this.areaRect.yMin)
            {
                pos.y = this.areaRect.yMin + (this.areaRect.yMin - pos.y);
                direction.y *= -1;
            }

            //上の壁に当たった
            if (pos.y > this.areaRect.yMax)
            {
                pos.y = this.areaRect.yMax - (pos.y - this.areaRect.yMax);
                direction.y *= -1;
                continue;
            }

            break;
        }

        this.rectTransform.anchoredPosition = pos;
        this.rectTransform.up = direction;
    }
}

}