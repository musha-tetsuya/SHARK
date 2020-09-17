using UnityEngine;
using UnityEngine.UI;

public class UserOptionDialogContent : MonoBehaviour
{
    /// <summary>
    /// Bgmスライダー
    /// </summary>
    [SerializeField]
    private Slider bgmAudioSlider = null;
    /// <summary>
    /// Seスライダー
    /// </summary>
    [SerializeField]
    private Slider seAudioSlider = null;

    // PlayerPrefsデータ
    private int bgmValue = 0;
    private int seValue = 0;

    /// <summary>
    /// スライダー値と音量の関係
    /// </summary>
    private static readonly float[] sliderToVolume = { 0.0f, 0.7f, 0.8f, 0.9f, 1.0f };

    /// <summary>
    /// BGM音量設定
    /// </summary>
    public static void SetBgmVolume(int sliderValue)
    {
        SoundManager.Instance.bgmVolume = sliderToVolume[sliderValue];
    }

    /// <summary>
    /// SE音量設定
    /// </summary>
    public static void SetSeVolume(int sliderValue)
    {
        SoundManager.Instance.seVolume = sliderToVolume[sliderValue];
    }

    /// <summary>
    /// セットアップ
    /// </summary>
    public void Setup(SimpleDialog dialog)
    {
        dialog.titleText.text = Masters.LocalizeTextDB.Get("Option");
        dialog.closeButtonEnabled = true;
        dialog.onClose = this.OnClose;

        this.bgmAudioSlider.value = this.bgmValue = UserData.bgmVolume;
        this.seAudioSlider.value = this.seValue = UserData.seVolume;
    }

    /// <summary>
    /// ダイアログ閉じる時
    /// </summary>
    private void OnClose()
    {
        UserData.bgmVolume = this.bgmValue;
        UserData.seVolume = this.seValue;
    }

    /// <summary>
    /// Bgmスライダーの値が変化する場合
    /// </summary>
    public void OnChangeBgmAudioValue()
    {
        this.bgmValue = Mathf.RoundToInt(this.bgmAudioSlider.value);
        SetBgmVolume(this.bgmValue);
    }

    /// <summary>
    /// Seスライダーの値が変化する場合
    /// </summary>
    public void OnChangeSeAudioValue()
    {
        this.seValue = Mathf.RoundToInt(this.seAudioSlider.value);
        SetSeVolume(this.seValue);
        SoundManager.Instance.PlaySe(SeName.YES);
    }

}
