using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using System;

public class UserInformationDialogContent : MonoBehaviour
{
    /// <summary>
    /// ユーザー名テキスト
    /// </summary>
    [SerializeField]
    private Text userNameText = null;
    /// <summary>
    /// ユーザー名入力
    /// </summary>
    [SerializeField]
    private Text inputField = null;
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
    /// 名前変更
    /// </summary>
    [SerializeField]
    private GameObject changeNameAreaGameObject = null;
    /// <summary>
    /// ユーザーID
    /// </summary>
    [SerializeField]
    private Text userIdText = null;
    /// <summary>
    /// 最高ベット
    /// </summary>
    [SerializeField]
    private Text maxBetText = null;
    /// <summary>
    /// ユーザ機器移動IDテキスト
    /// </summary>
    [SerializeField]
    private Text takeOverId = null;
    /// <summary>
    /// ユーザ機器移動PASSテキスト
    /// </summary>
    [SerializeField]
    private Text takeOverPass = null;
    /// <summary>
    /// IDテキストコピーボタン
    /// </summary>
    [SerializeField]
    private Button takeOverIdCopyButton = null;
    /// <summary>
    /// PASSテキストコピーボタン
    /// </summary>
    [SerializeField]
    private Button takeOverPassCopyButton = null;
    /// <summary>
    /// ボタングレイアウト
    /// </summary>
    [SerializeField]
    private Graphic grayoutTakeOverIdCopyButton = null;
    [SerializeField]
    private Graphic grayoutTakeOverPassCopyButton = null;
    /// <summary>
    /// コピー時、メッセージボックス
    /// </summary>
    [SerializeField]
    private GameObject messagePopupGameObject = null;

    /// <summary>
    /// 砲台ビューワー
    /// </summary>
    private TurretViewer turretViewer = null;
    /// <summary>
    /// ローダー
    /// </summary>
    private AssetListLoader loader = null;
    
    /// <summary>
    /// ユーザ機器移動APIのresponseデータ
    /// </summary>
    private UserApi.CreateDeviceChangeCodeResponseData response = null;

    /// <summary>
    /// OnDestroy
    /// </summary>
    private void OnDestroy()
    {
        if (this.turretViewer != null)
        {
            Destroy(this.turretViewer.gameObject);
            this.turretViewer = null;
        }

        if (this.loader != null)
        {
            this.loader.Unload();
            this.loader.Clear();
            this.loader = null;
        }
    }

    /// <summary>
    /// 開く
    /// </summary>
    public static void Open(UserInformationDialogContent contentPrefab, TurretViewer turretViewerPrefab)
    {
        var selectedTurret = UserData.Get().GetSelectedTurretData();
        var batteryData = Masters.BatteryDB.FindById(selectedTurret.batteryMasterId);
        var barrelData = Masters.BarrelDB.FindById(selectedTurret.barrelMasterId);
        var bulletData = Masters.BulletDB.FindById(selectedTurret.bulletMasterId);

        var loader = new AssetListLoader();
        loader.Add<GameObject>(SharkDefine.GetBatteryPrefabPath(batteryData.key));
        loader.Add<GameObject>(SharkDefine.GetBarrelPrefabPath(barrelData.key));
        loader.Add<BulletBase>(SharkDefine.GetBulletPrefabPath(bulletData.key));

        //ロード中はタッチブロック
        SharedUI.Instance.DisableTouch();

        //ロード
        loader.Load(() =>
        {
            //タッチブロック解除
            SharedUI.Instance.EnableTouch();

            //砲台表示
            var turretViewer = Instantiate(turretViewerPrefab, null, false);
            turretViewer.BatteryKey = batteryData.key;
            turretViewer.BarrelKey = barrelData.key;
            turretViewer.BulletKey = bulletData.key;
            turretViewer.Reflesh();
            turretViewer.StartShot();

            //ダイアログ開く
            var dialog = SharedUI.Instance.ShowSimpleDialog();
            dialog.closeButtonEnabled = true;
            dialog.titleText.text = Masters.LocalizeTextDB.Get("UserInformation");

            var content = dialog.AddContent(contentPrefab);
            content.Set(loader, turretViewer);
        });
    }

