using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// マルチモード関連API
/// </summary>
public class MultiPlayApi
{
    /// <summary>
    /// ステータス
    /// </summary>
    public enum Status
    {
        NotOpen = 0,//未公開
        Opened  = 1,//公開済み
    }

    /// <summary>
    /// 通信中表示を見せないWebRequest
    /// </summary>
    public class InvisibleWebRequest<T> : SharkWebRequest<T>
    {
        public InvisibleWebRequest(string path) : base(path){}
        protected override void ShowLoading(){}
        protected override void HideLoading(){}
    }

    /// <summary>
    /// エラー表示すら見せないWebRequest
    /// </summary>
    public class SilentWebRequest<T> : InvisibleWebRequest<T>
    {
        public SilentWebRequest(string path) : base(path){}
        protected override void OnSystemError(){}
    }

    /// <summary>
    /// 現在プレイ可能な（解放済み）ワールド情報
    /// </summary>
    public class TMultiWorld
    {
        public uint multiWorldId;
        public uint worldStatus;

        /// <summary>
        /// 解放済みかどうか
        /// </summary>
        public bool IsOpen()
        {
            return this.worldStatus > (int)Status.NotOpen;
        }
    }

    // TODO
    public class TJackpotInfo
    {
        public uint worldId;
        public long jackpotPoint;
    }

    /// <summary>
    /// multi/topのレスポンスデータ
    /// </summary>
    public class TopResponseData
    {
        public TMultiWorld[] tMultiWorld;
        public TJackpotInfo[] tMultiJackpot;
        public TMultiSoulBall[] tMultiSoulBall;
    }

    /// <summary>
    /// multi/multiClearのレスポンスデータ
    /// </summary>
    public class ClearResponseData
    {
        public TUsers tUsers;
    }

    /// <summary>
    /// 魚捕獲数データ
    /// </summary>
    public class FishCountData
    {
        public uint fishId;
        public uint amount;
    }

    /// <summary>
    /// ログデータ
    /// </summary>
    public class LogData
    {
        public long consumeCoin = 0;
        public long addCoin = 0;
        public uint fvCount = 0;
        public List<FishCountData> fishData = new List<FishCountData>();

        public Dictionary<string, object> ToRequestParameter()
        {
            return new Dictionary<string, object>
            {
                { "exp", UserData.Get().exp },
                { "coin", UserData.Get().coin },
                { "fv", Battle.BattleGlobal.instance.userData.fvPoint },
                { "consumeCoin", this.consumeCoin },
                { "addCoin", this.addCoin },
                { "fishData", this.fishData },
                { "fvCount", this.fvCount },
            };
        }
    }

    /// <summary>
    /// multiPlaying/levelUpのレスポンスデータ
    /// </summary>
    public class LevelUpResponseData
    {
        public bool levelUp;
        public TUsers tUsers;
        public AddItem[] mLevelReward;
    }

    /// <summary>
    /// multiPlaying/itemUseのレスポンスデータ
    /// </summary>
    public class ItemUseResponseData
    {
        public UserItemData tItem;
    }

    /// <summary>
    /// 龍玉、龍魂データ
    /// </summary>
    public class TMultiSoulBall
    {
        public uint worldId;
        public uint soul;
        public uint ball;
    }

    /// <summary>
    /// スロット抽選結果データ
    /// </summary>
    public class MMultiJackpotLottery : AddItem
    {
        public uint lotteryId;
    }

    /// <summary>
    /// multiPlaying/getBallのレスポンスデータ
    /// </summary>
    public class GetBallResponseData
    {
        public bool success;
        public TMultiSoulBall tMultiSoulBall;
    }

    /// <summary>
    /// multiPlaying/getSoulのレスポンスデータ
    /// </summary>
    public class GetSoulResponseData : GetBallResponseData
    {
        public bool jackpotHit;
        public MMultiJackpotLottery[] mMultiJackpotLottery;
    }

    /// <summary>
    /// マルチプレイトップ画面情報取得通信
    /// </summary>
    public static void CallTopApi(Action<TopResponseData> onCompleted)
    {
        var request = new SharkWebRequest<TopResponseData>("multi/top");

        request.SetRequestHeader("AccessToken", UserData.Get().hash);

        request.onSuccess = (response) =>
        {
            onCompleted?.Invoke(response);
        };

        request.Send();
    }

