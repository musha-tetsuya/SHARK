using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

/// <summary>
/// ショップシーン
/// </summary>
public class ShopScene : SceneBase
{
    //ページ(タブ)の種類
    public enum PageType
    {
        None = 0,
        Recommend = 1,      //お勧め   (Main)
        Coin = 2,           //コイン   (Main)
        Gem = 3,            //ジェム   (Main)
        ToolGroup = 4,      //道具     (Main)
        ToolExchange = 5,   //道具交換 (Sub)
        GearExchange = 6,   //ギア交換 (Sub)
        CannonExchange = 7, //砲台交換 (Sub)
    }

    /// <summary>
    /// ショップグループ
    /// </summary>
    public enum ShopGroupType
    {
        Coin = 1,
        Item = 2,
        Gear = 3,
        Cannon = 4,
    }

    /// <summary>
    /// ページ情報
    /// </summary>
    public class PageData
    {
        public PageType pageType;
        public string title;
        public ProductBase[] products;
    }

    /// <summary>
    /// 商品アイコンプレハブ
    /// </summary>
    [SerializeField]
    private ProductIcon productIconPrefab = null;
    /// <summary>
    /// サブページのタブが格納されている親のオブジェクト
    /// </summary>
    [SerializeField]
    private GameObject subToggleGroup = null;
    /// <summary>
    /// Vipダイアログプレハブ
    /// </summary>
    [SerializeField]
    private VipInfoDialog vipInfoDialog = null;
    /// <summary>
    /// VIP機能関連のコンテンツが格納されている親のオブジェクト
    /// </summary>
    [SerializeField]
    private VipExpGauge vipExpGauge = null;
    /// <summary>
    /// 商品一覧のタイトル
    /// </summary>
    [SerializeField]
    private Text pageTitleText = null;
    /// <summary>
    /// 商品スクロールビュー
    /// </summary>
    [SerializeField]
    private InfiniteScrollView productScrollView = null;
    /// <summary>
    /// 砲台購入画面の砲台ビューワープレハブ
    /// </summary>
    [SerializeField]
    private TurretViewer cannonPurchaseTurretViewerPrefab = null;
    /// <summary>
    /// 商品購入確認ダイアログ内容プレハブ
    /// </summary>
    [SerializeField]
    private ShopPurchaseConfirmDialogContent confirmDialogContent = null;
    /// <summary>
    /// 砲台購入確認ダイアログ内容プレハブ
    /// </summary>
    [SerializeField]
    private ShopCannonPurchaseConfirmDialogContent confirmCannonDialogContent = null;
    /// <summary>
    /// 商品購入完了ダイアログ内容プレハブ
    /// </summary>
    [SerializeField]
    private ShopPurchaseCompletedDialogContent completedDialogContent = null;
    /// <summary>
    /// 砲台購入完了ダイアログ内容プレハブ
    /// </summary>
    [SerializeField]
    private ShopCannonPurchaseCompletedDialogContent completedCannonDialogContent = null;
    /// <summary>
    /// ペンディング数通知バッジ
    /// </summary>
    [SerializeField]
    private GameObject pendingCountBadge = null;
    /// <summary>
    /// ペンディング中商品数テキスト
    /// </summary>
    [SerializeField]
    private Text pendingCountText = null;
    /// <summary>
    /// メインタブグループ
    /// </summary>
    [SerializeField]
    private TabGroup mainTabGroup = null;
    /// <summary>
    /// サブタブグループ
    /// </summary>
    [SerializeField]
    private TabGroup subTabGroup = null;
    /// <summary>
    /// VipレベルアップエフェクトPrefab
    /// </summary>
    [SerializeField]
    private UIVipLevelUp vipLevelUpPrefab = null;
    /// <summary>
    /// Coin Icon Path
    /// </summary>
    private string[] coinIconpath  = null;
    /// <summary>
    /// Gem Icon Path
    /// </summary>
    private string[] gemIconPath = null;
    /// <summary>
    /// コインID
    /// </summary>
    private List<uint> coinId = null;
    /// <summary>
    /// ジェムID
    /// </summary>
    private List<uint> gemId = null;
    /// <summary>
    /// アプリ内課金
    /// </summary>
    public SharkIAPController iap = new SharkIAPController();
    /// <summary>
    /// アセットローダー
    /// </summary>
    private AssetListLoader assetLoader = new AssetListLoader();

