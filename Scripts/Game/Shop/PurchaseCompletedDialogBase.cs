using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public abstract class PurchaseCompletedDialogBase : MonoBehaviour
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
    private PurchaseCompletedCoinContent coinContent = null;
    /// <summary>
    /// ジェムのコンテンツ
    /// </summary>
    [SerializeField]
    private PurchaseCompletedGemContent gemContent = null;

    /// <summary>
    /// 商品情報
    /// </summary>
    protected ProductBase product { get; private set; }

    /// <summary>
    /// 内容構築
    /// </summary>
    public virtual void Set(ProductBase product, SimpleDialog dialog)
    {
        this.product = product;

        //メッセージ設定
        this.SetMessage();

        //コインコンテンツ設定
        this.SetCoinContent();

        //ジェムコンテンツ設定
        this.SetGemContent();

        //ボタンの設定
        dialog.AddOKButton().onClick = dialog.Close;
    }

    /// <summary>
    /// メッセージ設定
    /// </summary>
    private void SetMessage()
    {
        //〇〇を購入しました
        this.messageText.text = Masters.LocalizeTextDB.GetFormat("BoughtProduct", this.product.productName);
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

        //コイン数表示
        this.coinContent.AfterCoinText = UserData.Get().GetCurrentCoin().ToString("#,0");
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

        //ジェム数表示
        this.gemContent.AfterTotalGemText = UserData.Get().totalGem.ToString("#,0");
        this.gemContent.AfterChargeGemText = UserData.Get().chargeGem.ToString("#,0");
    }
}