    /// <summary>
    /// セット
    /// </summary>
    private void Set(AssetListLoader loader, TurretViewer turretViewer)
    {
        this.loader = loader;
        this.turretViewer = turretViewer;

        var userData = UserData.Get();

        if (userData != null)
        {
            //ユーザー名
            this.userNameText.text = userData.name;

            //ユーザーレベル
            this.userLvText.text = string.Format("Lv.{0}", userData.lv);
            
            //EXPメーター
            if(userData.lv < Masters.LevelDB.GetList().Max(x => x.lv))
            {
                uint needExp = (uint)Masters.LevelDB.GetList()
                .Where(x => x.lv <= userData.lv)
                .Sum(x => x.exp);

                this.expMeter.fillAmount = (float)userData.exp / needExp;
            }
            else
            {
                this.expMeter.fillAmount = 1f;
            }

            // ユーザーID
            this.userIdText.text = Masters.LocalizeTextDB.GetFormat("UserId", userData.userId.ToString());
            
            // 最高ベット
            var betId = Masters.LevelDB.FindById(userData.lv).betId;
            var maxBet = Masters.BetDB.FindById(betId).maxBet;
            this.maxBetText.text = Masters.LocalizeTextDB.GetFormat("UnitMaxbet", maxBet);

            // ユーザ機器移動の情報セット
            this.takeOverId.text = PlayerPrefs.GetString("takeOverId", "");
            this.takeOverPass.text = PlayerPrefs.GetString("takeOverPass", "");

            // ボタンセット
            if (string.IsNullOrEmpty(this.takeOverId.text))
            {
                this.takeOverIdCopyButton.interactable = false;
                this.takeOverPassCopyButton.interactable = false;
                this.SetGrayout(true);
            }
            else
            {
                this.takeOverIdCopyButton.interactable = true;
                this.takeOverPassCopyButton.interactable = true;
                this.SetGrayout(false);
            }
        }
    }

    /// <summary>
    /// ユーザー名変更ボタンクリック時
    /// </summary>
    public void OnClickChangeNameButton()
    {
        this.changeNameAreaGameObject.SetActive(true);
    }

    /// <summary>
    /// ユーザー名変更確認ボタンクリック時
    /// </summary>
    public void OnClickChangeNameConfirmButton()
    {
        // InputFieldに入力したテキスト
        var inputName = inputField.text.ToString();

        // 特殊文字
        string str = @"[~!@\#$%^&*\()\=+|\\/:;?""<>'\uD83C-\uDBFF\uDC00-\uDFFF\u1F30-\u1F5F]";
        // Regex 再宣言
        System.Text.RegularExpressions.Regex rex = new System.Text.RegularExpressions.Regex(str);

        // 文字列の比較(特殊文字、空欄、スペース)
        if(rex.IsMatch(inputName) || String.IsNullOrEmpty(inputName) || String.IsNullOrWhiteSpace(inputName))
        {
            var dialog = SharedUI.Instance.ShowSimpleDialog();
            dialog.closeButtonEnabled = true;
            dialog.AddText(Masters.LocalizeTextDB.Get("DontUseSpecialCharacter"));
        }
        else
        {
            UserApi.CallChangeUserNameApi(inputName, this.OnCompletedChangeName);
        }
    }
    
    /// <summary>
    /// ユーザー名変更API成功の時
    /// </summary>
    private void OnCompletedChangeName()
    {
            // ユーザー名テキスト更新
            this.userNameText.text = UserData.Get().name;
            // ヘッダーのユーザー名更新
            SharedUI.Instance.header.SetInfo(UserData.Get());

            this.changeNameAreaGameObject.SetActive(false);
    }

    /// <summary>
    /// 引継ぎコードを発行するボタンクリック時
    /// </summary>
    public void OnClickCreateDeviceChangeCodeButton()
    {
        UserApi.CallCreateDeviceChangeCode((response) =>
        {
            // API成功時responseコールバック
            this.response = response;
            // ボタンセット
            this.takeOverIdCopyButton.interactable = true;
            this.takeOverPassCopyButton.interactable = true;

            // グレーアウトOFF
            this.SetGrayout(false);

            // ユーザ機器移動の情報セット
            this.takeOverId.text = response.tTakeOver.takeOverId;
            this.takeOverPass.text = response.takeOverPassword;
            
            // 発行されたデータをPlayerPrefsに保存
            PlayerPrefs.SetString("takeOverId", response.tTakeOver.takeOverId);
            PlayerPrefs.SetString("takeOverPass", response.takeOverPassword);
        });
    }

    public void OnClickTakeOverIdCopyButton()
    {
        GUIUtility.systemCopyBuffer = this.takeOverId.text;
        this.messagePopupGameObject.SetActive(true);
        Invoke("ColseMessagePopup", 1f);
    }

    public void OnClickTakeOverPassCopyButton()
    {
        GUIUtility.systemCopyBuffer = this.takeOverPass.text;
        this.messagePopupGameObject.SetActive(true);
        Invoke("ColseMessagePopup", 1f);
    }

    private void ColseMessagePopup()
    {
        this.messagePopupGameObject.SetActive(false);
    }

    /// <summary>
    /// ボタングレーアウトON/OFF
    /// </summary>
    private void SetGrayout(bool isGrayout)
    {
        this.grayoutTakeOverIdCopyButton.material = isGrayout ? SharedUI.Instance.grayScaleMaterial : null;
        this.grayoutTakeOverPassCopyButton.material = isGrayout ? SharedUI.Instance.grayScaleMaterial : null;
    }
}
