using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// プレゼントBOXダイアログ内容
/// </summary>
public class PresentBoxDialogContent : MonoBehaviour
{
    /// <summary>
    /// タブグループ
    /// </summary>
    [SerializeField]
    private TabGroup tabGroup = null;
    /// <summary>
    /// 無期限タブ
    /// </summary>
    [SerializeField]
    private PresentBoxTab unlimitedTab = null;
    /// <summary>
    /// 有期限タブ
    /// </summary>
    [SerializeField]
    private PresentBoxTab limitedTab = null;
    /// <summary>
    /// 履歴タブ
    /// </summary>
    [SerializeField]
    private PresentBoxTab historyTab = null;
    /// <summary>
    /// スクロールビュー
    /// </summary>
    [SerializeField]
    private InfiniteScrollView scrollView = null;
    /// <summary>
    /// スクロールビュー要素プレハブ
    /// </summary>
    [SerializeField]
    private PresentBoxItem presentBoxItemPrefab = null;
    /// <summary>
    /// 左矢印
    /// </summary>
    [SerializeField]
    private GameObject leftArrowObject = null;
    /// <summary>
    /// 右矢印
    /// </summary>
    [SerializeField]
    private GameObject rightArrowObject = null;
    /// <summary>
    /// 一括受け取りボタン
    /// </summary>
    [SerializeField]
    private Button receiveAllButton = null;
    /// <summary>
    /// 空であることを通知するオブジェクト
    /// </summary>
    [SerializeField]
    private GameObject emptyTextObject = null;
    /// <summary>
    /// ページ数テキスト
    /// </summary>
    [SerializeField]
    private Text pageCountText = null;
    /// <summary>
    /// 受け取り成功通知ダイアログ内容プレハブ
    /// </summary>
    [SerializeField]
    private PresentReceiveDialogContent receiveSuccessContentPrefab = null;
    /// <summary>
    /// 受け取り失敗通知ダイアログ内容プレハブ
    /// </summary>
    [SerializeField]
    private PresentReceiveDialogContent receiveFailedContentPrefab = null;
    /// <summary>
    /// VipレベルアップエフェクトPrefab
    /// </summary>
    [SerializeField]
    private UIVipLevelUp vipLevelUpPrefab = null;

    /// <summary>
    /// リスト確認通信のレスポンスデータ
    /// </summary>
    private PresentApi.ListResponseData response = null;
    /// <summary>
    /// 現在選択中のタブ
    /// </summary>
    private PresentBoxTab selectedTab = null;
    /// <summary>
    /// ローダー
    /// </summary>
    private AssetListLoader loader = null;
    /// <summary>
    /// Closeコールバック
    /// </summary>
    private Action<PresentBoxDialogContent> onClose = null;

    /// <summary>
    /// OnDestroy
    /// </summary>
    private void OnDestroy()
    {
        if (this.loader != null)
        {
            this.loader.Unload();
            this.loader.Clear();
            this.loader = null;
        }

        this.onClose?.Invoke(this);
    }

    /// <summary>
    /// 開く
    /// </summary>
    public static void Open(PresentBoxDialogContent prefab, Action<PresentBoxDialogContent> onClose)
    {
        //BOX内リスト確認通信
        PresentApi.CallListApi((response) =>
        {
            //ローダー準備
            var loader = new AssetListLoader(response.tPresentBox
                .Concat(response.tPresentBoxLimited)
                .Concat(response.tPresentBoxReceived)
                .Select(x => CommonIconUtility.GetItemInfo(x.itemType, x.itemId))
                .Where(x => !x.IsCommonSprite())
                .Select(x => new AssetLoader<Sprite>(x.GetSpritePath())));

            //ロード中はタッチブロック
            SharedUI.Instance.DisableTouch();

            //ロード
            loader.Load(() =>
            {
                //タッチブロック解除
                SharedUI.Instance.EnableTouch();

                //ダイアログ表示
                var dialog = SharedUI.Instance.ShowSimpleDialog();
                dialog.titleText.text = Masters.LocalizeTextDB.Get("PresentBoxTitle");
                dialog.closeButtonEnabled = true;

                var content = dialog.AddContent(prefab);
                content.Setup(response, loader);
                content.onClose = onClose;
            });
        });
    }

    /// <summary>
    /// Awake
    /// </summary>
    private void Awake()
    {
        //デフォルトは無期限タブ
        this.selectedTab = this.unlimitedTab;
        this.tabGroup.Setup();
        this.tabGroup.SetActiveTab(this.selectedTab);
    }

