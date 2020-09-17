using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// クジラパネルUI
/// </summary>
public class UIWhalePanel : MonoBehaviour
{
    /// <summary>
    /// アニメーター
    /// </summary>
    [SerializeField]
    private Animator animator = null;
    /// <summary>
    /// 軸
    /// </summary>
    [SerializeField]
    private RectTransform axis = null;
    /// <summary>
    /// 龍玉
    /// </summary>
    [SerializeField]
    private UIWhaleBall[] balls = null;
    /// <summary>
    /// 龍魂
    /// </summary>
    [SerializeField]
    private UIWhaleSoul[] souls = null;
    /// <summary>
    /// 龍玉通常レイアウト位置
    /// </summary>
    [SerializeField]
    private RectTransform[] soulNormalPosition = null;
    /// <summary>
    /// 龍玉三角形レイアウト位置
    /// </summary>
    [SerializeField]
    private RectTransform[] soulTrianglePosition = null;
    /// <summary>
    /// クジラ影ゲージ
    /// </summary>
    [SerializeField]
    private Image shapeGauge = null;
    /// <summary>
    /// WhaleDiveTextプレハブ
    /// </summary>
    [SerializeField]
    private AnimationEventReceiver whaleDiveTextPrefab = null;
    /// <summary>
    /// WhaleSlotTextプレハブ
    /// </summary>
    [SerializeField]
    private AnimationEventReceiver whaleSlotTextPrefab = null;
    /// <summary>
    /// スロットプレハブ
    /// </summary>
    [SerializeField]
    private SlotMachineManager slotMachinePrefab = null;
    /// <summary>
    /// スロット生成先
    /// </summary>
    [SerializeField]
    private RectTransform slotMachineParent = null;
    /// <summary>
    /// 演出中タッチブロック用オブジェクト
    /// </summary>
    [SerializeField]
    private GameObject touchBlock = null;
    /// <summary>
    /// 演出中黒背景
    /// </summary>
    [SerializeField]
    private GameObject bgBlack = null;

    /// <summary>
    /// ジャックポットクジラローダー
    /// </summary>
    private Battle.Fish.Loader[] fishLoader = null;
    /// <summary>
    /// スロットクジラローダー
    /// </summary>
    private Battle.Fish.Loader[] slotWhaleLoader = null;
    /// <summary>
    /// ローダー
    /// </summary>
    private AssetListLoader loader = new AssetListLoader();
    /// <summary>
    /// 龍玉取得時のレスポンスデータ
    /// </summary>
    private MultiPlayApi.GetBallResponseData ballResponse = null;
    /// <summary>
    /// 龍魂取得時のレスポンスデータ
    /// </summary>
    private MultiPlayApi.GetSoulResponseData soulResponse = null;
    /// <summary>
    /// フィーバータイムカウントダウンコルーチン
    /// </summary>
    private Coroutine updateFeverTimeCoroutine = null;

    /// <summary>
    /// OnDestroy
    /// </summary>
    private void OnDestroy()
    {
        this.loader.Unload();
        this.loader.Clear();
    }

