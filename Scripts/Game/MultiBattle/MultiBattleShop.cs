using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// マルチバトル内簡易ショップ
/// </summary>
public partial class MultiBattleShop : DialogBase
{
    /// <summary>
    /// スクロールビュー
    /// </summary>
    [SerializeField]
    private InfiniteScrollView scrollView = null;
    /// <summary>
    /// 商品アイコンプレハブ
    /// </summary>
    [SerializeField]
    private ProductIcon productIconPrefab = null;
    /// <summary>
    /// 購入確認ダイアログ内容プレハブ
    /// </summary>
    [SerializeField]
    private ShopPurchaseConfirmDialogContent purchaseConfirmDialogContentPrefab = null;
    /// <summary>
    /// 購入完了ダイアログ内容プレハブ
    /// </summary>
    [SerializeField]
    private ShopPurchaseCompletedDialogContent purchaseCompletedDialogContentPrefab = null;
    /// <summary>
    /// タブグループ
    /// </summary>
    [SerializeField]
    private TabGroup tabGroup = null;

    /// <summary>
    /// 選択中ショップタブ
    /// </summary>
    private MultiBattleShopTab selectedTab = null;
    /// <summary>
    /// ローダー
    /// </summary>
    private AssetListLoader loader = null;

    /// <summary>
    /// ショップ開く
    /// </summary>
    public static void Open(MultiBattleShop prefab)
    {
        //通信で商品リスト取得
        BillingApi.CallListApi(() => {
        ShopApi.CallNowShopApi((tShops) =>
        {
            //ローダー準備
            var loader = new AssetListLoader(Masters.ShopDB
                .GetList()
                .Where(x => x.multiFlg > 0)
                .SelectMany(x1 => Masters.ShopItemDB.GetList().FindAll(x2 => x2.shopItemId == x1.shopItemId))
                .Select(x => CommonIconUtility.GetItemInfo(x.itemType, x.itemId))
                .Where(x => !x.IsCommonSprite())
                .Select(x => new AssetLoader<Sprite>(x.GetSpritePath())));

            //ロード中タッチブロック
            SharedUI.Instance.DisableTouch();

            //リソースロード
            loader.Load(() =>
            {
                //タッチブロック解除
                SharedUI.Instance.EnableTouch();

                //ショップダイアログ生成
                var dialog = SharedUI.Instance.ShowPopup(prefab);
                dialog.loader = loader;
                dialog.Setup(tShops);
            });
        });
        });
    }

    /// <summary>
    /// セットアップ
    /// </summary>
    public void Setup(List<UserShopData> tShops)
    {
        this.tabGroup.Setup();

        //各タブに対応する商品を詰める
        for (int i = 0; i < this.tabGroup.tabList.Count; i++)
        {
            var tab = tabGroup.tabList[i] as MultiBattleShopTab;

            if (tab.tabType == ShopScene.PageType.Gem)
            {
                //一旦ジェムは販売しない
                /*
                tab.products = Masters.BillingDB
                    .GetList()
                    .Where(x => x.platform == SharkDefine.DEVICE_TYPE)
                    .Select(x => ProductBase.Create(x, 0))
                    .Where(x => x != null)
                    .OrderBy(x => x.sort)
                    .OrderBy(x => x.isOverPurchaseLimit ? 1 : 0)
                    .ToArray();
                */
            }
            else
            {
                tab.products = tab.shopGroupTypes
                    .SelectMany(x1 => Masters.ShopDB.GetList().FindAll(x2 => x2.shopGroupId == (uint)x1))
                    .Where(x => x.multiFlg > 0)
                    .Select(x1 => ProductBase.Create(x1, (uint)tShops.Where(x2 => x2.Id == x1.id).Sum(x2 => x2.BuyNum)))
                    .Where(x => x != null)
                    .OrderBy(x => x.sort)
                    .OrderBy(x => x.isOverPurchaseLimit ? 1 : 0)
                    .ToArray();
            }

            //商品が存在するタブだけ表示
            tab.gameObject.SetActive(tab.products != null && tab.products.Length > 0);
        }
    }

