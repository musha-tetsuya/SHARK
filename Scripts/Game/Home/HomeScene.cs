using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.Linq;
using UnityEngine.UI;

public class HomeScene : SceneBase
{
    /// <summary>
    /// ユーザー情報ダイアログ内容プレハブ
    /// </summary>
    [SerializeField]
    private UserInformationDialogContent userInformationDialogContentPrefab = null;
    /// <summary>
    /// ユーザー情報ダイアログ内容で使う砲台ビューワープレハブ
    /// </summary>
    [SerializeField]
    private TurretViewer userInfomationDialogTurretViewerPrefab = null;
    /// <summary>
    /// ログインボーナスダイアログ
    /// </summary>
    [SerializeField]
    private LoginBonusDialogContent loginBonusDialogContentPrefab = null;
    /// <summary>
    /// プレゼントBoxダイアログプレハブ
    /// </summary>
    [SerializeField]
    private PresentBoxDialogContent presentBoxDialogContentPrefab = null;
    /// <summary>
    /// ミッションダイアログプレハブ
    /// </summary>
    [SerializeField]
    private MissionDialog missionDialog = null;
    /// <summary>
    /// プレゼントBOX件数バッジ
    /// </summary>
    [SerializeField]
    private GameObject presentBoxCountBadge = null;
    /// <summary>
    /// プレゼントBOX件数テキスト
    /// </summary>
    [SerializeField]
    private Text presentBoxCountText = null;
    /// <summary>
    /// 大会モードボタンオブジェクト
    /// </summary>
    [SerializeField]
    private GameObject tourObj = null;

    /// <summary>
    /// 無期限Boxの上限超過フラグ
    /// </summary>
    public static bool isMaxPossession = false;

    /// <summary>
    /// アイテムのアイコンのリソースのパス
    /// </summary>
    public HashSet<string> itemIconPaths = new HashSet<string>();


    /// <summary>
    /// Awake
    /// </summary>
    protected override void Awake()
    {
        base.Awake();

        //シーン切り替えアニメーションは手動で消す
        SceneChanger.IsAutoHideLoading = false;

        //大会モードグレーアウト
        foreach (var graphic in this.tourObj.GetComponentsInChildren<Graphic>(true))
        {
            graphic.material = SharedUI.Instance.grayScaleMaterial;
        }
        foreach (var animator in this.tourObj.GetComponentsInChildren<Animator>(true))
        {
            animator.enabled = false;
        }
    }

    /// <summary>
    /// Start
    /// </summary>
    private void Start()
    {
        // Photon Master Server から、接続解除
        if(PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
        }

        //Home情報取得通信
        HomeApi.CallHomeDataApi((response) =>
        {
            //BGM再生
            SoundManager.Instance.PlayBgm(BgmName.HOME);

            //ローディング表示消す
            SharedUI.Instance.HideSceneChangeAnimation();

            //プレゼントBOX件数表示更新
            uint presentBoxCount = response.tPresentBoxCount + response.tPresentBoxLimitedCount;
            this.presentBoxCountBadge.SetActive(presentBoxCount > 0);
            this.presentBoxCountText.text = presentBoxCount.ToString();

            //必要ならログボ表示
            this.OpenLoginBonusIfNeed(response);
        });
    }

    /// <summary>
    /// 必要ならログインボーナス表示
    /// </summary>
    private void OpenLoginBonusIfNeed(HomeApi.HomeDataResponse response)
    {
        if (response.loginBonusChk)
        {
            //ダイアログ表示
            var dialog = SharedUI.Instance.ShowSimpleDialog();
            dialog.titleText.text = Masters.LocalizeTextDB.Get("LoginBonus");
            dialog.onClose = () => this.OpenSpecialLoginBonusIfNeed(response);

            var content = dialog.AddContent(this.loginBonusDialogContentPrefab);
            content.Set(dialog, response.tLoginBonus.loginBonusId);
        }
        else
        {
            this.OpenSpecialLoginBonusIfNeed(response);
        }
    }