    /// <summary>
    /// ロード
    /// </summary>
    public void Load(Action onLoaded = null)
    {
        if (this.loader.GetStatus() == AssetListLoader.Status.Empty)
        {
            Master.FishData[] whaleMasters =
            {
                Masters.FishDB.FindById(100062),//1段階目
                Masters.FishDB.FindById(101062),//2段階目
                Masters.FishDB.FindById(102062),//3段階目
                Masters.FishDB.FindById(103062),//1段階目
                Masters.FishDB.FindById(104062),//2段階目
                Masters.FishDB.FindById(105062),//3段階目
            };

            this.fishLoader = new Battle.Fish.Loader[3];
            this.fishLoader[0] = new Battle.Fish.Loader(whaleMasters[0].id, whaleMasters[0].key);//1段階目
            this.fishLoader[1] = new Battle.Fish.Loader(whaleMasters[1].id, whaleMasters[1].key);//2段階目
            this.fishLoader[2] = new Battle.Fish.Loader(whaleMasters[2].id, whaleMasters[2].key);//3段階目

            this.slotWhaleLoader = new Battle.Fish.Loader[3];
            this.slotWhaleLoader[0] = new Battle.Fish.Loader(whaleMasters[3].id, whaleMasters[3].key);//1段階目
            this.slotWhaleLoader[1] = new Battle.Fish.Loader(whaleMasters[4].id, whaleMasters[4].key);//2段階目
            this.slotWhaleLoader[2] = new Battle.Fish.Loader(whaleMasters[5].id, whaleMasters[5].key);//3段階目

            this.loader.AddRange(this.fishLoader.SelectMany(x => x));
            this.loader.AddRange(this.slotWhaleLoader.SelectMany(x => x));
            this.loader.Add<AudioClip>(SharkDefine.GetSeClipPath(SeName.BALL_DROP));
            this.loader.Add<AudioClip>(SharkDefine.GetSeClipPath(SeName.BALL_GET));
            this.loader.Add<AudioClip>(SharkDefine.GetSeClipPath(SeName.SOUL_DROP));
            this.loader.Add<AudioClip>(SharkDefine.GetSeClipPath(SeName.SOUL_GET));
            this.loader.Add<AudioClip>(SharkDefine.GetSeClipPath(SeName.SOUL_COMP));
            this.loader.Add<AudioClip>(SharkDefine.GetSeClipPath(SeName.WHALE_SLOT));
            this.loader.Add<AudioClip>(SharkDefine.GetSeClipPath(SeName.SLOT_STOP));
            this.loader.Load(onLoaded);
        }
    }

    /// <summary>
    /// Y軸角度設定
    /// </summary>
    public void SetAxisAngleY(float angle)
    {
        this.axis.localEulerAngles = new Vector3(0f, angle, 0f);
    }

    /// <summary>
    /// 龍玉イメージセット
    /// </summary>
    public void SetBallNum(uint num)
    {
        for (int i = 0; i < 9; i++)
        {
            this.balls[i].gameObject.SetActive(i < num);
        }
    }

    /// <summary>
    /// 龍魂イメージセット
    /// </summary>
    public void SetSoulNum(uint num)
    {
        this.souls[0].gameObject.SetActive(false);
        this.souls[1].gameObject.SetActive(false);
        this.souls[2].gameObject.SetActive(false);

        for (uint i = 0; i < num; i ++)
        {
            this.souls[i % 3].gameObject.SetActive(true);
            this.souls[i % 3].SetNumber(i + 1);
        }
    }

    /// <summary>
    /// 龍玉取得アニメーション再生
    /// </summary>
    public void PlayGetBallAnimation(MultiPlayApi.GetBallResponseData ballResponse, Vector3 dropPosition)
    {
        this.ballResponse = ballResponse;

        this.balls[this.ballResponse.tMultiSoulBall.ball - 1].PlayGetAnimation(dropPosition);

        //9個目の龍玉取得時
        if (this.ballResponse.tMultiSoulBall.ball == 9)
        {
            //UI触れなくする
            this.touchBlock.SetActive(true);

            //終わったら龍玉円形回転アニメーション再生（フィーバータイム開始演出スタート）
            this.balls[8].onFinished = (tag) =>
            {
                this.bgBlack.SetActive(true);
                this.animator.Play("circle", 0, 0f);
            };
        }
    }

    /// <summary>
    /// 龍玉円形回転開始時エフェクト再生
    /// </summary>
    private void OnCircleBallEffectStart(int i)
    {
        this.balls[i].PlayCircleStartEffectAnimation();
    }

    /// <summary>
    /// 龍玉円形回転エフェクト再生
    /// </summary>
    private void OnCircleBallEffect()
    {
        for (int i = 0; i < this.balls.Length; i++)
        {
            this.balls[i].PlayCircleEffectAnimation();
        }
    }

