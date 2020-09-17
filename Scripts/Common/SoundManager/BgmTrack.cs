using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// BGMトラック
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class BgmTrack : MonoBehaviour
{
    /// <summary>
    /// AudioSource
    /// </summary>
    [SerializeField]
    private AudioSource audioSource = null;

    /// <summary>
    /// BGMクリップデータ
    /// </summary>
    private BgmClipData clipData = null;
    /// <summary>
    /// フェードコルーチン
    /// </summary>
    private Coroutine fadeCoroutine = null;

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
        if (this.audioSource.isPlaying && this.audioSource.clip != null && this.clipData != null)
        {
            float loopStartTime = this.audioSource.clip.length * this.clipData.loopStartNormalizedTime;
            float loopEndTime = this.audioSource.clip.length * this.clipData.loopEndNormalizedTime;

            if (this.audioSource.time >= loopEndTime)
            {
                //ループ処理
                this.audioSource.time = loopStartTime + (this.audioSource.time - loopEndTime);
            }
        }
    }

    /// <summary>
    /// 指定のBGMを再生中かどうか
    /// </summary>
    public bool IsPlaying(string bgmName)
    {
        return this.audioSource.isPlaying
            && this.clipData != null
            && this.clipData.name.Equals(bgmName, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// 再生
    /// </summary>
    public void Play(BgmClipData clipData, float fadeTime = 0f)
    {
        this.Stop();
        this.clipData = clipData;
        this.audioSource.clip = this.clipData.clip;
        this.audioSource.volume = this.clipData.volume;
        this.audioSource.Play();

        //フェードインするなら
        if (fadeTime > 0f)
        {
            this.fadeCoroutine = StartCoroutine(this.FadeIn(fadeTime));
        }
    }

    /// <summary>
    /// 一時停止
    /// </summary>
    public void Pause()
    {
        this.audioSource.Pause();
    }

    /// <summary>
    /// 停止
    /// </summary>
    public void Stop(float fadeTime = 0f, Action onStop = null)
    {
        //既に停止中
        if (!this.audioSource.isPlaying)
        {
            //タイムだけリセットする
            this.audioSource.time = 0f;
            return;
        }

        //フェード停止
        if (this.fadeCoroutine != null)
        {
            StopCoroutine(this.fadeCoroutine);
            this.fadeCoroutine = null;
        }

        //フェードアウトするなら
        if (fadeTime > 0f)
        {
            this.fadeCoroutine = StartCoroutine(this.FadeOut(fadeTime, () =>
            {
                this.Stop(0f, onStop);
            }));
            return;
        }

        //停止
        this.audioSource.Stop();
        onStop?.Invoke();
        onStop = null;
    }

    /// <summary>
    /// フェードイン
    /// </summary>
    private IEnumerator FadeIn(float fadeTime)
    {
        float time = 0f;
        float endVolume = this.audioSource.volume;

        while (time < fadeTime)
        {
            if (this.audioSource.isPlaying)
            {
                this.audioSource.volume = Mathf.Lerp(0f, endVolume, time / fadeTime);
                time += Time.deltaTime;
            }
            yield return null;
        }

        this.audioSource.volume = endVolume;
    }

    /// <summary>
    /// フェードアウト
    /// </summary>
    private IEnumerator FadeOut(float fadeTime, Action onFinished = null)
    {
        float time = 0f;
        float startVolume = this.audioSource.volume;

        while (time < fadeTime)
        {
            if (this.audioSource.isPlaying)
            {
                this.audioSource.volume = Mathf.Lerp(startVolume, 0f, time / fadeTime);
                time += Time.deltaTime;
            }
            yield return null;
        }

        this.audioSource.volume = 0f;

        onFinished?.Invoke();
    }

#if UNITY_EDITOR
    /// <summary>
    /// カスタムインスペクター
    /// </summary>
    [CustomEditor(typeof(BgmTrack))]
    private class MyInspector : Editor
    {
        /// <summary>
        /// target
        /// </summary>
        private new BgmTrack target => base.target as BgmTrack;

        /// <summary>
        /// OnInspectorGUI
        /// </summary>
        public override void OnInspectorGUI()
        {
            this.serializedObject.Update();

            EditorGUILayout.PropertyField(this.serializedObject.FindProperty("audioSource"));

            this.target.clipData = EditorGUILayout.ObjectField("ClipData", this.target.clipData, typeof(BgmClipData), false) as BgmClipData;

            if (this.target.clipData != null)
            {
                if (this.target.clipData.clip != null)
                {
                    float length = this.target.clipData.clip.length;

                    //Editor再生中しか触れない
                    EditorGUI.BeginDisabledGroup(!Application.isPlaying);
                    {
                        EditorGUILayout.BeginHorizontal();
                        {
                            if (this.target.audioSource.isPlaying)
                            {
                                if (GUILayout.Button("Pause"))
                                {
                                    this.target.Pause();
                                }
                            }
                            else
                            {
                                if (GUILayout.Button("Play"))
                                {
                                    this.target.Play(this.target.clipData);
                                }
                            }

                            if (GUILayout.Button("Stop"))
                            {
                                this.target.Stop();
                            }
                        }
                        EditorGUILayout.EndHorizontal();

                        //シーク位置
                        this.target.audioSource.time = EditorGUILayout.Slider("シーク位置", this.target.audioSource.time, 0f, length);
                    }
                    EditorGUI.EndDisabledGroup();
                }

                //水平線
                GUILayout.Box("",GUILayout.ExpandWidth(true), GUILayout.Height(1));

                //クリップデータ名
                GUILayout.Label(this.target.clipData.name);

                //クリップデータ情報
                this.target.clipData.OnInspectorGUI();

                this.Repaint();
            }

            this.serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}