    /// <summary>
    /// 他のシーンから遷移した際に最初に開かれるページ
    /// </summary>
    private PageType firstOpenPage = PageType.Recommend;
    /// <summary>
    /// 現在開いている商品グループのページ
    /// </summary>
    private PageType nowPage = PageType.None;
    /// <summary>
    /// 現在開いているサブタブの種類
    /// </summary>
    private PageType subTabType = PageType.GearExchange;
    /// <summary>
    /// ページ情報リスト
    /// </summary>
    private Dictionary<int, PageData> pageDataList = new Dictionary<int, PageData>();

    /// <summary>
    /// OnDestroy
    /// </summary>
    private void OnDestroy()
    {
        this.assetLoader.Unload();
    }

    /// <summary>
    /// Awake
    /// </summary>
    protected override void Awake()
    {
        base.Awake();

        //シーン切り替えアニメーションは手動で消す
        SceneChanger.IsAutoHideLoading = false;
        
        // Coin Icon Path 修得
        this.coinIconpath = Enum.GetNames(typeof(ProductBase.CoinSizeType));
        // Gem Icon Path 修得
        this.gemIconPath = Enum.GetNames(typeof(ProductBase.GemSizeType));
    }

    /// <summary>
    /// シーンロード完了時
    /// </summary>
    public override void OnSceneLoaded(SceneDataPackBase dataPack)
    {
        if (dataPack is ToShopSceneDataPack)
        {
            var data = dataPack as ToShopSceneDataPack;
            this.firstOpenPage = data.pageType;
        }
    }

    /// <summary>
    /// ヘッダーのコインボタンクリック時
    /// </summary>
    public override void OnClickCoinButton()
    {
        OpenMainTab(PageType.Coin);
    }

    /// <summary>
    /// ヘッダーのジェムボタンクリック時
    /// </summary>
    public override void OnClickGemButton()
    {
        OpenMainTab(PageType.Gem);
    }

    /// <summary>
    /// Start
    /// </summary>
    private void Start()
    {
        Queue<Action> queue = new Queue<Action>();

        //通信で課金商品情報を取得
        queue.Enqueue(() => BillingApi.CallListApi(queue.Dequeue()));

        //IAP初期化
        queue.Enqueue(() =>
        {
            this.iap.Initialize(
                onSuccess: () =>
                {
                    //次の処理へ
                    queue.Dequeue().Invoke();
                },
                onFailed: () =>
                {
                    //TODO:初期化エラー。ダイアログ出してHOMEにでも戻すか。
                    queue.Dequeue().Invoke();
                }
            );
        });

        //通信で商品情報を取得
        queue.Enqueue(() =>
        {
            ShopApi.CallNowShopApi((tShops) =>
            {
                //ショップページ情報初期化
                this.InitPageData(tShops);

                //次の処理へ
                queue.Dequeue().Invoke();
            });
        });

        //リソースの読み込み
        queue.Enqueue(() =>
        {
            // Coin id 修得
            this.coinId = Masters.ShopItemDB.GetList()
                .Where(x => x.itemType == (uint)ItemType.Coin)
                .Select(x => x.id)
                .ToList();

            // Gem id 修得
            this.gemId = Masters.BillingDB.GetList()
                .Select(x => x.id)
                .ToList();

            //ペンディング数通知バッジ
            int pendingProductsCount = this.iap.GetPengingProductsCount();
            this.pendingCountBadge.SetActive(pendingProductsCount > 0);
            this.pendingCountText.text = pendingProductsCount.ToString();

            //タブセットアップ
            this.mainTabGroup.Setup();
            this.subTabGroup.Setup();

            //おすすめ商品が無い場合
            if (this.pageDataList[(int)PageType.Recommend].products.Length == 0)
            {
                //おすすめタブをグレーアウト
                var recommendTab = this.mainTabGroup.Find(x => (x as ShopMainTab).tabType == PageType.Recommend);
                recommendTab.SetGrayout(true);

                if (this.firstOpenPage == PageType.Recommend)
                {
                    //初期ページとしておすすめは開けないのでコインに変更
                    this.firstOpenPage = PageType.Coin;
                }
            }

            //リソースロード
            this.Load(queue.Dequeue());
        });

        //リソースロード完了時
        queue.Enqueue(() =>
        {
            //ローディング表示消す
            SharedUI.Instance.HideSceneChangeAnimation();
            //BGM再生
            SoundManager.Instance.PlayBgm(BgmName.HOME);
            //タブ開く
            this.OpenMainTab(this.firstOpenPage);
        });

        queue.Dequeue().Invoke();
    }

