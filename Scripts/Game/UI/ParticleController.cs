using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// パーティクル制御
/// </summary>
[RequireComponent(typeof(ParticleSystem))]
public class ParticleController : MonoBehaviour
{
    /// <summary>
    /// パーティクル
    /// </summary>
    [SerializeField]
    public ParticleSystem particle = null;
    /// <summary>
    /// 終了時コールバックのキー
    /// </summary>
    [SerializeField]
    private string key = null;
    /// <summary>
    /// 親
    /// </summary>
    [SerializeField]
    private ParticleController parent = null;

    /// <summary>
    /// Time
    /// </summary>
    private float time = 0f;
    /// <summary>
    /// 子の更新処理
    /// </summary>
    private Action<float> childrenUpdate = null;
    /// <summary>
    /// パーティクル終了時コールバック
    /// </summary>
    public Action<string> onParticleSystemStopped = null;

    /// <summary>
    /// Reset
    /// </summary>
    private void Reset()
    {
        this.particle = GetComponent<ParticleSystem>();
    }

    /// <summary>
    /// Awake
    /// </summary>
    private void Awake()
    {
        if (this.parent != null)
        {
            //親に自分の更新処理を依頼
            this.parent.childrenUpdate += this.Run;
        }
    }

    /// <summary>
    /// Run
    /// </summary>
    public void Run(float deltaTime)
    {
        this.particle.time = this.time;
        this.time += deltaTime;
        this.childrenUpdate?.Invoke(deltaTime);
    }

    /// <summary>
    /// OnParticleSystemStopped
    /// </summary>
    private void OnParticleSystemStopped()
    {
        if (this.parent != null)
        {
            this.parent.onParticleSystemStopped?.Invoke(this.key);
        }
        else
        {
            this.onParticleSystemStopped?.Invoke(this.key);
        }
    }
}
