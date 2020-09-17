using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Purchasing;

/// <summary>
/// SHARKアプリ内課金
/// </summary>
public class SharkIAP : IStoreListener
{
    /// <summary>
    /// ストアコントローラ
    /// </summary>
    private IStoreController controller = null;
    /// <summary>
    /// 初期化完了時コールバック
    /// </summary>
    private Action onInitialized = null;
    /// <summary>
    /// 初期化失敗時コールバック
    /// </summary>
    private Action onInitializeFailed = null;
    /// <summary>
    /// 購入成功時コールバック
    /// </summary>
    private Action<string> onProcessPurchase = null;
    /// <summary>
    /// 購入失敗時コールバック
    /// </summary>
    private Action<PurchaseFailureReason> onPurchaseFailed = null;
    /// <summary>
    /// ペンディング中商品リスト
    /// </summary>
    private List<Product> pendingProducts = new List<Product>();

    /// <summary>
    /// 初期化
    /// </summary>
    public void Initialize(string[] productIds, Action onInitialized, Action onInitializeFailed)
    {
        var module = StandardPurchasingModule.Instance();
        var builder = ConfigurationBuilder.Instance(module);

        //商品の登録
        builder.AddProducts(productIds.Select(x => new ProductDefinition(x, ProductType.Consumable)));

        //コールバック登録
        this.onInitialized = onInitialized;
        this.onInitializeFailed = onInitializeFailed;

#if UNITY_EDITOR || (!UNITY_ANDROID && !UNITY_IOS)
        //ダミー初期化
        CoroutineUpdator.Create(null, this.onInitialized);
#else
        //初期化
        UnityPurchasing.Initialize(this, builder);
#endif
    }

    /// <summary>
    /// 初期化完了時
    /// </summary>
    void IStoreListener.OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        this.controller = controller;
        this.onInitialized?.Invoke();
    }

    /// <summary>
    /// 初期化失敗時
    /// </summary>
    void IStoreListener.OnInitializeFailed(InitializationFailureReason error)
    {
        this.onInitializeFailed?.Invoke();
    }

    /// <summary>
    /// 商品情報取得
    /// </summary>
    public Product GetProduct(string productId)
    {
        return this.controller.products.WithID(productId);
    }

    /// <summary>
    /// 購入
    /// </summary>
    public void Purchase(string productId, Action<string> onProcessPurchase, Action<PurchaseFailureReason> onPurchaseFailed)
    {
        this.onProcessPurchase = onProcessPurchase;
        this.onPurchaseFailed = onPurchaseFailed;

        if (this.pendingProducts.Count > 0)
        {
            this.onPurchaseFailed?.Invoke(PurchaseFailureReason.ExistingPurchasePending);
            return;
        }

        if (this.controller != null)
        {
            var product = this.controller.products.WithID(productId);
            if (product != null)
            {
                this.controller.InitiatePurchase(product);
                return;
            }
        }

        CoroutineUpdator.Create(null, () => this.onPurchaseFailed?.Invoke(PurchaseFailureReason.Unknown));
    }

    /// <summary>
    /// 購入成功時
    /// </summary>
    PurchaseProcessingResult IStoreListener.ProcessPurchase(PurchaseEventArgs e)
    {
        //ペンディング中商品としてリストに保持
        this.pendingProducts.Add(e.purchasedProduct);

        //購入成功通知
        this.onProcessPurchase?.Invoke(e.purchasedProduct.receipt);

        //サーバーで処理完了するまでペンディング
        return PurchaseProcessingResult.Pending;
    }

    /// <summary>
    /// 購入失敗時
    /// </summary>
    void IStoreListener.OnPurchaseFailed(Product i, PurchaseFailureReason p)
    {
        this.onPurchaseFailed?.Invoke(p);
    }

    /// <summary>
    /// サーバー処理後、ペンディング中商品を完了させる
    /// </summary>
    public void ConfirmPendingPurchase(string productId)
    {
        var product = this.pendingProducts.Find(x => x.definition.id == productId);
        if (product != null)
        {
            this.controller.ConfirmPendingPurchase(product);
            this.pendingProducts.Remove(product);
        }
    }

    /// <summary>
    /// ペンディング中商品の取得
    /// </summary>
    public Product[] GetPendingProducts()
    {
        return this.pendingProducts.ToArray();
    }
}

