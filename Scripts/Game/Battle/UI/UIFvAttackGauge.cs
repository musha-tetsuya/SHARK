using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace Battle {

/// <summary>
/// FVアタックゲージUI
/// </summary>
public class UIFvAttackGauge : MonoBehaviour
{
    [SerializeField]
    private bool isMultiBattle = false;
    /// <summary>
    /// ゲージアニメーター
    /// </summary>
    [SerializeField]
    private Animator gaugeAnimator = null;
    /// <summary>
    /// ゲージイメージ
    /// </summary>
    [SerializeField]
    private Image gaugeImage = null;
    /// <summary>
    /// パーセントテキスト
    /// </summary>
    [SerializeField]
    private Text percentText = null;
    /// <summary>
    /// タイプアイコンイメージ
    /// </summary>
    [SerializeField]
    private Image typeIconImage = null;
    /// <summary>
    /// ボタン
    /// </summary>
    [SerializeField]
    private Button button = null;
    /// <summary>
    /// 禁止マークイメージ
    /// </summary>
    [SerializeField]
    private Image banImage = null;
    /// <summary>
    /// FVアタック時間終了時イベント
    /// </summary>
    [SerializeField]
    private UnityEvent onFinishedFvAttackTime = null;

    /// <summary>
    /// バトル用ユーザーデータ
    /// </summary>
    private BattleUserData userData => BattleGlobal.instance.userData;
    /// <summary>
    /// ゲージ値
    /// </summary>
    private float gaugeValue = 0f;

    /// <summary>
    /// セットアップ
    /// </summary>
    public void Setup()
    {
        this.RefleshIconSprite();
        this.SetVisibleBanMark(false);
        this.RefleshView();
    }

    /// <summary>
    /// 見た目更新
    /// </summary>
    public void RefleshView()
    {
        this.SetGaugeValue((float)this.userData.fvPoint / this.userData.currentBetData.needFvPoint);
        this.RefleshGaugeFill();
        this.RefleshPercentText();
        this.RefleshButtonInteractable();
    }

    /// <summary>
    /// アイコン差し替え
    /// </summary>
    public void RefleshIconSprite()
    {
        var batteryData = Masters.BatteryDB.FindById(userData.turretData.batteryMasterId);
        var fvAttackData = Masters.FvAttackDB.FindById(batteryData.fvAttackId);
        var iconSpritePath = SharkDefine.GetFvAttackTypeIconSpritePath((FvAttackType)fvAttackData.type);
        var iconSpriteHandle = AssetManager.FindHandle<Sprite>(iconSpritePath);
        this.typeIconImage.sprite = iconSpriteHandle.asset as Sprite;
    }

    /// <summary>
    /// ゲージ値設定
    /// </summary>
    public void SetGaugeValue(float gaugeValue)
    {
        float beforeGaugeValue = this.gaugeValue;
        this.gaugeValue = gaugeValue;

        //ゲージが溜まった瞬間
        if (beforeGaugeValue < 1f && this.gaugeValue >= 1f)
        {
            //SE再生
            SoundManager.Instance.PlaySe(SeName.FVATTACK_OK);
            //エフェクトアニメーション再生、PUSHマーク表示
            this.gaugeAnimator.Play("FVButtonEF");
        }

        if (beforeGaugeValue >= 1f && this.gaugeValue < 1f)
        {
            //エフェクトアニメーション停止、PUSHマーク非表示
            this.gaugeAnimator.Play("None");
        }
    }

    /// <summary>
    /// セットしたゲージ値に合わせて、ゲージ表示更新
    /// </summary>
    public void RefleshGaugeFill()
    {
        this.gaugeImage.fillAmount = Mathf.Clamp01(this.gaugeValue);
    }

    /// <summary>
    /// セットしたゲージ値に合わせて、テキスト表示更新
    /// </summary>
    public void RefleshPercentText()
    {
        if (this.isMultiBattle)
        {
            //マルチバトルの場合、100％に達したら「×〇〇」表示
            int percentNum = Mathf.RoundToInt(this.gaugeValue * 10000) / 100;
            this.percentText.text = (percentNum < 100) 
                ? string.Format("{0}%", percentNum)
                : string.Format("×{0}", percentNum / 100);
        }
        else
        {
            //シングルバトルの場合
            int percentNum = Mathf.RoundToInt(Mathf.Clamp01(this.gaugeValue) * 10000) / 100;
            this.percentText.text = (percentNum < 100)
                ? string.Format("{0}%", percentNum)
                : null;
        }
    }

    /// <summary>
    /// ボタンの有効無効更新
    /// </summary>
    public void RefleshButtonInteractable()
    {
        this.button.interactable = this.gaugeValue >= 1f && !this.banImage.isActiveAndEnabled;
    }

    /// <summary>
    /// 禁止マークの表示切替
    /// </summary>
    public void SetVisibleBanMark(bool visible)
    {
        this.banImage.enabled = visible;
    }

    /// <summary>
    /// FVアタック時間更新時
    /// </summary>
    public void OnUpdateFvAttackTime(float nowTime, float maxTime)
    {
        float beforeFillAmount = this.gaugeImage.fillAmount;
        this.gaugeImage.fillAmount = Mathf.Clamp01(1f - nowTime / maxTime);

        //時間更新終了時
        if (beforeFillAmount > 0f && this.gaugeImage.fillAmount <= 0f)
        {
            this.onFinishedFvAttackTime?.Invoke();
        }
    }
}

}