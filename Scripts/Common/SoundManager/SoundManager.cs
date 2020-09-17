using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// サウンド管理
/// </summary>
public class SoundManager : SingletonMonoBehaviour<SoundManager>
{
    /// <summary>
    /// ミキサー最大音量
    /// </summary>
    private const float MAX_MIXER_VOLUME = 0f;
    /// <summary>
    /// ミキサー最小音量
    /// </summary>
    private const float MIN_MIXER_VOLUME = -80f;

    /// <summary>
    /// AudioMixer
    /// </summary>
    [SerializeField]
    private AudioMixer audioMixer = null;
    /// <summary>
    /// BGMトラックリスト
    /// </summary>
    [SerializeField]
    private BgmTrack[] bgmTrackList = null;
    /// <summary>
    /// SEトラックリスト
    /// </summary>
    [SerializeField]
    private SeTrack[] seTrackList = null;
    /// <summary>
    /// 内臓BGMリスト
    /// </summary>
    [SerializeField]
    private List<BgmClipData> builtinBgmClips = null;
    /// <summary>
    /// 内臓SEリスト
    /// </summary>
    [SerializeField]
    private List<AudioClip> builtinSeClips = null;

    /// <summary>
    /// マスター音量
    /// </summary>
    public float masterVolume
    {
        get => this.GetVolume("Master Volume");
        set => this.SetVolume("Master Volume", value);
    }

    /// <summary>
    /// BGM音量
    /// </summary>
    public float bgmVolume
    {
        get => this.GetVolume("BGM Volume");
        set => this.SetVolume("BGM Volume", value);
    }

    /// <summary>
    /// SE音量
    /// </summary>
    public float seVolume
    {
        get => this.GetVolume("SE Volume");
        set => this.SetVolume("SE Volume", value);
    }

    /// <summary>
    /// 音量取得
    /// </summary>
    private float GetVolume(string name)
    {
        float volume;
        this.audioMixer.GetFloat(name, out volume);
        return Mathf.InverseLerp(MIN_MIXER_VOLUME, MAX_MIXER_VOLUME, volume);

    }

    /// <summary>
    /// 音量設定
    /// </summary>
    private void SetVolume(string name, float value)
    {
        float volume = Mathf.Lerp(MIN_MIXER_VOLUME, MAX_MIXER_VOLUME, value);
        this.audioMixer.SetFloat(name, volume);
    }

    /// <summary>
    /// BGMクリップを探す
    /// </summary>
    private BgmClipData FindBgmClip(string bgmName)
    {
        //ビルトインから探す
        var clip = this.builtinBgmClips.Find(x => x.name == bgmName);
        if (clip == null)
        {
            //無かったらロード済みアセットバンドルから探す
            var handle = AssetManager.FindHandle<BgmClipData>(SharkDefine.GetBgmClipPath(bgmName));
            if (handle != null)
            {
                clip = handle.asset as BgmClipData;
            }
        }
        return clip;
    }

    /// <summary>
    /// SEクリップを探す
    /// </summary>
    private AudioClip FindSeClip(string seName)
    {
        //ビルトインから探す
        var clip = this.builtinSeClips.Find(x => x.name == seName);
        if (clip == null)
        {
            //無かったらロード済みアセットバンドルから探す
            var handle = AssetManager.FindHandle<AudioClip>(SharkDefine.GetSeClipPath(seName));
            if (handle != null)
            {
                clip = handle.asset as AudioClip;
            }
        }
        return clip;
    }

    /// <summary>
    /// BGM再生
    /// </summary>
    public BgmTrack PlayBgm(string bgmName, float fadeIn = 0f, float fadeOut = 0.5f)
    {
        if (this.bgmTrackList[1].IsPlaying(bgmName))
        {
            //既に再生中
            return this.bgmTrackList[1];
        }

        var clipData = this.FindBgmClip(bgmName);
        if (clipData == null)
        {
            Debug.LogErrorFormat("BGM名:{0}は存在しません。", bgmName);
            return null;
        }

        return this.PlayBgm(clipData, fadeIn, fadeOut);
    }

