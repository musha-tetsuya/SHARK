using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.U2D;

public class TitleScreen : SceneBase
{
    /// <summary>
    /// メニューダイアログ
    /// </summary>
    [SerializeField]
    private MenuDialogContent menuDialogContentPrefab = null;
    /// <summary>
    /// ファイルダウンロードダイアログ
    /// </summary>
    [SerializeField]
    private FileDownloadDialogContent downloadDialogContentPrefab = null;
    /// <summary>
    /// クレジットプレハブ
    /// </summary>
    [SerializeField]
    private GameObject creditPrefab = null;

    /// <summary>
    /// BGクリック回数
    /// </summary>
    private int bgClickCount = 0;

    /// <summary>
    /// Awake
    /// </summary>
    protected override void Awake()
    {
        base.Awake();

        UserData.Set(new UserData());
        UserData.Get().Load();
    }

    /// <summary>
    /// Start
    /// </summary>
    private void Start()
    {
        // サウンドボリュームセット
        UserOptionDialogContent.SetBgmVolume(UserData.bgmVolume);
        UserOptionDialogContent.SetSeVolume(UserData.seVolume);

        SoundManager.Instance.PlayBgm(BgmName.SELECT);
    }

    /// <summary>
    /// メニューボタンクリック時
    /// </summary>
    public void OnTapMenuButton()
    {
        //メニューダイアログ表示
        var menuDialog = SharedUI.Instance.ShowSimpleDialog();
        menuDialog.titleText.text = Masters.LocalizeTextDB.Get("Menu");
        menuDialog.closeButtonEnabled = true;

        var content = menuDialog.AddContent(this.menuDialogContentPrefab);
        content.Set(menuDialog, this.CheckResourceList);
    }

    /// <summary>
    /// スタートボタンクリック時
    /// </summary>
    public void OnTapStartButton()
    {
        this.CheckResourceList();
    }

    /// <summary>
    /// リソースリストチェック
    /// </summary>
    private void CheckResourceList()
    {
#if !USE_ASSETBUNDLE
        this.Login();
        return;
#endif
        //リソースバージョン取得通信
        TopApi.CallTopApi((resourceVersion) =>
        {
            var handle = new FileDownloadHandle(resourceVersion, "infoList.dat");
            handle.isAutoSave = false;

            //リソースリストダウンロード開始
            var downloader = new FileDownloadManager();
            downloader.Add(handle);
            downloader.DownloadStart();

            //タッチブロック
            SharedUI.Instance.DisableTouch();
            
            //ダウンロード完了時
            downloader.onCompleted = () =>
            {
                //タッチブロック解除
                SharedUI.Instance.EnableTouch();

                var directory = AssetManager.GetAssetBundleDirectoryPath();
                
                //旧リソースリスト
                var oldInfoList = new List<AssetBundleInfo>();
                {
                    var path = Path.Combine(directory, handle.hash);
                    if (File.Exists(path))
                    {
                        var bytes = File.ReadAllBytes(path).Cryption();
                        oldInfoList.AddRange(BinaryUtility.CreateArray<AssetBundleInfo>(bytes));
                    }
                }

                //新リソースリスト
                var newInfoList = new List<AssetBundleInfo>();
                {
                    var bytes = handle.bytes.Cryption();
                    newInfoList.AddRange(BinaryUtility.CreateArray<AssetBundleInfo>(bytes));
                }

                //ダウンロード対象
                var targetInfoList = new List<AssetBundleInfo>();

                for (int i = 0; i < newInfoList.Count; i++)
                {
                    var targetInfo = newInfoList[i];
                    var filePath = Path.Combine(directory, targetInfo.assetBundleName.GetHashString());

                    //既存ファイルの場合
                    if (File.Exists(filePath))
                    {
                        //CRC値が同じならダウンロードの必要無し
                        var oldInfo = oldInfoList.Find(x => x.assetBundleName == targetInfo.assetBundleName);
                        if (oldInfo != null && oldInfo.crc == targetInfo.crc)
                        {
                            continue;
                        }
                    }

                    //ダウンロード対象として追加
                    targetInfoList.Add(targetInfo);
                }

                //ダウンロードする必要があるなら
                if (targetInfoList.Count > 0)
                {
                    //確認ダイアログ表示
                    this.OpenDownlodConfirmDialog(targetInfoList, () =>
                    {
                        //ダウンロード開始
                        var dialog = SharedUI.Instance.ShowSimpleDialog();
                        var content = dialog.AddContent(this.downloadDialogContentPrefab);
                        content.Setup(dialog, resourceVersion, oldInfoList, handle, targetInfoList);

                        //ダウンロード終わったら
                        dialog.onClose = () =>
                        {
                            //アセットバンドル情報のセット
                            AssetManager.SetAssetBundleInfoList(newInfoList);
                            //ログイン
                            this.Login();
                        };
                    });
                }
                else
                {
                    //アセットバンドル情報のセット
                    AssetManager.SetAssetBundleInfoList(newInfoList);
                    //ログイン
                    this.Login();
                }
            };
        });
    }

    /// <summary>
    /// ダウンロード確認ダイアログを開く
    /// </summary>
    private void OpenDownlodConfirmDialog(List<AssetBundleInfo> targetInfoList, Action onClickYes)
    {
        //確認ダイアログ表示
        var dialog = SharedUI.Instance.ShowSimpleDialog();
        var totalFileSize = Mathf.CeilToInt(targetInfoList.Sum(x => x.fileSize) / 100000f) * 0.1f;
        var content = dialog.SetAsYesNoMessageDialog(Masters.LocalizeTextDB.GetFormat("NoticeDownloadFileSize", totalFileSize));
        content.yesNo.yes.onClick = () =>
        {
            //ダウンロード開始
            dialog.Close();
            dialog.onClose = onClickYes;
        };
        content.yesNo.no.onClick = () =>
        {
            //戻る
            dialog.Close();
        };
    }