    /// <summary>
    /// 必要リソースの読み込み
    /// </summary>
    private void Load(Action onCompleted)
    {
        var items = Masters.ShopGroupDB
            .GetList()
            .SelectMany(x1 => Masters.ShopDB.GetList().FindAll(x2 => x2.shopGroupId == x1.id))
            .SelectMany(x1 => Masters.ShopItemDB.GetList().FindAll(x2 => x2.shopItemId == x1.shopItemId))
            .Select(x => CommonIconUtility.GetItemInfo(x.itemType, x.itemId))
            .Concat(Masters.BillingGroupDB
                .GetList()
                .SelectMany(x1 => Masters.BillingDB.GetList().FindAll(x2 => x2.billingGroupId == x1.id))
                .SelectMany(x1 => Masters.BillingItemDB.GetList().FindAll(x2 => x2.billingItemId == x1.billingItemId))
                .Select(x => CommonIconUtility.GetItemInfo(x.itemType, x.itemId)))
            .Where(x => !x.IsCommonSprite())
            .ToArray();

        foreach (var item in items)
        {
            if (item is Master.CannonSetData)
            {
                var cannonSet = item as Master.CannonSetData;
                var batteryData = Masters.BatteryDB.FindById(cannonSet.batteryId);
                var barrelData = Masters.BarrelDB.FindById(cannonSet.barrelId);
                var bulletData = Masters.BulletDB.FindById(cannonSet.bulletId);
                var fvaData = Masters.FvAttackDB.FindById(batteryData.fvAttackId);

                //砲台サムネロード
                this.assetLoader.Add<Sprite>(SharkDefine.GetTurretSetSpritePath(batteryData.key));

                //砲台パーツスプライトロード
                this.assetLoader.Add<Sprite>(SharkDefine.GetBatterySpritePath(batteryData.key));
                this.assetLoader.Add<Sprite>(SharkDefine.GetBarrelSpritePath(barrelData.key));
                this.assetLoader.Add<Sprite>(SharkDefine.GetBulletThumbnailPath(bulletData.key));

                //砲台パーツプレハブロード
                this.assetLoader.Add<GameObject>(SharkDefine.GetBatteryPrefabPath(batteryData.key));
                this.assetLoader.Add<GameObject>(SharkDefine.GetBarrelPrefabPath(barrelData.key));
                this.assetLoader.Add<BulletBase>(SharkDefine.GetBulletPrefabPath(bulletData.key));

                //FVAアイコンスプライトロード
                this.assetLoader.Add<Sprite>(SharkDefine.GetFvAttackTypeIconSpritePath((FvAttackType)fvaData.type));

                //シリーズスキルスプライトロード
                if (batteryData.seriesId == barrelData.seriesId && batteryData.seriesId == bulletData.seriesId)
                {
                    var serieseData = Masters.TurretSerieseDB.FindById(batteryData.seriesId);
                    var serieseSkillData = Masters.SerieseSkillDB.FindById(serieseData.seriesSkillId);
                    this.assetLoader.Add<Sprite>(SharkDefine.GetSeriesSkillIconSpritePath(serieseSkillData.key));
                }
            }
            else
            {
                var spritePath = item.GetSpritePath();
                this.assetLoader.Add<Sprite>(spritePath);
            }
        }

        //読み込み開始
        this.assetLoader.Load(onCompleted);
    }