    /// <summary>
    /// OnDestroy
    /// </summary>
    protected override void OnDestroy()
    {
        this.loader.Unload();
        this.loader.Clear();
        this.loader = null;
        base.OnDestroy();
    }

    /// <summary>
    /// Start
    /// </summary>
    protected override void Start()
    {
        base.Start();

        //デフォルトはコインタブ
        this.selectedTab = this.tabGroup.Find(x => (x as MultiBattleShopTab).tabType == ShopScene.PageType.Coin) as MultiBattleShopTab;

        if (this.selectedTab == null || this.selectedTab.products.Length == 0)
        {
            //コインタブが無かったりコイン商品が存在しなかったら、商品があるタブを選択
            this.selectedTab = this.tabGroup.Find(x => (x as MultiBattleShopTab).products.Length > 0) as MultiBattleShopTab;
        }

        if (this.selectedTab != null)
        {
            //初期タブを開く
            this.tabGroup.SetActiveTab(this.selectedTab);

            //スクロールビュー構築
            this.scrollView.Initialize(this.productIconPrefab.gameObject, this.selectedTab.products.Length, this.OnUpdateElement);
        }
    }

    /// <summary>
    /// タブクリック時
    /// </summary>
    public void OnClickTab(MultiBattleShopTab tab)
    {
        this.selectedTab = tab;

        //SE
        SoundManager.Instance.PlaySe(SeName.YES);

        //スクロールビュー構築
        this.scrollView.Initialize(this.productIconPrefab.gameObject, this.selectedTab.products.Length, this.OnUpdateElement);
    }

    /// <summary>
    /// スクロールビュー要素更新時
    /// </summary>
    private void OnUpdateElement(GameObject gobj, int index)
    {
        var icon = gobj.GetComponent<ProductIcon>();
        var product = this.selectedTab.products[index];

        //アイコン表示更新
        icon.BuildView(product);

        //アイコンクリック時処理
        icon.onClick = () => this.OnClickProductIcon(product, icon);
    }

    /// <summary>
    /// 商品アイコンクリック時
    /// </summary>
    private void OnClickProductIcon(ProductBase product, ProductIcon icon)
    {
        //購入確認ダイアログ開く
        var dialog = SharedUI.Instance.ShowSimpleDialog();
        var content = dialog.AddContent(this.purchaseConfirmDialogContentPrefab);
        content.Set(icon, product, dialog, this.OnPurchaseCompleted, null);
    }

    /// <summary>
    /// 商品購入完了時コールバック
    /// </summary>
    private void OnPurchaseCompleted(
        UserBillingData userBillingData,
        UserShopData userShopData,
        ProductIcon icon,
        ProductBase product)
    {
        //商品購入成功通知
        (SceneChanger.currentScene as Battle.MultiBattleScene)?.RefleshCoinCount();

        //購入回数更新
        if (userShopData != null)
        {
            product.buyCount = userShopData.BuyNum;
        }
        else if (userBillingData != null)
        {
            product.buyCount = userBillingData.BuyNum;
        }

        //購入上限に達した
        if (product.isOverPurchaseLimit)
        {
            //商品をソート
            this.selectedTab.products = this.selectedTab.products
                .OrderBy(x => x.isOverPurchaseLimit ? 1 : 0)
                .ToArray();

            //スクロールビュー更新
            this.scrollView.Initialize(this.productIconPrefab.gameObject, this.selectedTab.products.Length, this.OnUpdateElement);
        }
        else
        {
            //購入回数のみ更新
            icon.SetPurchasesCountText(product.remainCount);
        }

        //購入完了ダイアログ表示
        var dialog = SharedUI.Instance.ShowSimpleDialog();
        var content = dialog.AddContent(this.purchaseCompletedDialogContentPrefab);
        content.Set(product, dialog);
    }
}