    /// <summary>
    /// マルチゲーム開始
    /// </summary>
    public static void CallStartApi(uint worldId, string roomId, Action onCompleted)
    {
        var request = new SharkWebRequest<object>("multi/multiStart");

        request.SetRequestHeader("AccessToken", UserData.Get().hash);

        request.SetRequestParameter(new Dictionary<string, object>
        {
            { "multiWorldId", worldId },
            { "roomId", roomId },
        });

        request.onSuccess = (response) =>
        {
            onCompleted?.Invoke();
        };

        request.Send();
    }

    /// <summary>
    /// マルチゲーム終了
    /// </summary>
    public static void CallClearApi(LogData logData, Action onCompleted)
    {
        var request = new SharkWebRequest<ClearResponseData>("multi/multiClear");

        request.SetRequestHeader("AccessToken", UserData.Get().hash);

        request.SetRequestParameter(logData.ToRequestParameter());

        request.onSuccess = (response) =>
        {
            UserData.Get().Set(response.tUsers);
            onCompleted?.Invoke();
        };

        request.Send();
    }

    /// <summary>
    /// 定期ログ送信
    /// </summary>
    public static void CallLogApi(LogData logData)
    {
        var request = new SilentWebRequest<object>("multiPlaying/multiLog");

        request.SetRequestHeader("AccessToken", UserData.Get().hash);

        request.SetRequestParameter(logData.ToRequestParameter());

        request.Send();
    }

    /// <summary>
    /// レベルアップ通信
    /// </summary>
    public static void CallLevelUpApi(LogData logData, Action<LevelUpResponseData> onCompleted)
    {
        var request = new SharkWebRequest<LevelUpResponseData>("multiPlaying/levelUp");

        request.SetRequestHeader("AccessToken", UserData.Get().hash);

        request.SetRequestParameter(logData.ToRequestParameter());

        request.onSuccess = (response) =>
        {
            onCompleted?.Invoke(response);
        };

        request.Send();
    }

    /// <summary>
    /// アイテム使用通信
    /// </summary>
    public static void CallItemUseApi(UserItemData itemData, Action onCompleted)
    {
        var request = new SharkWebRequest<ItemUseResponseData>("multiPlaying/itemUse");

        request.SetRequestHeader("AccessToken", UserData.Get().hash);

        request.SetRequestParameter(new Dictionary<string, object>
        {
            { "itemId", itemData.itemId }
        });

        request.onSuccess = (response) =>
        {
            itemData.stockCount = response.tItem.stockCount;

            onCompleted?.Invoke();
        };

        request.Send();
    }

    /// <summary>
    /// 商品購入通信
    /// </summary>
    public static void CallItemBuyApi(uint shopId, uint buyNum, LogData logData, Action<UserShopData> onCompleted)
    {
        var request = new SharkWebRequest<ShopApi.BuyResponseData>("multiPlaying/itemBuy");

        request.SetRequestHeader("AccessToken", UserData.Get().hash);

        var requestParameter = logData.ToRequestParameter();
        requestParameter.Add("shopId", shopId);
        requestParameter.Add("buyNum", buyNum);

        request.SetRequestParameter(requestParameter);

        request.onSuccess = (response) =>
        {
            ShopApi.OnSuccessBuyApi(response, onCompleted);
        };

        request.Send();
    }

    /// <summary>
    /// 龍玉入手通信
    /// </summary>
    public static void CallGetBallApi(uint fishId, Action<GetBallResponseData> onCompleted)
    {
        var request = new InvisibleWebRequest<GetBallResponseData>("multiPlaying/getBall");

        request.SetRequestHeader("AccessToken", UserData.Get().hash);

        request.SetRequestParameter(new Dictionary<string, object>
        {
            { "fishId", fishId },
        });

        request.onSuccess = (response) =>
        {
            onCompleted?.Invoke(response);
        };

        request.Send();
    }

    /// <summary>
    /// 龍魂入手通信
    /// </summary>
    public static void CallGetSoulApi(uint fishId, Action<GetSoulResponseData> onCompleted)
    {
        var request = new InvisibleWebRequest<GetSoulResponseData>("multiPlaying/getSoul");

        request.SetRequestHeader("AccessToken", UserData.Get().hash);

        request.SetRequestParameter(new Dictionary<string, object>
        {
            { "fishId", fishId },
        });

        request.onSuccess = (response) =>
        {
            onCompleted?.Invoke(response);
        };

        request.Send();
    }

    /// <summary>
    /// フィーバータイム終了通信
    /// </summary>
    public static void CallFeverEndApi(Action onCompleted)
    {
        var request = new InvisibleWebRequest<object>("multiPlaying/feverEnd");

        request.SetRequestHeader("AccessToken", UserData.Get().hash);

        request.onSuccess = (response) =>
        {
            onCompleted?.Invoke();
        };

        request.Send();
    }
}
