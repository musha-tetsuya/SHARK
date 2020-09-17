using System;
using System.Collections.Generic;
using System.Linq;
using Master;
using Newtonsoft.Json;
using UnityEngine;

/// <summary>
/// 課金関連API.
/// </summary>
public class BillingApi
{
    /// <summary>
    /// billing/Buyのエラータイプ
    /// </summary>
    public enum BillingBuyErrorType
    {
        None,
        BillingStatusType,
        GooglePurchaseState,
        ErrorCodeType,
    }

    /// <summary>
    /// Google購入結果
    /// </summary>
    public class GoogleBuyResult
    {
        //https://developers.google.com/android-publisher/api-ref/purchases/products
        public enum PurchaseState
        {
            Purchased   = 0,
            Canceled    = 1,
            Pending     = 2,
        }

        public int purchaseState;
    }

    /// <summary>
    /// billing/buyのレスポンスデータ
    /// </summary>
    public class BuyResponseData
    {
        public UserBillingData tBilling;
        public BillingData mBilling;
        public BillingItemData[] mBillingItem;
        public TUsers tUsers;
#if UNITY_EDITOR || UNITY_ANDROID
        public GoogleBuyResult result;
#endif
    }

    /// <summary>
    /// billing/historyのレスポンスデータ
    /// </summary>
    public class HistoryResponseData
    {
        public List<UserBillingData> tBilling;
    }

    /// <summary>
    /// 有償石の購入
    /// </summary>
    public static void CallBuyApi(
        string productId,
        string receipt,
        Action<UserBillingData> onCompleted,
        Action<BillingBuyErrorType, int> onError)
    {
#if DEBUG
        if (receipt == "dummy")
        {
            CoroutineUpdator.Create(null, () =>
            {
                var billingData = Masters.BillingDB.GetList().Find(x => x.productId == productId);
                var billingItems = Masters.BillingItemDB.GetList().FindAll(x => x.billingItemId == billingData.billingItemId);
                UserData.Get().chargeGem += (ulong)billingItems.Where(x => x.itemType == (uint)ItemType.ChargeGem).Sum(x => x.itemNum);
                UserData.Get().freeGem += (ulong)billingItems.Where(x => x.itemType == (uint)ItemType.FreeGem).Sum(x => x.itemNum);
                UserData.Get().coin += (ulong)billingItems.Where(x => x.itemType == (uint)ItemType.Coin).Sum(x => x.itemNum);
                onCompleted?.Invoke(new UserBillingData{ billingId = billingData.id });
            });
            return;
        }
#endif

        //リクエスト作成
        var request = new SharkWebRequest<BuyResponseData>("billing/buy");

        request.SetRequestHeader("AccessToken", UserData.Get().hash);

        //リクエストパラメータセット
        request.SetRequestParameter(new Dictionary<string, object>
        {
            { "productId", productId },
            { "receipt", receipt },
            { "platform", SharkDefine.DEVICE_TYPE },
        });

        //通信完了時コールバック登録
        request.onSuccess = (response) =>
        {
            //サーバーでアイテム付与成功
            if (response.tBilling.status == (uint)UserBillingData.Status.AddItem)
            {
                //クライアントもアイテム付与
                foreach (var item in response.mBillingItem)
                {
                    UserData.Get().AddItem((ItemType)item.itemType, item.itemId, item.itemNum);
                }

                // tUsers更新
                if (response.tUsers != null)
                {
                    UserData.Get().Set(response.tUsers);
                }

                //通信完了
                onCompleted?.Invoke(response.tBilling);
            }
            //何らかのエラー
            else
            {
                BillingBuyErrorType errorType = BillingBuyErrorType.None;
                int errorCode = 0;

                if (response.tBilling.status == (uint)UserBillingData.Status.ReceiptChkFailed)
                {
                    //レシート検証に失敗：念のためペンディングは解除しない
                    errorType = BillingBuyErrorType.BillingStatusType;
                    errorCode = (int)UserBillingData.Status.ReceiptChkFailed;
                }
#if UNITY_EDITOR || UNITY_ANDROID
                else if (response.result.purchaseState == (int)GoogleBuyResult.PurchaseState.Canceled)
                {
                    //キャンセル済み：ペンディング解除OK
                    errorType = BillingBuyErrorType.GooglePurchaseState;
                    errorCode = (int)GoogleBuyResult.PurchaseState.Canceled;
                }
                else if (response.result.purchaseState == (int)GoogleBuyResult.PurchaseState.Pending)
                {
                    //ペンディング中：ペンディング解除NG
                    errorType = BillingBuyErrorType.GooglePurchaseState;
                    errorCode = (int)GoogleBuyResult.PurchaseState.Pending;
                }
#endif
                //エラー通知
                onError?.Invoke(errorType, errorCode);
            }
        };

        //エラーコールバック登録
        request.onError = (errodCode) =>
        {
            onError?.Invoke(BillingBuyErrorType.ErrorCodeType, errodCode);
        };

        //通信開始
        request.Send();
    }

    /// <summary>
    /// 有償石の購入履歴の取得
    /// </summary>
    public static void CallHistoryApi(Action onCompleted)
    {
        //リクエスト作成
        var request = new SharkWebRequest<HistoryResponseData>("billing/history");

        request.SetRequestHeader("AccessToken", UserData.Get().hash);

        //通信完了時コールバック登録
        request.onSuccess = (response) =>
        {
            //通信完了
            onCompleted?.Invoke();
        };

        //通信開始
        request.Send();
    }

    /// <summary>
    /// 有償石の商品リストの取得
    /// </summary>
    public static void CallListApi(Action onCompleted)
    {
#if SHARK_OFFLINE
        CoroutineUpdator.Create(null, () =>
        {
            Masters.BillingDB.GetList().RemoveAll(x => x.platform != SharkDefine.DEVICE_TYPE);
            onCompleted?.Invoke();
        });
        return;
#endif
        //リクエスト作成
        var request = new SharkWebRequest<Dictionary<string, object>>("billing/list");

        request.SetRequestHeader("AccessToken", UserData.Get().hash);

        //通信完了時コールバック登録
        request.onSuccess = (response) =>
        {
            //商品リスト更新
            Masters.BillingDB.SetList(response[Masters.BillingDB.tableName].ToString());
            Masters.BillingGroupDB.SetList(response[Masters.BillingGroupDB.tableName].ToString());
            Masters.BillingItemDB.SetList(response[Masters.BillingItemDB.tableName].ToString());

            //通信完了
            onCompleted?.Invoke();
        };

        //通信開始
        request.Send();
    }
}