    /// <summary>
    /// 商品ページ情報初期化
    /// </summary>
    private void InitPageData(List<UserShopData> tShops)
    {
        this.pageDataList.Clear();

        //おすすめページのデータセット
        this.pageDataList[(int)PageType.Recommend] = new PageData
        {
            pageType = PageType.Recommend,
            title = Masters.LocalizeTextDB.Get("RecommendPageTitle"),
            products = Masters.ShopDB
                .GetList()
                .Where(x => x.recommended > 0)
                .Select(x1 => ProductBase.Create(x1, (uint)tShops.Where(x2 => x2.Id == x1.id).Sum(x2 => x2.BuyNum)))
                .Concat(Masters.BillingDB
                    .GetList()
                    .Where(x => x.recommended > 0)
                    .Select(x => ProductBase.Create(x, 0)))
                .Where(x => x != null)
                .OrderBy(x => x.sort)
                .OrderBy(x => x.isOverPurchaseLimit ? 1 : 0)
                .ToArray()
        };

        //ジェムページのデータセット
        this.pageDataList[(int)PageType.Gem] = new PageData
        {
            pageType = PageType.Gem,
            title = Masters.BillingGroupDB.FindById(SharkDefine.DEVICE_TYPE).name,
            products = Masters.BillingDB
                .GetList()
                .Select(x => ProductBase.Create(x, 0))
                .Where(x => x != null)
                .OrderBy(x => x.sort)
                .OrderBy(x => x.isOverPurchaseLimit ? 1 : 0)
                .ToArray()
        };

        //コイン、道具交換、ギア交換、砲台交換ページのデータセット
        (int pageType, uint shopGroupId)[] pageDatas =
        {
            ((int)PageType.Coin, (int)ShopGroupType.Coin),
            ((int)PageType.ToolExchange, (int)ShopGroupType.Item),
            ((int)PageType.GearExchange, (int)ShopGroupType.Gear),
            ((int)PageType.CannonExchange, (int)ShopGroupType.Cannon),
        };

        foreach (var pageData in pageDatas)
        {
            this.pageDataList[pageData.pageType] = new PageData
            {
                pageType = (PageType)pageData.pageType,
                title = Masters.ShopGroupDB.FindById(pageData.shopGroupId).name,
                products = Masters.ShopDB
                    .GetList()
                    .Where(x => x.shopGroupId == pageData.shopGroupId)
                    .Select(x1 => ProductBase.Create(x1, (uint)tShops.Where(x2 => x2.Id == x1.id).Sum(x2 => x2.BuyNum)))
                    .Where(x => x != null)
                    .OrderBy(x => x.sort)
                    .OrderBy(x => x.isOverPurchaseLimit ? 1 : 0)
                    .ToArray()
            };
        }
        // 経験値ゲージエリアセット
        SetVipExpGauge();
    }

    /// <summary>
    /// VIPエリアセット
    /// </summary>
    private void SetVipExpGauge()
    {
        var userData = UserData.Get();
        List<Master.VipLevelData> vipLevels = Masters.VipLevelDB.GetList();
        uint nextExp = vipLevels.Where(x => x.vipLevel == userData.vipLevel).Select(x => x.nextExp).FirstOrDefault();
        this.vipExpGauge.SetRank(userData.vipLevel);
        this.vipExpGauge.SetExp(userData.vipExp, nextExp);
    }

    /// <summary>
    /// Vipボタンクリック時
    /// </summary>
    public void OnClickVipButton()
    {
        SoundManager.Instance.PlaySe(SeName.YES);
        VipInfoDialog.Open(this.vipInfoDialog);
    }

