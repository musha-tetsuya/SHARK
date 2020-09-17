using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// SharkIAP制御
/// </summary>
public class SharkIAPController
{
    /// <summary>
    /// IAP
    /// </summary>
    private SharkIAP iap = new SharkIAP();
    /// <summary>
    /// 処理中フラグ
    /// </summary>
    private bool isBusy = false;

    /// <summary>
    /// 初期化
    /// </summary>
    public void Initialize(Action onSuccess, Action onFailed)
    {
        if (this.isBusy)
        {
            return;
        }

        this.isBusy = true;

        //IAP初期化
        this.iap.Initialize(
            productIds: Masters.BillingDB.GetList().Select(x => x.productId).ToArray(),
            onInitialized: () =>
            {
                this.isBusy = false;
                onSuccess?.Invoke();
            },
            onInitializeFailed: () =>
            {
                var dialog = SharedUI.Instance.ShowSimpleDialog(true);
                var content = dialog.SetAsMessageDialog(Masters.LocalizeTextDB.Get("IAPInitializeError"));
                content.buttonGroup.buttons[0].onClick = () =>
                {
                    this.isBusy = false;
                    dialog.Close();
                    dialog.onClose = onFailed;
                };
            }
        );
    }

    /// <summary>
    /// 購入
    /// </summary>
    public void Purchase(string productId, Action<UserBillingData> onSuccess, Action onFailed)
    {
        if (this.isBusy)
        {
            return;
        }

        this.isBusy = true;

        //課金処理
        this.iap.Purchase(
            productId: productId,
            onProcessPurchase: (receipt) =>
            {
                //サーバーにレシート渡す
                this.CallBuyApi(
                    productId: productId,
                    receipt: receipt,
                    onSuccess: (userBillingData) =>
                    {
                        this.isBusy = false;
                        onSuccess?.Invoke(userBillingData);
                    },
                    onFailed: onFailed
                );
            },
            onPurchaseFailed: (p) =>
            {
                if (p == UnityEngine.Purchasing.PurchaseFailureReason.UserCancelled)
                {
                    //ユーザーによるキャンセルなので特に表示なし
                    this.isBusy = false;
                    onFailed?.Invoke();
                }
                else
                {
                    //課金失敗をダイアログで通知
                    var dialog = SharedUI.Instance.ShowSimpleDialog(true);
                    var content = dialog.SetAsMessageDialog(Masters.LocalizeTextDB.Get("IAPPurchaseError"));
                    content.buttonGroup.buttons[0].onClick = () =>
                    {
                        this.isBusy = false;
                        dialog.Close();
                        dialog.onClose = onFailed;
                    };
                }
            }
        );
    }

    /// <summary>
    /// 購入通信
    /// </summary>
    private void CallBuyApi(string productId, string receipt, Action<UserBillingData> onSuccess, Action onFailed)
    {
#if false
        //レシート出力
        System.IO.File.WriteAllText(Application.persistentDataPath + "/iap.log", receipt);
#endif

        BillingApi.CallBuyApi(
            productId: productId,
            receipt: receipt,
            onCompleted: (userBillingData) =>
            {
                //課金完了
                this.iap.ConfirmPendingPurchase(productId);
                onSuccess?.Invoke(userBillingData);
            },
            onError: (errorType, errorCode) =>
            {
                if (errorType == BillingApi.BillingBuyErrorType.GooglePurchaseState && errorCode == (int)BillingApi.GoogleBuyResult.PurchaseState.Canceled)
                {
                    //キャンセル済みレシートなのでペンディング解消する
                    this.iap.ConfirmPendingPurchase(productId);
                }
                else if (errorType == BillingApi.BillingBuyErrorType.ErrorCodeType && errorCode == (int)ErrorCode.AlreadyCheckReceiptId)
                {
                    //すでにアイテム付与済みのレシートなのでペンディング解消する
                    this.iap.ConfirmPendingPurchase(productId);
                }

                //エラーコード通知
                var dialog = SharedUI.Instance.ShowSimpleDialog(true);
                var content = dialog.SetAsMessageDialog(string.Format("ERROR_TYPE : {0}\nERROR_CODE : {1}", (int)errorType, errorCode));
                content.buttonGroup.buttons[0].onClick = () =>
                {
                    dialog.Close();
                    dialog.onClose = onFailed;
                };
            });
    }

    /// <summary>
    /// ペンディング中商品の解決
    /// </summary>
    public void ResolvePending(Action onCompleted)
    {
        //ペンディング中商品の取得
        var pendingProducts = this.iap.GetPendingProducts();

        if (pendingProducts == null || pendingProducts.Length == 0)
        {
            //ペンディング中商品は無い
            var dialog = SharedUI.Instance.ShowSimpleDialog();
            var content = dialog.SetAsMessageDialog(Masters.LocalizeTextDB.Get("PendingProductNotFound"));
            content.buttonGroup.buttons[0].onClick = dialog.Close;
        }
        else
        {
            //ペンディング解消開始
            this.ResolvePendingInternal(onCompleted);
        }
    }

    /// <summary>
    /// ペンディング解消処理
    /// </summary>
    private void ResolvePendingInternal(Action onCompleted)
    {
        //ペンディング中商品の取得
        var pendingProducts = this.iap.GetPendingProducts();

        if (pendingProducts == null || pendingProducts.Length == 0)
        {
            //全てのペンディングを解消したことを通知
            var dialog = SharedUI.Instance.ShowSimpleDialog();
            var content = dialog.SetAsMessageDialog(Masters.LocalizeTextDB.Get("ResolvePendingMessage"));
            content.buttonGroup.buttons[0].onClick = () =>
            {
                dialog.onClose = onCompleted;
                dialog.Close();
            };
        }
        else
        {
            //サーバーにレシート渡す
            this.CallBuyApi(
                productId: pendingProducts[0].definition.id,
                receipt: pendingProducts[0].receipt,
                onSuccess: (userBillingData) =>
                {
                    //次のペンディング商品の解決へ
                    this.ResolvePendingInternal(onCompleted);
                },
                onFailed: () =>
                {
                    //何か失敗したのでここまで
                    onCompleted?.Invoke();
                });
        }
    }

    /// <summary>
    /// ペンディング中商品数取得
    /// </summary>
    public int GetPengingProductsCount()
    {
        var pendingProducts = this.iap.GetPendingProducts();
        return (pendingProducts == null) ? 0 : pendingProducts.Length;
    }
}
