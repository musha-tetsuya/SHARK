using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SEトラック
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class SeTrack : MonoBehaviour
{
    /// <summary>
    /// ループデータ
    /// </summary>
    public class LoopData
    {
        /// <summary>
        /// ループ開始位置正規化時間
        /// </summary>
        public float startNormalizedTime = 0f;
        /// <summary>
        /// ループ終了位置正規化時間
        /// </summary>
        public float endNormalizedTime = 1f;
    }

    /// <summary>
    /// AudioSource
    /// </summary>
    [SerializeField]
    private AudioSource audioSource = null;
    /// <summary>
    /// 優先度
    /// </summary>
    public int priority { get; private set; }
    /// <summary>
    /// ループデータ
    /// </summary>
    public LoopData loopData = null;

    /// <summary>
    /// Reset
    /// </summary>
    private void Reset()
    {
        this.audioSource = this.GetComponent<AudioSource>();
    }

    /// <summary>
    /// Update
    /// </summary>
    private void Update()
    {
        if (this.loopData != null && this.audioSource.isPlaying && this.audioSource.clip != null)
        {
            float loopStartTime = this.audioSource.clip.length * this.loopData.startNormalizedTime;
            float loopEndTime = this.audioSource.clip.length * this.loopData.endNormalizedTime;

            if (this.audioSource.time >= loopEndTime)
            {
                //ループ処理
                this.audioSource.time = loopStartTime + (this.audioSource.time - loopEndTime);
            }
        }
    }

    /// <summary>
    /// 再生中かどうか
    /// </summary>
    public bool IsPlaying()
    {
        return this.audioSource.clip != null && this.audioSource.isPlaying;
    }

    /// <summary>
    /// 指定のクリップを再生中かどうか
    /// </summary>
    public bool IsPlaying(AudioClip clip)
    {
        return this.IsPlaying() && this.audioSource.clip == clip;
    }

    /// <summary>
    /// 再生
    /// </summary>
    public void Play(AudioClip clip, int priority = 0)
    {
        this.audioSource.clip = clip;
        this.audioSource.volume = 1f;
        this.priority = priority;
        this.audioSource.Play();
    }

    /// <summary>
    /// 停止
    /// </summary>
    public void Stop()
    {
        this.audioSource.Stop();
        this.audioSource.clip = null;
        this.loopData = null;
    }
}