    /// <summary>
    /// コード上からメインタブを開く
    /// </summary>
    private void OpenMainTab(PageType pageType)
    {
        //選択されたメインタブのアクティブをON。それ以外はOFF
        this.mainTabGroup.SetActiveTab(this.mainTabGroup.Find(x => (x as ShopMainTab).tabType == pageType));
        //選択されたショップページを開く
        this.SetTabContent(pageType);
    }

    /// <summary>
    /// コード上からサブタブを開く
    /// </summary>
    private void OpenSubTab(PageType pageType)
    {
        //選択されたサブタブのアクティブをON。それ以外はOFF
        this.subTabGroup.SetActiveTab(this.subTabGroup.Find(x => (x as ShopSubTab).tabType == pageType));
        //選択されたショップページを開く
        this.SetTabContent(pageType);
    }

    /// <summary>
    /// メインタブ押下時
    /// </summary>
    public void OnClickMainTab(ShopMainTab tab)
    {
        this.SetTabContent(tab.tabType);
    }

    /// <summary>
    /// サブタブ押下時
    /// </summary>
    public void OnClickSubTab(ShopSubTab tab)
    {
        this.subTabType = tab.tabType;
        this.SetTabContent(tab.tabType);
    }

    /// <summary>
    /// ページ毎のコンテンツを設定
    /// </summary>
    private void SetTabContent(PageType nextPage)
    {
        if (nextPage == PageType.None || nextPage == this.nowPage)
        {
            Debug.LogWarningFormat("開けない:{0}", nextPage);
            return;
        }

        if (nextPage == PageType.ToolGroup)
        {
            //サブタブページを開く
            this.OpenSubTab(this.subTabType);
            return;
        }

        if (this.nowPage != PageType.None)
        {
            //タブ切り替え時のSE
            SoundManager.Instance.PlaySe(SeName.YES);
        }

        this.nowPage = nextPage;

        //ページタイトル
        this.pageTitleText.text = this.pageDataList[(int)this.nowPage].title;

        //「サブページ」と「VIPコンテンツ」の表示/非表示を切り替え
        this.vipExpGauge.gameObject.SetActive(this.nowPage < PageType.ToolGroup);
        this.subToggleGroup.SetActive(this.nowPage > PageType.ToolGroup);

        //スクロールビューに商品リストセット
        this.productScrollView.Initialize(this.productIconPrefab.gameObject, this.pageDataList[(int)nowPage].products.Length, this.OnUpdateProduct);
    }

    /// <summary>
    /// コイン・ジェムのサイズ別に、スプライトセット
    /// </summary>
    private void SetCoinGemSprite(List<uint> coinGemList, uint productId, Sprite productIconSprite, string[] path, ProductIcon icon)
    {
        var min = coinGemList.Min();
        var max = coinGemList.Max();
        var average = (coinGemList.Count/2) + min - 1;

        if (productId == min)
            productIconSprite = this.sceneAtlas.GetSprite(path[0]);
        else if (productId == max)
            productIconSprite = this.sceneAtlas.GetSprite(path[3]);
        else if (productId > min && productId <= average)
            productIconSprite = this.sceneAtlas.GetSprite(path[1]);
        else
            productIconSprite = this.sceneAtlas.GetSprite(path[2]);

        // スプライトセット
        icon.commonIcon.iconImage.sprite = productIconSprite;
    }

    /// <summary>
    /// 商品の更新
    /// </summary>
    private void OnUpdateProduct(GameObject gobj, int index)
    {
        var icon = gobj.GetComponent<ProductIcon>();

        //商品情報
        var product = this.pageDataList[(int)this.nowPage].products[index];

        //商品アイコン見た目構築
        icon.BuildView(product);

        //商品アイコンのコールバックを設定
        icon.onClick = () =>
        {
            OnPurchaseConfirm(icon, product);

            SoundManager.Instance.PlaySe(SeName.YES);
        };
    }