    /// <summary>
    /// 内容構築
    /// </summary>
    private void Setup(PresentApi.ListResponseData response, AssetListLoader loader)
    {
        this.response = response;
        this.loader = loader;

        //各タブに要素を割り当てる
        this.unlimitedTab.SetItems(response.tPresentBox.ToArray(), 10);
        this.limitedTab.SetItems(response.tPresentBoxLimited.ToArray(), 10);
        this.historyTab.SetItems(response.tPresentBoxReceived.ToArray(), 999);

        //一括受け取りボタンの表示設定
        this.SetReceiveAllButtonView();

        //スクロールビュー構築
        this.SetPageNo(this.selectedTab.pageIndex);
    }

    /// <summary>
    /// 一括受け取りボタンの表示設定
    /// </summary>
    private void SetReceiveAllButtonView()
    {
        if (this.selectedTab == this.historyTab)
        {
            //履歴タブの時は非表示
            this.receiveAllButton.gameObject.SetActive(false);
            this.emptyTextObject.SetActive(false);
        }
        else
        {
            this.receiveAllButton.gameObject.SetActive(true);

            //受け取れるアイテムが存在しているならボタン押せる
            this.receiveAllButton.enabled = this.selectedTab.items[0].Length > 0;

            //受け取れるアイテムが存在してなかったらその旨を通知
            this.emptyTextObject.SetActive(!this.receiveAllButton.enabled);

            //ボタンが押せない場合はグレースケールに
            var material = this.receiveAllButton.enabled ? null : SharedUI.Instance.grayScaleMaterial;

            foreach (var graphic in this.receiveAllButton.GetComponentsInChildren<Graphic>())
            {
                graphic.material = material;
            }
        }
    }

    /// <summary>
    /// ページ切り替え
    /// </summary>
    private void SetPageNo(int pageIndex)
    {
        //総ページ数の範囲内に収める
        this.selectedTab.pageIndex = Mathf.Clamp(pageIndex, 0, this.selectedTab.items.Length - 1);

        //ページ数テキスト
        this.pageCountText.text = string.Format("{0}/{1}", this.selectedTab.pageIndex + 1, this.selectedTab.items.Length);

        //左右矢印の表示設定
        this.leftArrowObject.SetActive(this.selectedTab.pageIndex > 0);
        this.rightArrowObject.SetActive(this.selectedTab.pageIndex + 1 < this.selectedTab.items.Length);

        //スクロールビュー構築
        this.scrollView.Initialize(this.presentBoxItemPrefab.gameObject, this.selectedTab.items[this.selectedTab.pageIndex].Length, this.OnUpdateElement);
    }

    /// <summary>
    /// タブクリック時
    /// </summary>
    public void OnClickTab(PresentBoxTab tab)
    {
        SoundManager.Instance.PlaySe(SeName.YES);

        //タブ切り替え
        this.selectedTab = tab;

        //一括受け取りボタンの表示設定
        this.SetReceiveAllButtonView();

        //スクロールビュー構築
        this.SetPageNo(this.selectedTab.pageIndex);
    }

    /// <summary>
    /// スクロールビュー要素更新時
    /// </summary>
    private void OnUpdateElement(GameObject gobj, int index)
    {
        var item = gobj.GetComponent<PresentBoxItem>();
        item.BuildView(this.selectedTab.items[this.selectedTab.pageIndex][index], this.OnClickReceiveButton);
    }

    /// <summary>
    /// 左矢印押下時
    /// </summary>
    public void OnClickLeftArrow()
    {
        this.SetPageNo(this.selectedTab.pageIndex - 1);
    }

    /// <summary>
    /// 右矢印押下時
    /// </summary>
    public void OnClickRightArrow()
    {
        this.SetPageNo(this.selectedTab.pageIndex + 1);
    }

    /// <summary>
    /// 受け取りボタン押下時
    /// </summary>
    private void OnClickReceiveButton(PresentBoxItem item)
    {
        //受け取り通信
        PresentApi.CallReceiveApi(
            new[]{ item.server },
            this.OnReceiveSuccess,
            this.OnReceiveError
        );
    }

    /// <summary>
    /// 一括受け取りボタン押下時
    /// </summary>
    public void OnClickReceiveAllButton()
    {
        //一括受け取り通信
        PresentApi.CallReceiveApi(
            this.selectedTab.items[this.selectedTab.pageIndex],
            this.OnReceiveSuccess,
            this.OnReceiveError
        );
    }

