using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 弾丸移動機能
/// </summary>
public class BulletMovement : MonoBehaviour
{
    /// <summary>
    /// RectTransform
    /// </summary>
    public RectTransform rectTransform { get; private set; }
    /// <summary>
    /// 速度
    /// </summary>
    [NonSerialized]
    public uint speed = 1000;
    /// <summary>
    /// サーバータイムスタンプ
    /// </summary>
    [NonSerialized]
    public int timeStamp = 0;

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
        float deltaTime = Battle.BattleGlobal.GetDeltaTime(ref this.timeStamp);

        //向いてる方向に移動
        this.rectTransform.anchoredPosition += (Vector2)this.rectTransform.up * this.speed * deltaTime;
    }
}