    /// <summary>
    /// 商品の購入確認時のコールバック
    /// </summary>
    private void OnPurchaseConfirm(ProductIcon icon, ProductBase product)
    {
        //ダイアログ生成
        var dialog = SharedUI.Instance.ShowSimpleDialog();
        if (product is ProductCannon)
        {
            var content = dialog.AddContent(this.confirmCannonDialogContent);
            content.Set(this.cannonPurchaseTurretViewerPrefab, icon, product, dialog, this.OnPurchaseCompleted, () => this.OnNonPurchase(product));
        }
        else
        {
            var content = dialog.AddContent(this.confirmDialogContent);
            content.Set(icon, product, dialog, this.OnPurchaseCompleted, () => this.OnNonPurchase(product));
        }
    }

    /// <summary>
    /// 商品の購入完了時のコールバック
    /// </summary>
    private void OnPurchaseCompleted(UserBillingData userBilling, UserShopData userShop, ProductIcon icon, ProductBase product)
    {
        //購入回数更新
        if (userShop != null)
        {
            product.buyCount = userShop.BuyNum;
        }
        else if (userBilling != null)
        {
            product.buyCount = userBilling.BuyNum;
        }

        //購入した商品が購入上限に達しているか?
        if (product.isOverPurchaseLimit)
        {
            //商品をソート
            this.pageDataList[(int)this.nowPage].products = this.pageDataList[(int)this.nowPage].products
                .OrderBy(x => x.isOverPurchaseLimit ? 1 : 0)
                .ToArray();

            //商品の並びが変わったため、全更新をかける
            this.productScrollView.Initialize(this.productIconPrefab.gameObject, this.pageDataList[(int)this.nowPage].products.Length, this.OnUpdateProduct);
        }
        else
        {
            //対象商品の購入回数のみ更新
            icon.SetPurchasesCountText(product.remainCount);
        }

        //ヘッダー情報の更新
        SharedUI.Instance.header.SetInfo(UserData.Get());
        //vipエリア更新
        this.SetVipExpGauge();

        //ダイアログ生成
        var dialog = SharedUI.Instance.ShowSimpleDialog();
        if (product is ProductCannon)
        {
            var content = dialog.AddContent(this.completedCannonDialogContent);
            content.Set(this.cannonPurchaseTurretViewerPrefab, product, dialog);
        }
        else
        {
            var content = dialog.AddContent(this.completedDialogContent);
            content.Set(product, dialog);

            // // Vip Level Popup
            UIVipLevelUp.OpenIfNeed(this.vipLevelUpPrefab);
        }
    }

    /// <summary>
    /// 商品のお金足りない時のコールバック
    /// </summary>
    private void OnNonPurchase(ProductBase product)
    {
        switch (product.payType)
        {
            case ItemType.ChargeGem:
            case ItemType.FreeGem:
            this.OpenMainTab(PageType.Gem);
            break;

            case ItemType.Coin:
            this.OpenMainTab(PageType.Coin);
            break;
        }
    }

    /// <summary>
    /// ペンディング解決ボタン押下時
    /// </summary>
    public void OnClickResolvePendingButton()
    {
        this.iap.ResolvePending(() =>
        {
            //ジェム表示更新
            SharedUI.Instance.header.SetInfo(UserData.Get());
            //ペンディング数通知バッジ
            int pendingProductsCount = this.iap.GetPengingProductsCount();
            this.pendingCountBadge.SetActive(pendingProductsCount > 0);
            this.pendingCountText.text = pendingProductsCount.ToString();
        });
    }

    /// <summary>
    /// 特商法ボタン押下時
    /// </summary>
    public void OnClickCommercialLawButton()
    {
        //TODO:特商法ページの用意
        Application.OpenURL("http://dev-fish-1.sunchoi.co.jp/notice");
    }

    /// <summary>
    /// 資金決済法ボタン押下時
    /// </summary>
    public void OnClickFundSettlementButton()
    {
        //TODO:資金決済法ページの用意
        Application.OpenURL("http://dev-fish-1.sunchoi.co.jp/notice");
    }
}
