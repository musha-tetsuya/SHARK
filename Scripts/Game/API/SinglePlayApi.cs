using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;

/// <summary>
/// シングルモード関連API
/// </summary>
public class SinglePlayApi
{
    /// <summary>
    /// ステータス
    /// </summary>
    public enum Status
    {
        NotOpen = 0,//未公開
        Opened  = 1,//公開済み
        Played  = 2,//プレイ済み
        Cleared = 3,//クリア済み
    }

    /// <summary>
    /// プレイ状況
    /// </summary>
    public enum PlayingStatus
    {
        Playing      = 1,   //プレイ中
        Finish       = 2,   //終了
        Interruption = 3,
    }

    /// <summary>
    /// クリア結果
    /// </summary>
    public enum ClearResult
    {
        DisConnect = 0,
        Retire     = 1, //リタイア
        Failed     = 2, //失敗
        Cleared    = 3, //クリア
    }

    /// <summary>
    /// 現在プレイ可能な（解放済み）ステージ情報
    /// </summary>
    public class TSingleStage
    {
        public uint stageId;
        public uint stageStatus;
        public uint clearRank;

        /// <summary>
        /// 解放済みかどうか
        /// </summary>
        public bool IsOpen()
        {
            return this.stageStatus > (int)Status.NotOpen;
        }

        /// <summary>
        /// クリアランクを持っているかどうか
        /// </summary>
        public bool HasClearRank()
        {
            return this.clearRank > (int)Rank.None;
        }
    }

    /// <summary>
    /// 現在プレイ可能な（解放済み）ワールド情報
    /// </summary>
    public class TSingleWorld
    {
        public uint worldId;
        public uint worldStatus;

        /// <summary>
        /// 解放済みかどうか
        /// </summary>
        public bool IsOpen()
        {
            return this.worldStatus > (int)Status.NotOpen;
        }
    }

    /// <summary>
    /// single/topのレスポンスデータ
    /// </summary>
    public class TopResponseData
    {
        public TSingleStage[] tSingleStage;
        public TSingleWorld[] tSingleWorld;
    }

    /// <summary>
    /// 通常報酬情報
    /// </summary>
    public class RewardData
    {
        public uint itemType;
        public uint itemId;
        public uint itemNum;
        public uint directFlg;
        [JsonProperty("mSingleStageRewardGroup")]
        public Dictionary<string, object> rewardData;
        [JsonProperty("mSingleStageRewardLotGroup")]
        public Dictionary<string, object> lotData;

        public void Set()
        {
            this.itemType = (uint)(long)this.lotData["itemType"];
            this.itemId = (uint)(long)this.lotData["itemId"];
            this.itemNum = (uint)(long)this.lotData["itemNum"];
            this.directFlg = (uint)(long)this.rewardData["directFlg"];
        }
    }

    /// <summary>
    /// single/clearのレスポンスデータ
    /// </summary>
    public class ClearResponseData
    {
        [JsonProperty("mSingleStageRewardFirstGroup")]
        public RewardData[] firstReward;
        [JsonProperty("mSingleStageReward")]
        public RewardData[] normalReward;
        public bool isMaxPossession;

        public bool IsReceivedFirstReward()
        {
            return (this.firstReward == null || this.firstReward.Length == 0);
        }
    }

    /// <summary>
    /// シングルプレイトップ画面情報取得通信
    /// </summary>
    public static void CallTopApi(Action<TopResponseData> onCompleted)
    {
        var request = new SharkWebRequest<TopResponseData>("single/top");

        request.SetRequestHeader("AccessToken", UserData.Get().hash);

        request.onSuccess = (response) =>
        {
            onCompleted?.Invoke(response);
        };

        request.Send();
    }

    /// <summary>
    /// シングルバトル開始通信
    /// </summary>
    public static void CallStartApi(uint stageId, Action onCompleted)
    {
#if SHARK_OFFLINE
        onCompleted?.Invoke();
        return;
#endif

        var request = new SharkWebRequest<object>("single/start");

        request.SetRequestHeader("AccessToken", UserData.Get().hash);

        request.SetRequestParameter(new Dictionary<string, object>
        {
            { "stageId", stageId }
        });

        request.onSuccess = (response) =>
        {
            UserData.Get().coin -= Masters.SingleStageDB.FindById(stageId).needCoin;
            onCompleted?.Invoke();
        };

        request.Send();
    }

    /// <summary>
    /// シングルバトル終了通信
    /// </summary>
    public static void CallClearApi(
        ClearResult clearResult,
        Rank clearRank,
        Action<ClearResponseData> onCompleted)
    {
        var request = new SharkWebRequest<ClearResponseData>("single/clear");

        request.SetRequestHeader("AccessToken", UserData.Get().hash);

        request.SetRequestParameter(new Dictionary<string, object>
        {
            { "playingStatus", (int)PlayingStatus.Finish },
            { "result", (int)clearResult },
            { "clearRank", (int)clearRank },
        });

        request.onSuccess = (response) =>
        {
            foreach (var reward in response.firstReward)
            {
                if (reward.directFlg > 0)
                {
                    UserData.Get().AddItem((ItemType)reward.itemType, reward.itemId, reward.itemNum);
                }
            }

            foreach (var reward in response.normalReward)
            {
                reward.Set();
                if (reward.directFlg > 0)
                {
                    UserData.Get().AddItem((ItemType)reward.itemType, reward.itemId, reward.itemNum);
                }
            }

            HomeScene.isMaxPossession = response.isMaxPossession;

            onCompleted?.Invoke(response);
        };

        request.Send();
    }
}