    /// <summary>
    /// BGM再生
    /// </summary>
    public BgmTrack PlayBgm(BgmClipData clipData, float fadeIn = 0f, float fadeOut = 0.5f)
    {
        this.bgmTrackList[1].Stop(fadeOut);
        this.bgmTrackList[0].Play(clipData, fadeIn);
        this.bgmTrackList[0].transform.SetAsLastSibling();
        this.bgmTrackList = this.bgmTrackList.OrderBy(x => x.transform.GetSiblingIndex()).ToArray();
        return this.bgmTrackList[1];
    }

    /// <summary>
    /// SE再生
    /// </summary>
    /// <param name="seName">SEファイル名</param>
    /// <param name="priority">優先度</param>
    /// <param name="polyphonySize">同時発音許可数</param>
    public SeTrack PlaySe(string seName, int priority = 0, int polyphonySize = 2)
    {
        var clip = this.FindSeClip(seName);
        if (clip == null)
        {
            Debug.LogErrorFormat("SE名:{0}は存在しません。", seName);
            return null;
        }

        return this.PlaySe(clip, priority, polyphonySize);
    }

    /// <summary>
    /// SE再生
    /// </summary>
    /// <param name="clip">SEクリップ</param>
    /// <param name="priority">優先度</param>
    /// <param name="polyphonySize">同時発音許可数</param>
    public SeTrack PlaySe(AudioClip clip, int priority = 0, int polyphonySize = 2)
    {
        SeTrack freeTrack = null;
        polyphonySize = Mathf.Max(polyphonySize, 1);

        var sameTrack = this.seTrackList.Where(x => x.IsPlaying(clip));
        if (sameTrack.Count() >= polyphonySize)
        {
            //既に同時発音数以上発声していたら、発声中トラックのうち一番古いトラックを利用する
            freeTrack = sameTrack.First();
            freeTrack.Stop();
        }

        if (freeTrack == null)
        {
            //空いてるトラックを探す
            freeTrack = this.seTrackList.FirstOrDefault(x => !x.IsPlaying());
        }

        if (freeTrack == null)
        {
            //空いてるトラックがなかったら、プライオリティが低いトラックを利用する
            freeTrack = this.seTrackList.OrderBy(x => x.priority).First();
            freeTrack.Stop();
        }

        //空きトラックで再生
        freeTrack.Play(clip, priority);
        freeTrack.transform.SetAsLastSibling();

        //トラックリストのソート
        this.seTrackList = this.seTrackList.OrderBy(x => x.transform.GetSiblingIndex()).ToArray();

        return freeTrack;
    }

#if UNITY_EDITOR
    /// <summary>
    /// BGMトラックを探してセットする
    /// </summary>
    [ContextMenu("Find BgmTrackList")]
    private void FindBgmTrackList()
    {
        this.bgmTrackList = this.GetComponentsInChildren<BgmTrack>();
    }

    /// <summary>
    /// SEトラックを探してセットする
    /// </summary>
    [ContextMenu("Find SeTrackList")]
    private void FindSeTrackList()
    {
        this.seTrackList = this.GetComponentsInChildren<SeTrack>();
    }

    /// <summary>
    /// ビルトインBGMクリップデータを探してセットする
    /// </summary>
    [ContextMenu("Find Builtin BgmClips")]
    private void FindBuiltinBgmClips()
    {
        this.builtinBgmClips = AssetDatabase
            .FindAssets("", new[]{"Assets/Sunchoi/BuiltinResources/References/Sound/Bgm"})
            .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
            .Select(path => AssetDatabase.LoadAssetAtPath<BgmClipData>(path))
            .Where(asset => asset != null)
            .ToList();
    }

    /// <summary>
    /// ビルトインSEクリップを探してセットする
    /// </summary>
    [ContextMenu("Find Builtin SeClips")]
    private void FindBuiltinSeClips()
    {
        this.builtinSeClips = AssetDatabase
            .FindAssets("", new[]{"Assets/Sunchoi/BuiltinResources/References/Sound/Se"})
            .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
            .Select(path => AssetDatabase.LoadAssetAtPath<AudioClip>(path))
            .ToList();
    }
#endif
}