    /// <summary>
    /// ログイン処理
    /// </summary>
    private void Login()
    {
        var queue = new Queue<Action>();

        //ユーザーデータがある
        if (UserData.Get().userId > 0)
        {
            //ログイン
            queue.Enqueue(() => LoginApi.CallLoginApi(
                UserData.Get(),
                queue.Dequeue()
            ));
        }
        //ユーザーデータがない
        else
        {
            //ユーザーデータ作成
            queue.Enqueue(() => UserApi.CallCreateApi(
                "GuestUser",
                queue.Dequeue()
            ));
        }

        //ユーザー情報取得
        queue.Enqueue(() => FirstApi.CallFirstUserApi(
            UserData.Get(),
            queue.Dequeue()
        ));

        //マスター分割取得その１
        queue.Enqueue(() => MasterApi.CallGetMasterApi(
            queue.Dequeue(),
            Masters.AccessoriesDB,
            Masters.BarrelDB,
            Masters.BatteryDB,
            Masters.BulletDB,
            Masters.FvAttackDB,
            Masters.TurretSerieseDB,
            Masters.CannonSetDB,
            Masters.ConfigDB,
            Masters.FishDB,
            Masters.FishCaptureDB
        ));

        //マスター分割取得その２
        queue.Enqueue(() => MasterApi.CallGetMasterApi(
            queue.Dequeue(),
            Masters.FishCategoryDB,
            Masters.FishParticleDB,
            Masters.GearDB,
            Masters.BattleItemDB,
            Masters.ItemSellDB,
            Masters.LevelDB,
            Masters.BetDB,
            Masters.LocalizeTextDB,
            Masters.LoginBonusDB,
            Masters.LoginBonusSpecialDB
        ));

        //マスター分割取得その３
        queue.Enqueue(() => MasterApi.CallGetMasterApi(
            queue.Dequeue(),
            Masters.PartsExpansionDB,
            Masters.CannonExpansionDB,
            Masters.GearExpansionDB,
            Masters.MessageDB,
            Masters.MissionTypeDB,
            Masters.MissionRewardDB,
            Masters.MultiWorldDB,
            Masters.MultiBallDropRateDB,
            Masters.MultiSoulDropRateDB,
            Masters.MultiStageFishDB
        ));

        //マスター分割取得その４
        queue.Enqueue(() => MasterApi.CallGetMasterApi(
            queue.Dequeue(),
            Masters.SerieseSkillDB,
            Masters.SingleStageDB,
            Masters.SingleStageFishDB,
            Masters.SingleStageFirstRewardDB,
            Masters.SingleStageRewardDB,
            Masters.SingleStageRewardLotDB,
            Masters.SingleWorldDB,
            Masters.SkillDB,
            Masters.SkillGroupDB,
            Masters.VipBenefitDB
        ));

        //マスター分割取得その５
        queue.Enqueue(() => MasterApi.CallGetMasterApi(
            queue.Dequeue(),
            Masters.VipBenefitTypeDB,
            Masters.VipLevelDB,
            Masters.VipRewardDB
        ));

        //ローカライズアトラスセット
        queue.Enqueue(() =>
        {
            var handle = AssetManager.Load<SpriteAtlas>(LocalizeImage.GetLocalizationAtlasPath(), (asset) =>
            {
                var atlas = new AtlasSpriteCache(asset);
                GlobalSpriteAtlas.SetAtlas(GlobalSpriteAtlas.AtlasType.Localization, atlas);
                queue.Dequeue().Invoke();
            });

            handle.isDontDestroy = true;
        });

        //HOMEシーンへ
        queue.Enqueue(() =>
            SceneChanger.ChangeSceneAsync("Home")
        );

        //Queue実行
        queue.Dequeue().Invoke();
    }

    /// <summary>
    /// BGクリック時
    /// </summary>
    public void OnClickBg()
    {
        if (++this.bgClickCount == 10)
        {
            SoundManager.Instance.PlaySe(SeName.PARTS_CHANGE);
            this.bgClickCount = 0;
            var dialog = SharedUI.Instance.ShowSimpleDialog();
            dialog.titleText.text = "Credit";
            dialog.closeButtonEnabled = true;
            dialog.AddContent(this.creditPrefab);
        }
    }

#if DEBUG
    /// <summary>
    /// GUIボタンスタイル
    /// </summary>
    private GUIStyle buttonStyle = null;

    /// <summary>
    /// GUI描画
    /// </summary>
    public override void DrawGUI()
    {
        if (this.buttonStyle == null)
        {
            this.buttonStyle = UIUtility.CreateButtonStyle(600, 50);
        }

        GUILayout.Button("BuildNo." + SharedUI.Instance.buildData.buildNumber, this.buttonStyle);

        var userData = UserData.Get();
        int userId = userData == null ? 0 : userData.userId;
        GUILayout.Button("UserID:" + userId, this.buttonStyle);

        if (GUILayout.Button("Delete User Data", this.buttonStyle))
        {
            PlayerPrefs.DeleteAll();
            UserData.Set(new UserData());
        }
    }
#endif

}