    /// <summary>
    /// 受け取り通信成功時
    /// </summary>
    private void OnReceiveSuccess(PresentApi.ReceiveListResponseData response)
    {
        bool hasReceived = response.tPresentBoxReceived != null && response.tPresentBoxReceived.Length > 0;
        bool hasWakuFull = response.wakuFull != null && response.wakuFull.Length > 0;

        if (!hasReceived && !hasWakuFull)
        {
            //何も表示出来るものが無いのでエラー扱い
            this.OnReceiveError((int)ErrorCode.PresentBoxError);
            return;
        }

        //結果通知ダイアログ生成
        var dialog = SharedUI.Instance.ShowSimpleDialog();
        dialog.closeButtonEnabled = true;
        dialog.onClose = this.Reload;

        if (hasReceived)
        {
            //受け取れたものを表示
            var content = dialog.AddContent(this.receiveSuccessContentPrefab);
            content.BuildView(response.tPresentBoxReceived);
        }

        if (hasWakuFull)
        {
            //所持上限で受け取れなかったものを表示
            var content = dialog.AddContent(this.receiveFailedContentPrefab);
            content.BuildView(response.wakuFull);
        }
    }

    /// <summary>
    /// 受け取り通信失敗時
    /// </summary>
    private void OnReceiveError(int errorCode)
    {
        var dialog = SharedUI.Instance.ShowSimpleDialog(true);
        var content = dialog.SetAsMessageDialog(string.Format("ERROR_CODE : {0}", errorCode));
        content.buttonGroup.buttons[0].onClick = dialog.Close;
        dialog.onClose = this.Reload;
    }

    /// <summary>
    /// リロード
    /// </summary>
    private void Reload()
    {
        //BOX内リスト確認通信
        PresentApi.CallListApi((response) =>
        {
            //追加ローダー準備
            var addLoader = new AssetListLoader(response.tPresentBox
                .Concat(response.tPresentBoxLimited)
                .Concat(response.tPresentBoxReceived)
                .Select(x => CommonIconUtility.GetItemInfo(x.itemType, x.itemId))
                .Where(x1 => !x1.IsCommonSprite() && !this.loader.Exists(x2 => x2.path == x1.GetSpritePath()))
                .Select(x => new AssetLoader<Sprite>(x.GetSpritePath())));

            //ロード中はタッチブロック
            SharedUI.Instance.DisableTouch();

            //ロード
            addLoader.Load(() =>
            {
                //タッチブロック解除
                SharedUI.Instance.EnableTouch();
                //ローダー統合
                this.loader.AddRange(addLoader);
                //再セットアップ
                this.Setup(response, this.loader);
                //必要ならVIPレベルアップ表示
                UIVipLevelUp.OpenIfNeed(this.vipLevelUpPrefab);
            });
        });
    }

    /// <summary>
    /// BOX内件数を取得
    /// </summary>
    public uint GetBoxCount()
    {
        return this.response.tPresentBoxCount + this.response.tPresentBoxLimitedCount;
    }

#if false
    /// <summary>
    /// プレゼントBox固有のエラー対応
    /// </summary>
    private void OnReceiveError(int errorCode)
    {
        string errorMessage = "";
        var dialog = SharedUI.Instance.ShowSimpleDialog();

        switch ((ErrorCode)errorCode)
        {
            case ErrorCode.PresentBoxError:
                errorMessage = Masters.LocalizeTextDB.Get("ErrorPresentBox");
                dialog.onClose = OnReloadPresentBox;
                break;
            case ErrorCode.PresentBoxNotFound:
                errorMessage = Masters.LocalizeTextDB.Get("ErrorPresentBoxNotFound");
                dialog.onClose = OnReloadPresentBox;
                break;
            case ErrorCode.PresentBoxClosed:
                errorMessage = Masters.LocalizeTextDB.Get("ErrorPresentBoxClosed");
                dialog.onClose = OnReloadPresentBox;
                break;
            case ErrorCode.PresentBoxReceived:
                errorMessage = Masters.LocalizeTextDB.Get("ErrorPresentBoxReceived");
                dialog.onClose = OnReloadPresentBox;
                break;
            case ErrorCode.PresentBoxEmpty:
                errorMessage = Masters.LocalizeTextDB.Get("ErrorPresentBoxEmpty");
                dialog.onClose = OnReloadPresentBox;
                break;
            case ErrorCode.PresentBoxMaxPossession:
                errorMessage = Masters.LocalizeTextDB.Get("ErrorPresentBoxMaxPossession");
                dialog.onClose = OnReloadPresentBox;
                break;
            default:
                //ひとまず元の画面に戻す
                errorMessage = Masters.LocalizeTextDB.Get("ConnectErrorMessage");
                dialog.onClose = OnReloadPresentBox;
                break;
        }

        var content = dialog.SetAsMessageDialog(errorMessage);
        content.buttonGroup.buttons[0].onClick = dialog.Close;
    }
#endif
}
