using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 弾丸基礎
/// </summary>
public class BulletBase : TimePauseBehaviour
{
    /// <summary>
    /// 弾丸コライダ機能
    /// </summary>
    [SerializeField]
    public BulletCollider bulletCollider = null;
    /// <summary>
    /// 着弾エフェクトプレハブ
    /// </summary>
    [SerializeField]
    public LandingEffect landingEffectPrefab = null;
    /// <summary>
    /// 移動機能：移動しない弾丸の場合null
    /// </summary>
    [SerializeField]
    public BulletMovement movement = null;
    /// <summary>
    /// アニメーター：アニメーションが無ければnull
    /// </summary>
    [SerializeField]
    public Animator animator = null;

    /// <summary>
    /// パーティクルリスト
    /// </summary>
    private ParticleSystem[] m_particles = null;
    private ParticleSystem[] particles => this.m_particles ?? (this.m_particles = GetComponentsInChildren<ParticleSystem>());
    private ParticleSystemRenderer[] m_particleRenderers = null;
    private ParticleSystemRenderer[] particleRenderers => this.m_particleRenderers ?? (this.m_particleRenderers = GetComponentsInChildren<ParticleSystemRenderer>());

    /// <summary>
    /// 停止
    /// </summary>
    public override void Pause(BinaryWriter writer)
    {
        base.Pause(writer);

        if (this.bulletCollider != null)
        {
            writer.Write(this.bulletCollider.enabled);
            this.bulletCollider.enabled = false;
        }
        if (this.movement != null)
        {
            writer.Write(this.movement.enabled);
            this.movement.enabled = false;
        }
        if (this.animator != null)
        {
            writer.Write(this.animator.enabled);
            this.animator.enabled = false;
        }
        for (int i = 0; i < this.particles.Length; i++)
        {
            this.particles[i].Pause();
        }
    }

    /// <summary>
    /// 再開
    /// </summary>
    public override void Play(BinaryReader reader)
    {
        base.Play(reader);

        if (this.bulletCollider != null)
        {
            this.bulletCollider.enabled = reader.ReadBoolean();
        }
        if (this.movement != null)
        {
            this.movement.enabled = reader.ReadBoolean();
        }
        if (this.animator != null)
        {
            this.animator.enabled = reader.ReadBoolean();
        }
        for (int i = 0; i < this.particles.Length; i++)
        {
            this.particles[i].Play();
        }
    }

    /// <summary>
    /// パーティクルのレイヤーを設定する
    /// </summary>
    public void SetParticleLayer(Canvas bulletCanvas)
    {
        foreach (var renderer in this.particleRenderers)
        {
            renderer.gameObject.layer = bulletCanvas.gameObject.layer;
            renderer.sortingOrder = bulletCanvas.sortingOrder + 1;
        }
    }

    /// <summary>
    /// Start
    /// </summary>
    private void Start()
    {
        for (int i = 0; i < this.particles.Length; i++)
        {
            this.particles[i].Play();
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(BulletBase))]
    private class MyInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            this.serializedObject.Update();

            base.OnInspectorGUI();

            if (GUILayout.Button("Auto Set ParticleLayer"))
            {
                var t = this.target as BulletBase;
                var canvas = t.GetComponentInParent<Canvas>();
                if (canvas != null)
                {
                    t.SetParticleLayer(canvas);
                }
            }

            this.serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}