    /// <summary>
    /// 必要ならスペシャルログインボーナス表示
    /// </summary>
    private void OpenSpecialLoginBonusIfNeed(HomeApi.HomeDataResponse response)
    {
        if (response.specialLoginBonusChk)
        {
            //ダイアログ表示
            var dialog = SharedUI.Instance.ShowSimpleDialog();
            dialog.titleText.text = Masters.LocalizeTextDB.Get("SpecialLoginBonus");

            var content = dialog.AddContent(this.loginBonusDialogContentPrefab);
            content.SetSpecialLoginBonus(dialog, response.tLoginBonusSpecial.loginBonusId);
        }
    }

    /// <summary>
    /// プレゼントBoxの期限なしBoxが上限以上の場合に表示するダイアログを開く
    /// </summary>
    public static void OpenPresentBoxOverDialog()
    {
        var dialog = SharedUI.Instance.ShowSimpleDialog();
        var content = dialog.SetAsMessageDialog(Masters.LocalizeTextDB.Get("PresentBoxMaxPossession"));
        content.buttonGroup.buttons[0].onClick = dialog.Close;
    }

    /// <summary>
    /// headerのユーザーアイコンをクリック時
    /// </summary>
    public override void OnClickUserIcon()
    {
        //ユーザー情報ダイアログ開く
        UserInformationDialogContent.Open(this.userInformationDialogContentPrefab, this.userInfomationDialogTurretViewerPrefab);
    }

    /// <summary>
    /// マルチの遷移ボタンをクリック時
    /// </summary>
    public void OnTapGameModeMultiButton()
    {
        //プレゼントBoxの期限なしアイテムが上限以上の場合に、一部機能に遷移できないようにする
        if (!isMaxPossession)
        {
            SceneChanger.ChangeSceneAsync("MultiStageSelect");
        }
        else
        {
            OpenPresentBoxOverDialog();
        }
    }

    /// <summary>
    /// シングルの遷移ボタンをクリック時
    /// </summary>
    public void OnTapGameModeSingleButton()
    {
        if (!isMaxPossession)
        {
            SceneChanger.ChangeSceneAsync("SingleStageSelect");
        }
        else
        {
            OpenPresentBoxOverDialog();
        }
    }

    /// <summary>
    /// 大会の遷移ボタンをクリック時
    /// </summary>
    public void OnTapGameModeTournamentButton()
    {

    }

    /// <summary>
    /// お知らせの遷移ボタンをクリック時
    /// </summary>
    public void OnTapNoticeButton()
    {
        var url = "http://dev-fish-1.sunchoi.co.jp/notice";
        Application.OpenURL(url);
    }

    /// <summary>
    /// プレゼントBoxの遷移ボタンをクリック時
    /// </summary>
    public void OnTapPresentBoxButton()
    {
        //プレゼントBOX開く
        PresentBoxDialogContent.Open(this.presentBoxDialogContentPrefab, (content) =>
        {
            //プレゼントBOX件数表示更新
            uint presentBoxCount = content.GetBoxCount();
            this.presentBoxCountBadge.SetActive(presentBoxCount > 0);
            this.presentBoxCountText.text = presentBoxCount.ToString();
        });
    }

    /// <summary>
    /// 砲台の遷移ボタンをクリック時
    /// </summary>
    public void OnTapCustomTurretButton()
    {
        SceneChanger.ChangeSceneAsync("CustomTurret");
    }

    /// <summary>
    /// アイテムの遷移ボタンをクリック時
    /// </summary>
    public void OnTapInventoryButton()
    {
        SceneChanger.ChangeSceneAsync("ItemInventory");
    }

    /// <summary>
    /// ミッションの遷移ボタンをクリック時
    /// </summary>
    public void OnTapMissionButton()
    {
        if (!isMaxPossession)
        {
            MissionDialog.Open(this.missionDialog, null);
        }
        else
        {
            OpenPresentBoxOverDialog();
        }
    }

    /// <summary>
    /// ランキングの遷移ボタンをクリック時
    /// </summary>
    public void OnTapRankingButton()
    {

    }

    /// <summary>
    /// ショップの遷移ボタンをクリック時
    /// </summary>
    public void OnTapShopButton()
    {
        if (!isMaxPossession)
        {
            SceneChanger.ChangeSceneAsync("Shop");
        }
        else
        {
            OpenPresentBoxOverDialog();
        }
    }
}
