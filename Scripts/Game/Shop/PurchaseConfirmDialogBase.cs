using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public abstract class PurchaseConfirmDialogBase : MonoBehaviour
{
    /// <summary>
    /// 購入に対する確認の文言テキスト
    /// </summary>
    [SerializeField]
    private Text messageText = null;
    /// <summary>
    /// コインのコンテンツ
    /// </summary>
    [SerializeField]
    private PurchaseConfirmCoinContent coinContent = null;
    /// <summary>
    /// ジェムのコンテンツ
    /// </summary>
    [SerializeField]
    private PurchaseConfirmGemContent gemContent = null;
    /// <summary>
    /// 資金不足時の警告テキスト
    /// </summary>
    [SerializeField]
    private Text paymentAlertText = null;

    /// <summary>
    /// 自身のダイアログ
    /// </summary>
    private SimpleDialog dialog = null;
    /// <summary>
    /// 商品アイコン
    /// </summary>
    private ProductIcon icon = null;
    /// <summary>
    /// 商品情報
    /// </summary>
    protected ProductBase product = null;
    /// <summary>
    /// 購入数
    /// </summary>
    private uint buyNum = 0;
    /// <summary>
    /// 購入の通信が完了した際に呼ばれるコールバック
    /// </summary>
    private Action<UserBillingData, UserShopData, ProductIcon, ProductBase> onPurchaseCompleted = null;
    /// <summary>
    /// 所持金が足りず購入出来ない際に呼ばれるコールバック
    /// </summary>
    private Action onNonPurchase = null;

    /// <summary>
    /// 内容構築
    /// </summary>
    public virtual void Set(ProductIcon icon, ProductBase product, SimpleDialog dialog, Action<UserBillingData, UserShopData, ProductIcon, ProductBase> onPurchaseCompleted, Action onNonPurchase)
    {
        this.icon = icon;
        this.product = product;
        this.dialog = dialog;
        //現状では購入数の指定を行う仕様がないため、定数で設定
        this.buyNum = 1;

        this.onPurchaseCompleted = onPurchaseCompleted;
        this.onNonPurchase = onNonPurchase;

        //確認メッセージ設定
        this.SetConfirmMessage();

        //コインコンテンツ設定
        this.SetCoinContent();

        //ジェムコンテンツ設定
        this.SetGemContent();

        if (this.product is ProductBilling)
        {
            //購入ボタンを表示
            this.PurchaseProduct();
        }
    }

    /// <summary>
    /// 確認メッセージ設定
    /// </summary>
    private void SetConfirmMessage()
    {
        //一次通貨による商品の場合
        if (this.product is ProductBilling)
        {
            //{0}を購入しますか？
            this.messageText.text = Masters.LocalizeTextDB.GetFormat("BuyToMoney", this.product.productName);
        }
        //二次通貨による商品の場合
        else
        {
            switch (this.product.payType)
            {
                //ジェムによる支払い
                case ItemType.ChargeGem:
                case ItemType.FreeGem:
                {
                    //{0}ジェムで{1}を購入しますか？
                    this.messageText.text = Masters.LocalizeTextDB.GetFormat(
                        "BuyToSecondaryCurrency",
                        Masters.LocalizeTextDB.GetFormat("UnitGem", this.product.price),
                        this.product.productName
                    );
                }
                break;

                //コインによる支払い
                case ItemType.Coin:
                {
                    //{0}コインで{1}を購入しますか？
                    this.messageText.text = Masters.LocalizeTextDB.GetFormat(
                        "BuyToSecondaryCurrency",
                        Masters.LocalizeTextDB.GetFormat("UnitCoin", this.product.price),
                        this.product.productName
                    );
                }
                break;
            }
        }
    }

    /// <summary>
    /// コインコンテンツ設定
    /// </summary>
    private void SetCoinContent()
    {
        //コイン商品かどうか
        bool isAddCoin = this.product.addItems.Any(x => x.itemType == (uint)ItemType.Coin);

        //支払い方法がコインかどうか
        bool isPayCoin = (this.product.payType == ItemType.Coin);

        //どちらでもないなら、表示OFFにしてreturn
        if (!isAddCoin && !isPayCoin)
        {
            this.coinContent.gameObject.SetActive(false);
            return;
        }

        long beforeCoin = UserData.Get().GetCurrentCoin();

        //変化前のコイン数表示
        this.coinContent.BeforeCoinText = beforeCoin.ToString("#,0");

        if (isAddCoin)
        {
            //増加コイン数
            var addCoin = this.product.addItems
                .Where(x => x.itemType == (uint)ItemType.Coin)
                .Sum(x => x.itemNum);

            //増加後コイン数表示
            this.coinContent.AfterCoinText = (beforeCoin + addCoin).ToString("#,0");
        }
        else
        {
            //支払後コイン数
            var afterCoin = beforeCoin - (long)this.product.price;
            
            //支払後コイン数表示
            this.coinContent.AfterCoinText = UIUtility.GetColorText(
                (afterCoin >= 0) ? TextColorType.None : TextColorType.DecreaseParam,
                afterCoin.ToString("#,0")
            );

            if (afterCoin >= 0)
            {
                //購入ボタンを表示
                this.PurchaseProduct();
            }
            else
            {
                //アラートと戻るボタンを表示
                this.UnpurchaseProduct(Masters.LocalizeTextDB.Get("Coin"));
            }
        }
    }

    /// <summary>
    /// ジェムコンテンツ設定
    /// </summary>
    private void SetGemContent()
    {
        //ジェム商品かどうか
        bool isAddGem = this.product.addItems.Any(x => x.itemType == (uint)ItemType.ChargeGem || x.itemType == (uint)ItemType.FreeGem);

        //支払い方法がジェムかどうか
        bool isPayGem = (this.product.payType == ItemType.ChargeGem || this.product.payType == ItemType.FreeGem);

        //どちらでもないなら、表示OFFにしてreturn
        if (!isAddGem && !isPayGem)
        {
            this.gemContent.gameObject.SetActive(false);
            return;
        }

        var beforeChargeGem = UserData.Get().chargeGem;
        var beforeFreeGem = UserData.Get().freeGem;
        var beforeTotalGem = beforeChargeGem + beforeFreeGem;

        //変化前のジェム数表示
        this.gemContent.BeforeTotalGemText = beforeTotalGem.ToString("#,0");
        this.gemContent.BeforeChargeGemText = beforeChargeGem.ToString("#,0");

        if (isAddGem)
        {
            //増加ジェム数
            var addChargeGem = this.product.addItems.Where(x => x.itemType == (uint)ItemType.ChargeGem).Sum(x => x.itemNum);
            var addFreeGem = this.product.addItems.Where(x => x.itemType == (uint)ItemType.FreeGem).Sum(x => x.itemNum);

            //増加後ジェム数表示
            this.gemContent.AfterTotalGemText = ((long)beforeTotalGem + addChargeGem + addFreeGem).ToString("#,0");
            this.gemContent.AfterChargeGemText = ((long)beforeChargeGem + addChargeGem).ToString("#,0");
        }
        else
        {
            //支払後ジェム数
            var afterTotalGem = beforeTotalGem - this.product.price;
            var afterChargeGem = beforeChargeGem;

            if (this.product.payType == ItemType.ChargeGem)
            {
                //そのまま有償ジェムから引く
                afterChargeGem -= this.product.price;
            }
            else if (beforeFreeGem < this.product.price)
            {
                //無償ジェムで足りない分を引く
                afterChargeGem -= (this.product.price - beforeFreeGem);
            }
            
            //支払後総ジェム数表示
            this.gemContent.AfterTotalGemText = UIUtility.GetColorText(
                (afterTotalGem >= 0) ? TextColorType.None : TextColorType.DecreaseParam,
                afterTotalGem.ToString("#,0")
            );

            //支払い後有償ジェム数表示
            this.gemContent.AfterChargeGemText = UIUtility.GetColorText(
                (afterChargeGem >= 0) ? TextColorType.None : TextColorType.DecreaseParam,
                afterChargeGem.ToString("#, 0")
            );

            if (afterTotalGem >= 0 && afterChargeGem >= 0)
            {
                //購入ボタンを表示
                this.PurchaseProduct();
            }
            else
            {
                //アラートと戻るボタンを表示
                this.UnpurchaseProduct(Masters.LocalizeTextDB.Get("Gem"));
            }
        }
    }

    /// <summary>
    /// 商品が購入できる場合の処理
    /// </summary>
    private void PurchaseProduct()
    {
        //支払が可能な場合は null を代入
        this.paymentAlertText.text = null;

        //YesNoボタン追加：決定、キャンセル
        var yesNo = this.dialog.AddYesNoButton();
        yesNo.yes.text.text = Masters.LocalizeTextDB.Get("Buy");
        yesNo.yes.onClick = this.OnClickDecideButton;
        yesNo.no.text.text = Masters.LocalizeTextDB.Get("Back");
        yesNo.no.onClick = this.OnClickCancelButton;
    }

    /// <summary>
    /// 商品が購入できない場合の処理
    /// </summary>
    private void UnpurchaseProduct(string paymentItemName)
    {
        //支払アイテムの種類に応じて不足しているアイテムの警告文を表示
        this.paymentAlertText.text = UIUtility.GetColorText(
            TextColorType.Alert,
            Masters.LocalizeTextDB.GetFormat("PaymentAlert", paymentItemName)
        );

        //戻るボタン追加
        var buttonGroup = this.dialog.AddButton(1);
        buttonGroup.buttons[0].text.text = Masters.LocalizeTextDB.Get("Back");
        buttonGroup.buttons[0].onClick = this.OnClickBackButton;
    }


    /// <summary>
    /// ダイアログを閉じる
    /// </summary>
    protected virtual void CloseDialog()
    {
        this.dialog.Close();
    }


    /// <summary>
    /// 決定ボタンクリック時
    /// </summary>
    private void OnClickDecideButton()
    {
        if (!this.dialog.isClose)
        {
            if (this.product is ProductBilling)
            {
                var iap = (SceneChanger.currentScene as ShopScene).iap;

                //処理完了までタッチブロック
                SharedUI.Instance.DisableTouch();

                //一次通貨による商品の購入を行う
                iap.Purchase(
                    productId: Masters.BillingDB.FindById(this.product.master.id).productId,
                    onSuccess: (userBillingData) =>
                    {
                        //タッチブロック解除
                        SharedUI.Instance.EnableTouch();

                        this.CloseDialog();
                        this.onPurchaseCompleted?.Invoke(userBillingData, null, this.icon, this.product);
                    },
                    onFailed: () =>
                    {
                        //タッチブロック解除
                        SharedUI.Instance.EnableTouch();
                    });
            }
            else if (SceneChanger.currentScene is Battle.MultiBattleScene)
            {
                //バトル中簡易ショップの場合の購入通信
                MultiPlayApi.CallItemBuyApi(
                    this.product.master.id,
                    this.buyNum,
                    (SceneChanger.currentScene as Battle.MultiBattleScene).logData,
                    (userShop) =>
                    {
                        this.CloseDialog();
                        this.onPurchaseCompleted?.Invoke(null, userShop, this.icon, this.product);
                    });
            }
            else
            {
                //ジェム・コインなどの二次通貨による商品の購入を行う
                ShopApi.CallBuyApi(
                    this.product.master.id,
                    this.buyNum,
                    (userShop) =>
                    {
                        CloseDialog();
                        this.onPurchaseCompleted?.Invoke(null, userShop, this.icon, this.product);
                    });
            }

            SoundManager.Instance.PlaySe(SeName.YES);
        }
    }

    /// <summary>
    /// キャンセルボタンクリック時
    /// </summary>
    private void OnClickCancelButton()
    {
        if (!this.dialog.isClose)
        {
            CloseDialog();

            SoundManager.Instance.PlaySe(SeName.NO);
        }
    }

    /// <summary>
    /// 戻るボタンクリック時
    /// </summary>
    private void OnClickBackButton()
    {
        if (!this.dialog.isClose)
        {
            CloseDialog();
            this.onNonPurchase?.Invoke();

            SoundManager.Instance.PlaySe(SeName.NO);
        }
    }
}