    /// <summary>
    /// 龍玉円形回転アニメーション中に訪れるWhaleDiveText表示タイミング
    /// </summary>
    private void OnWhaleDiveTextTiming()
    {
        //WhaleDiveTextアニメーション開始
        var whaleDiveText = Instantiate(this.whaleDiveTextPrefab, this.transform, false);

        //WhaleDiveTextアニメーション終了時
        whaleDiveText.onFinished = (tag) =>
        {
            //段階
            var i = (this.ballResponse.tMultiSoulBall.soul % 9) / 3;

            //クジラ生成
            Animator whaleModelAnimator;
            var whaleModel = Battle.Fish.CreateModel(this.fishLoader[i], Battle.BattleGlobal.instance.fishArea, out whaleModelAnimator);

            //クジラアニメーション完了を待つ
            StartCoroutine(new WaitAnimation(whaleModelAnimator).AddCallback(() =>
            {
                //クジラ破棄
                Destroy(whaleModel);
                whaleModel = null;
                whaleModelAnimator = null;

                //WhaleDiveText破棄
                Destroy(whaleDiveText.gameObject);
                whaleDiveText = null;

                //UI触れるようにする
                this.touchBlock.SetActive(false);

                //フィーバータイムカウントダウン開始
                this.bgBlack.SetActive(false);
                this.animator.Play("none", 0, 0f);
                this.updateFeverTimeCoroutine = this.StartCoroutine(this.UpdateFeverTimeGauge());
            }));
        };
    }

    /// <summary>
    /// フィーバータイムゲージ更新
    /// </summary>
    private IEnumerator UpdateFeverTimeGauge()
    {
        //フィーバータイム開始時刻取得
        var feverTimeStamp = Battle.BattleGlobal.GetTimeStamp();

        //フィーバータイム開始を通知
        (SceneChanger.currentScene as Battle.MultiBattleScene)?.OnStartFeverTime();

        //龍玉MAX表示
        this.SetBallNum(9);

        //1番目の龍玉点滅開始
        this.balls[0].PlayBlinkAnimation();

        this.shapeGauge.fillAmount = 1f;

        while (this.shapeGauge.fillAmount > 0f)
        {
            //タイムゲージ減少
            var elapsedTime = unchecked(Battle.BattleGlobal.GetTimeStamp() - feverTimeStamp) * Masters.MilliSecToSecond;
            this.shapeGauge.fillAmount = 1f - (elapsedTime / 120);

            for (int i = 0; i < 9; i++)
            {
                if (this.balls[i].gameObject.activeSelf)
                {
                    if (this.shapeGauge.fillAmount <= 1f - (i + 1) * (1f / 9f))
                    {
                        //時間が来たら順に龍玉消えていく
                        this.balls[i].gameObject.SetActive(false);

                        if (i + 1 < 9)
                        {
                            //次の龍玉の点滅開始
                            this.balls[i + 1].PlayBlinkAnimation();
                        }
                    }

                    break;
                }
            }

            yield return null;
        }

        //カウントダウン表示をリセット
        this.shapeGauge.fillAmount = 0f;
        this.SetBallNum(0);
        this.updateFeverTimeCoroutine = null;

        //フィーバータイム終了を通知
        (SceneChanger.currentScene as Battle.MultiBattleScene)?.OnFinishedFeverTime();
    }

