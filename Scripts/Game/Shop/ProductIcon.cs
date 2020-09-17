using UnityEngine;
using UnityEngine.UI;

public class ProductIcon : IconBase
{
    /// <summary>
    /// 商品アイコン
    /// </summary>
    [SerializeField]
    public CommonIcon commonIcon = null;
    /// <summary>
    /// 商品詳細テキスト
    /// </summary>
    [SerializeField]
    private Text productNameText = null;
    /// <summary>
    /// 支払い時のアイテムアイコンイメージ
    /// </summary>
    [SerializeField]
    private Image paymentItemIconImage = null;
    /// <summary>
    /// 値段テキスト
    /// </summary>
    [SerializeField]
    private Text priceText = null;
    /// <summary>
    /// 購入上限に達したアイテムの上に表示するイメージ
    /// </summary>
    [SerializeField]
    private Image overPurchaseLimitImage = null;
    /// <summary>
    /// 購入回数のテキスト
    /// </summary>
    [SerializeField]
    private Text purchasesCountText = null;
    /// <summary>
    /// おすすめの表示物
    /// </summary>
    [SerializeField]
    private GameObject recommendContent = null;
    /// <summary>
    /// おすすめテキスト
    /// </summary>
    [SerializeField]
    private Text recommendText = null;
    /// <summary>
    /// 詳細ダイアログデータ
    /// </summary>
    [SerializeField]
    private CommonItemInfoDialogData infoDialogData = null;

    /// <summary>
    /// 表示構築
    /// </summary>
    public void BuildView(ProductBase product)
    {
        this.Initialize();

        //長押し時コールバック
        this.onLongPress = () =>
        {
            this.infoDialogData.OpenDialog(product);
        };

        //CommonIcon表示構築
        this.commonIcon.Set(product.addItems[0].itemType, product.addItems[0].itemId, false);

        //CommonIconのフレームと背景表示切り替え
        this.commonIcon.SetFrameVisible(product.isVisibleProductIconFrame);

        //CommonIconのサイズ変更
        this.commonIcon.rectTransform.sizeDelta = product.iconSize;

        //CommonIconに対する追加処理
        product.SetCommonIcon(this.commonIcon);

        //商品名
        this.productNameText.text = product.productName;

        //値段テキスト
        string priceText = product.price.ToString("#,0");

        if (product is ProductBilling)
        {
            //一次通貨で購入する場合は￥マーク表示になるので、支払いアイコンは非表示
            this.paymentItemIconImage.gameObject.SetActive(false);

            //値段テキスト
            priceText = Masters.LocalizeTextDB.GetFormat("Price", priceText);
        }
        else
        {
            var payInfo = CommonIconUtility.GetItemInfo((uint)product.payType, 0);
            if (payInfo.IsCommonSprite())
            {
                //支払いアイコン設定
                this.paymentItemIconImage.gameObject.SetActive(true);
                this.paymentItemIconImage.sprite = SharedUI.Instance.commonAtlas.GetSprite(payInfo.GetSpritePath());
            }
        }

        //値段テキスト
        this.priceText.text = priceText;

        //残り購入回数表示
        this.SetPurchasesCountText(product.remainCount);

        //おすすめの設定 (α版では表示したくないとのこと)
        this.recommendContent.SetActive(false);//(product.Recommended > 0);
    }

    /// <summary>
    /// 表示物をnullで初期化
    /// </summary>
    private void Initialize()
    {
        this.commonIcon.SetIconSprite(null);
        this.productNameText.text = null;
        this.paymentItemIconImage.sprite = null;
        this.priceText.text = null;
        this.purchasesCountText.text = null;
        this.recommendText.text = null;
        base.onClick = null;
        base.onLongPress = null;
    }

    /// <summary>
    /// 購入回数の表示設定
    /// </summary>
    public void SetPurchasesCountText(uint? purchasesCount)
    {
        if (purchasesCount != null)
        {
            this.purchasesCountText.gameObject.SetActive(true);
            this.purchasesCountText.text = Masters.LocalizeTextDB.GetFormat("PurchasesCount", purchasesCount);
            if (purchasesCount > 0)
            {
                //購入上限に未到達
                this.overPurchaseLimitImage.gameObject.SetActive(false);
            }
            else
            {
                //購入上限に到達
                this.overPurchaseLimitImage.gameObject.SetActive(true);
            }
        }
        else
        {
            //無限購入
            this.purchasesCountText.gameObject.SetActive(false);
            this.overPurchaseLimitImage.gameObject.SetActive(false);
        }

        //タップ判定を設定
        base.button.interactable = !this.overPurchaseLimitImage.gameObject.activeSelf;
    }
}
