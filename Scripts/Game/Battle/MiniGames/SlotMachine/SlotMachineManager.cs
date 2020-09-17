using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class SlotMachineManager : MonoBehaviour
{
    /// <summary>
    /// アニメーター
    /// </summary>
    [SerializeField]
    private Animator animator = null;
    /// <summary>
    /// スロットオブジェクト
    /// </summary>
    [SerializeField]
    private RectTransform[] slotItemObject = null;
    /// <summary>
    /// スロットイメージリスト
    /// </summary>
    [System.Serializable]
    private class DisplayItemSlot
    {
        public List<Image> slotSprite = new List<Image>();
    }
    [SerializeField]
    private DisplayItemSlot[] displayItemSlots = null;
    /// <summary>
    /// 全スロットのスプライト
    /// </summary>
    [SerializeField]
    private Sprite[] slotSprites = null;
    /// <summary>
    /// スロット別、使用するスプライトIndex数
    /// </summary>
    private int itemCount = 10;
    /// <summary>
    /// 停止するインデックス値(SlotSprites スプライトIndex)
    /// </summary>
    private int[] answer = { 0, 0, 0 };
    /// <summary>
    /// TODO.Startイメージアニメ
    /// </summary>
    [SerializeField]
    Animation startTextAnimator = null;
    /// <summary>
    /// 変更するスロットマシーンフレイムイメージ対象
    /// </summary>
    [SerializeField]
    private Image slotMachineFrame = null;
    /// <summary>
    /// スロットマシーンフレイムイメージ
    /// </summary>
    [SerializeField]
    private Sprite[] slotMachineFrames = null;
    /// <summary>
    /// コインエフェクト中プレハブ
    /// </summary>
    [SerializeField]
    private CoinEffect coinEffectMiddlePrefab = null;
    /// <summary>
    /// コインエフェクト大プレハブ
    /// </summary>
    [SerializeField]
    private CoinEffect coinEffectBigPrefab = null;

    /// <summary>
    /// TODO.スロットマシン段階(API Response)
    /// </summary>
    private uint jackpotChanceId = 3;
    /// <summary>
    /// TODO.スロットマシン当選ID(API Response) ResponseがNullなら0
    /// </summary>
    private uint lotteryId = 0;
    /// <summary>
    /// TODO. 臨時スロット配列値(後、マスターデータで管理？)
    /// </summary>
    private int[][] slot =
    {
        new int[]{ 5, 4, 3, 2, 1, 0, 2, 4, 1, 2 },
        new int[]{ 4, 2, 4, 5, 3, 0, 1, 5, 1, 2 },
        new int[]{ 1, 5, 4, 1, 2, 0, 4, 1, 2, 3 },
    };
    /// <summary>
    /// 初期位置
    /// </summary>
    private Vector3 startPosition = new Vector3(0f, 1600f, 0f);
    /// <summary>
    /// スロットに必要なレスポンスデータ
    /// </summary>
    private MultiPlayApi.GetSoulResponseData response = null;
    /// <summary>
    /// スロット回転時SEトラック
    /// </summary>
    private SeTrack slotSeTrack = null;
    /// <summary>
    /// スロット停止フラグ
    /// </summary>
    private bool isStop = false;
    /// <summary>
    /// クジラ生成タイミングコールバック
    /// </summary>
    private Action<uint> onCreateWhale = null;
    /// <summary>
    /// スロット演出終了時コールバック
    /// </summary>
    private Action onFinished = null;

    /// <summary>
    /// Awake
    /// </summary>
    private void Awake()
    {
        // StartTextAnimation停止状態で待機
        this.startTextAnimator.Stop();

        // スロットリールスプライトセット
        for (int i = 0; i < this.slotItemObject.Length; i++)
        {
            for (int j = 0; j < this.displayItemSlots[i].slotSprite.Count; j++)
            {
                int slotIndex = (int)Mathf.Repeat(j - 1, this.slot[i].Length);//0～9
                int spriteNo = this.slot[i][slotIndex];//0～5
                this.displayItemSlots[i].slotSprite[j].sprite = this.slotSprites[spriteNo];
            }
        }
    }

    /// <summary>
    /// Set
    /// </summary>
    public void Set(MultiPlayApi.GetSoulResponseData response, Action<uint> onCreateWhale, Action onFinished)
    {
        // レスポンス受け取り
        this.response = response;
        // 各コールバックセット
        this.onCreateWhale = onCreateWhale;
        this.onFinished = onFinished;
        // スロット段階
        this.jackpotChanceId = this.response.tMultiSoulBall.soul / 3;
        this.animator.SetInteger("JackpotChanceId", (int)this.jackpotChanceId);
        // 取得するId (現在1~5まで、あり)
        this.lotteryId = this.response.jackpotHit ? this.response.mMultiJackpotLottery[0].lotteryId : 0;

        // 1段スロット、リール停止値設定
        if (this.jackpotChanceId == 1)
        {
            this.slotMachineFrame.sprite = this.slotMachineFrames[this.jackpotChanceId - 1];
            StopValueRule();
        }
        // 2段スロット、リール停止値設定
        else if (this.jackpotChanceId == 2)
        {
            this.slotMachineFrame.sprite = this.slotMachineFrames[this.jackpotChanceId - 1];
            StopValueRule();
        }
        // 3段スロット、リール停止値設定
        else
        {
            this.slotMachineFrame.sprite = this.slotMachineFrames[this.jackpotChanceId - 1];
            // ジャックポッド(3段のみ)
            if (lotteryId == 1)
            {
                // ジャックポットセット(0番目のslotSprite)
                SetSlotResult(0, 0, 0);
            }
            else
            {
                StopValueRule();
            }
        }
    }

    /// <summary>
    /// 停止するスロットルール設定
    /// </summary>
    private void StopValueRule()
    {
        // 当たり
        if (lotteryId > 0)
        {
            int stopValue = UnityEngine.Random.Range(1, slotSprites.Length);
            SetSlotResult(stopValue, stopValue, stopValue);
        }
        // 外れ
        else if (lotteryId == 0)
        {
            int[] nums = { 0, 0, 1, 1, 2, 2, 3, 3, 4, 4, 5, 5 };
            int[] randomNums = nums.OrderBy(x => UnityEngine.Random.Range(0, 100)).Take(3).ToArray();
            SetSlotResult(randomNums);
        }
    }

    /// <summary>
    /// 停止するスロットルール設定
    /// </summary>
    private void SetSlotResult(params int[] stopValue)
    {
        for (int i = 0; i < this.slotItemObject.Length; i++)
        {
            //0～9の値
            this.answer[i] = this.slot[i].Length - 1 - Array.IndexOf(this.slot[i], stopValue[i]);
        }
    }

    /// <summary>
    /// スロット開始
    /// </summary>
    private void StartSlotMachine()
    {
        //リール回転音ループ再生
        this.slotSeTrack = SoundManager.Instance.PlaySe(SeName.WHALE_SLOT, 100);
        this.slotSeTrack.loopData = new SeTrack.LoopData{ endNormalizedTime = 0.9f };

        // StartTextAnimation開始
        this.startTextAnimator.Play();

        // 位置設定
        for (int i = 0; i < this.slotItemObject.Length; i++)
        {
            this.slotItemObject[i].localPosition = this.startPosition;
        }

        // スロット回転コルチン実行
        StartCoroutine(this.StartSlot());

        // 段階別クジラの生成
        this.onCreateWhale?.Invoke(this.jackpotChanceId - 1);
    }

    /// <summary>
    /// スロット回転コルチン
    /// </summary>
    private IEnumerator StartSlot()
    {
        var rotationSize = new Vector3(0f, 80f, 0f);

        //停止したリール数
        int stopCount = 0;
        //フレームカウント
        int frameCount = 0;
        //最大フレーム数 = 20フレームで1周
        int maxFrame = this.itemCount * 2;
        //リール毎の停止までの待ち時間
        float[] delay = { 0f, 1f, 1f };

        //全リールが止まるまで
        while (stopCount < this.slotItemObject.Length)
        {
            //1f～0.05f
            float t = 1f - (float)frameCount / maxFrame;

            //リール回転
            for (int i = stopCount; i < this.slotItemObject.Length; i++)
            {
                this.slotItemObject[i].localPosition = this.startPosition * t;
            }

            //停止フラグが立ったとき
            if (this.isStop)
            {
                if (delay[stopCount] > 0f)
                {
                    //待ち
                    delay[stopCount] -= Time.deltaTime;
                }
                else if (frameCount == this.answer[stopCount] * 2)
                {
                    //リールが指定値になったら順にリール停止させる
                    stopCount++;
                    SoundManager.Instance.PlaySe(SeName.SLOT_STOP);
                }
            }

            //フレームカウント増加 0～19
            frameCount = (frameCount + 1) % maxFrame;

            yield return null;
        }

        //リール回転音停止
        this.slotSeTrack.Stop();

        if (this.response.jackpotHit)
        {
            //スロット当たったならコイン獲得演出表示
            CoinEffect coinEffect = null;
                
            //1,2段階目
            if (this.jackpotChanceId < 3)
            {
                SoundManager.Instance.PlaySe(SeName.WIN_0);
                coinEffect = Instantiate(this.coinEffectMiddlePrefab, this.transform, false);
            }
            //3段階目
            else
            {
                SoundManager.Instance.PlaySe(SeName.WIN_1);
                coinEffect = Instantiate(this.coinEffectBigPrefab, this.transform, false);
            }

            //獲得コイン数セット
            coinEffect.SetNum(this.response.mMultiJackpotLottery.Where(x => x.itemType == (uint)ItemType.Coin).Sum(x => x.itemNum));

            //コイン獲得演出終了後、1秒経ったらスロット演出終了通知
            coinEffect.onDestroy = () => StartCoroutine(new WaitForSeconds(1f).AddCallback(this.onFinished));
        }
        else
        {
            //スロット外れたので、1秒経ったらスロット演出終了通知
            StartCoroutine(new WaitForSeconds(1f).AddCallback(this.onFinished));
        }
    }

    /// <summary>
    /// スロット停止
    /// </summary>
    private void StopSlot()
    {
        this.isStop = true;
    }
}