    /// <summary>
    /// 龍魂取得アニメーション再生
    /// </summary>
    public void PlayGetSoulAnimation(MultiPlayApi.GetSoulResponseData soulResponse, Vector3 dropPosition)
    {
        this.soulResponse = soulResponse;

        //スロット開始かどうか
        bool isSlot = this.soulResponse.tMultiSoulBall.soul % 3 == 0;

        if (isSlot)
        {
            //スロット開始するのでこれ以上カウントダウンしない
            if (this.updateFeverTimeCoroutine != null)
            {
                StopCoroutine(this.updateFeverTimeCoroutine);
                this.updateFeverTimeCoroutine = null;
            }

            //スロット演出開始を通知
            (SceneChanger.currentScene as Battle.MultiBattleScene).OnStartSlot();
        }

        uint i = (this.soulResponse.tMultiSoulBall.soul - 1) % 3;
        this.souls[i].PlayGetAnimation(this.soulResponse.tMultiSoulBall.soul, dropPosition, this.transform as RectTransform, () =>
        {
            if (isSlot)
            {
                SoundManager.Instance.PlaySe(SeName.SOUL_COMP);

                //カウントダウン途中で止められたかもしれないので、カウントダウン表示をリセット
                this.shapeGauge.fillAmount = 0f;
                this.SetBallNum(0);

                //スロット開始なら龍魂画面外に飛び出す（演出中はタッチブロック）
                this.bgBlack.SetActive(true);
                this.touchBlock.SetActive(true);
                this.animator.Play("SoulFlyOut", 0, 0f);
            }
        });
    }

    /// <summary>
    /// 龍魂飛び出す演出終了時
    /// </summary>
    private void OnFinishedSoulFlyOut()
    {
        //トランジションにより自動で龍魂三角形レイアウトアニメーションへ遷移する

        for (int i = 0; i < 3; i++)
        {
            //龍魂三角形レイアウト位置へ切り替え
            var rectTransform = this.souls[i].transform as RectTransform;
            rectTransform.SetParent(this.soulTrianglePosition[i]);
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.localScale = Vector3.one;
        }
    }

    /// <summary>
    /// 龍魂三角形レイアウト演出終了時
    /// </summary>
    private void OnFinishedSoulTriangle()
    {
        //龍魂発光
        for (int i = 0; i < 3; i++)
        {
            this.souls[i].PlayEffectAnimation();
        }

        //スロットテキスト表示
        var slotText = Instantiate(this.whaleSlotTextPrefab, this.transform, false);

        //スロットテキスト表示終了時
        slotText.onFinished = (tag) =>
        {
            //スロットテキスト破棄
            Destroy(slotText.gameObject);
            slotText = null;

            //龍魂回転フレームアウト演出再生
            this.animator.Play("SoulCircle", 0, 0f);

            //上からスロット降りてくる
            var slotMachine = Instantiate(this.slotMachinePrefab, this.slotMachineParent, false);
            slotMachine.transform.localEulerAngles = -this.axis.localEulerAngles;
            slotMachine.Set(
                response: this.soulResponse,
                onCreateWhale: (i) =>
                {
                    //クジラ生成
                    Animator whaleModelAnimator;
                    var whaleModel = Battle.Fish.CreateModel(this.slotWhaleLoader[i], Battle.BattleGlobal.instance.fishArea, out whaleModelAnimator);

                    //クジラアニメーション完了を待つ
                    StartCoroutine(new WaitAnimation(whaleModelAnimator).AddCallback(() =>
                    {
                        //クジラ破棄
                        Destroy(whaleModel);
                        whaleModel = null;
                        whaleModelAnimator = null;
                    }));
                },
                onFinished: () =>
                {
                    //スロット破棄
                    Destroy(slotMachine.gameObject);
                    slotMachine = null;

                    //タッチブロック解除
                    this.bgBlack.SetActive(false);
                    this.touchBlock.SetActive(false);

                    //スロット演出終了を通知
                    (SceneChanger.currentScene as Battle.MultiBattleScene)?.OnFinishedSlot();
                }
            );
        };
    }

    /// <summary>
    /// 龍玉回転フレームアウト演出終了時
    /// </summary>
    private void OnFinishedSoulCircle()
    {
        for (int i = 0; i < 3; i++)
        {
            //龍魂通常レイアウト位置へ切り替え
            var rectTransform = this.souls[i].transform as RectTransform;
            rectTransform.SetParent(this.soulNormalPosition[i]);
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.localScale = Vector3.one;

            var angle = rectTransform.localEulerAngles;
            angle.z = 0f;
            rectTransform.localEulerAngles = angle;
        }

        this.SetSoulNum(this.soulResponse.tMultiSoulBall.soul % 9);
    }
}
