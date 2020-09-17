using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ヘッダー
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class HeaderPanel : SafeAreaSupporter
{
    /// <summary>
    /// イベントリスナー
    /// </summary>
    public interface IEventListner
    {
        /// <summary>
        /// ユーザーアイコンクリック時
        /// </summary>
        void OnClickUserIcon();
        /// <summary>
        /// HOMEボタンクリック時
        /// </summary>
        void OnClickHomeButton();
        /// <summary>
        /// コインボタンクリック時
        /// </summary>
        void OnClickCoinButton();
        /// <summary>
        /// ジェムボタンクリック時
        /// </summary>
        void OnClickGemButton();
    }

    //背景
    [SerializeField]
    private Image bg = null;

    /// <summary>
    /// HOMEボタン
    /// </summary>
    [SerializeField]
    private GameObject homeButton = null;
    /// <summary>
    /// ユーザーアイコン
    /// </summary>
    [SerializeField]
    private GameObject userIcon = null;

    /// <summary>
    /// ユーザー名テキスト
    /// </summary>
    [SerializeField]
    private Text userNameText = null;
    /// <summary>
    /// ユーザーレベルテキスト
    /// </summary>
    [SerializeField]
    private Text userLvText = null;
    /// <summary>
    /// 経験値メーター
    /// </summary>
    [SerializeField]
    private Image expMeter = null;
    /// <summary>
    /// コイン数テキスト
    /// </summary>
    [SerializeField]
    private Text coinText = null;
    /// <summary>
    /// ジェム数テキスト
    /// </summary>
    [SerializeField]
    private Text gemText = null;
    [SerializeField]
    private UserOptionDialogContent optionDialogContentPrefab = null;
    /// <summary>
    /// Vipダイアログプレハブ
    /// </summary>
    [SerializeField]
    private VipInfoDialog vipInfoDialog = null;

    /// <summary>
    /// キャンバスグループ
    /// </summary>
    public CanvasGroup canvasGroup { get; private set; }
    /// <summary>
    /// イベントリスナー
    /// </summary>
    private IEventListner eventListner = null;

    /// <summary>
    /// Awake
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        this.canvasGroup = GetComponent<CanvasGroup>();
    }

    /// <summary>
    /// Start
    /// </summary>
    protected override void Start()
    {
        base.Start();
        this.SetBgRectSize();
    }

    /// <summary>
    /// SafeAreaを考慮してBGサイズを調整
    /// </summary>
    private void SetBgRectSize()
    {
        Vector2 size;
        Vector2 pos;

        size = this.bg.rectTransform.sizeDelta;
        size.x += this.rectTransform.offsetMin.x;
        this.bg.rectTransform.sizeDelta = size;

        pos = this.bg.rectTransform.anchoredPosition;
        pos.x -= this.rectTransform.offsetMin.x;
        this.bg.rectTransform.anchoredPosition = pos;
    }

    /// <summary>
    /// 表示構築
    /// </summary>
    public void Set(SceneBase scene)
    {
        //イベントリスナー登録
        this.eventListner = scene;

        //HOMEボタン←→ユーザーアイコン切り替え
        bool isHome = scene is HomeScene;
        this.homeButton.SetActive(!isHome);
        this.userIcon.SetActive(isHome);

        //表示情報の設定
        SetInfo(UserData.Get());
    }

    /// <summary>
    /// 表示する情報のみの更新
    /// </summary>
    public void SetInfo(UserData userData)
    {
        if (userData != null)
        {
            //ユーザー名
            this.userNameText.text = userData.name;

            //ユーザーレベル
            this.userLvText.text = string.Format("Lv.{0}", userData.lv);

            //EXPメーター
            if (userData.lv < Masters.LevelDB.GetList().Max(x => x.lv))
            {
                uint needExp = (uint)Masters.LevelDB.GetList().Where(x => x.lv <= userData.lv).Sum(x => x.exp);
                this.expMeter.fillAmount = (float)userData.exp / needExp;
            }
            else
            {
                this.expMeter.fillAmount = 1f;
            }

            //コイン
            this.coinText.text = userData.coin.ToString();

            //ジェム
            this.gemText.text = userData.totalGem.ToString();
        }
    }

    /// <summary>
    /// ユーザーアイコンクリック時
    /// </summary>
    public void OnClickUserIcon()
    {
        this.eventListner?.OnClickUserIcon();
    }

    /// <summary>
    /// HOMEボタンクリック時
    /// </summary>
    public void OnClickHomeButton()
    {
        this.eventListner?.OnClickHomeButton();
    }

    /// <summary>
    /// コインボタンクリック時
    /// </summary>
    public void OnClickCoinButton()
    {
        this.eventListner?.OnClickCoinButton();
    }

    /// <summary>
    /// ジェムボタンクリック時
    /// </summary>
    public void OnClickGemButton()
    {
        this.eventListner?.OnClickGemButton();
    }

    /// <summary>
    /// Vipボタンクリック時
    /// </summary>
    public void OnClickVipButton()
    {
        VipInfoDialog.Open(this.vipInfoDialog);
    }

    /// <summary>
    /// 設定ボタンクリック時
    /// </summary>
    public void OnClickOptionButton()
    {
        var dialog = SharedUI.Instance.ShowSimpleDialog();
        var content = dialog.AddContent(this.optionDialogContentPrefab);
        content.Setup(dialog);
    }

    /*void OnTapMyInfomationButton()
    {
        var myInfoPopup = SharedUI.Instance.ShowPopup<MyInfomationPopup>(myInfomationPopupPrefab);
        myInfoPopup.Set();
    }*/

    /*void OnTapIntroductionSpecialGiftButton()
    {
        Debug.Log("OnTapIntroductionSpecialGift");
        SharedUI.Instance.TestShowPopup();
    }*/

    /*void OnTapSettingsButton()
    {
        var settingsPopup = SharedUI.Instance.ShowPopup<SettingsPopup>(settingsPopupPrefab);
        settingsPopup.Set();
    }*/
}
