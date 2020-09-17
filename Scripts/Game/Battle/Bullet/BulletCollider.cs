using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 弾丸コライダ機能
/// </summary>
public class BulletCollider : MonoBehaviour
{
    /// <summary>
    /// レシーバーインターフェース
    /// </summary>
    public interface IReceiver
    {
        void OnHit(Collider2D collider2D);
    }

    /// <summary>
    /// レシーバー
    /// </summary>
    public IReceiver receiver = null;
    /// <summary>
    /// Stayイベントレシーバー
    /// </summary>
    public IReceiver stayEventReceiver = null;

    /// <summary>
    /// 当たり判定
    /// </summary>
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (this.enabled)
        {
            this.receiver?.OnHit(col);
        }
    }

    /// <summary>
    /// 当たり判定
    /// </summary>
    private void OnTriggerStay2D(Collider2D col)
    {
        if (this.enabled)
        {
            this.stayEventReceiver?.OnHit(col);
        }
    }
}

