using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

/// <summary>
/// シングルステージセレクトシーン
/// </summary>
public class MultiStageSelectScene : SceneBase
{
#if DEBUG
    public void Disconnect() => PhotonNetwork.Disconnect();
#endif

    /// <summary>
    /// ネットワークイベント監視
    /// </summary>
    [SerializeField]
    private MultiNetworkEventObserver networkEventObserver = null;
    /// <summary>
    /// ステート管理
    /// </summary>
    private StateManager stateManager = new StateManager();
    /// <summary>
    /// ロビー情報
    /// </summary>
    private LobbyInfo[] lobbyInfos = null;
    /// <summary>
    /// ワールド別魚情報ポップアップコンテンツ
    /// </summary>
    [SerializeField]
    private SingleStageFishDictionaryDialogContent fishDictionaryDialogContent = null;

    [Header("ワールドパンネル")]
    /// <summary>
    /// ワールドスクロールビュー
    /// </summary>
    [SerializeField]
    private InfiniteScrollView worldScrollView = null;
    /// <summary>
    /// ワールドパネルプレハブ
    /// </summary>
    [SerializeField]
    private MultiWorldPanel worldPanelPrefab = null;
    /// <summary>
    /// 左矢印
    /// </summary>
    [SerializeField]
    private GameObject leftArrow = null;
    /// <summary>
    /// 右矢印
    /// </summary>
    [SerializeField]
    private GameObject rightArrow = null;
    
    [Header("ワールド情報パンネル")]
    /// <summary>
    /// 情報パンネル
    /// </summary>
    [SerializeField]
    private GameObject infoPanelGameObject = null;
    /// <summary>
    /// ワールド名
    /// </summary>
    [SerializeField]
    private Text worldNameText = null;
    /// <summary>
    /// 倍率
    /// </summary>
    [SerializeField]
    private Text multipleText = null;
    /// <summary>
    /// 現在、プレイ中のユーザー数
    /// </summary>
    [SerializeField]
    private Text totalPlayerNumText = null;
    /// <summary>
    /// バッティング(弾丸消耗コイン)
    /// </summary>
    [SerializeField]
    private Text bettingText = null;
    /// <summary>
    /// 開始ボタン
    /// </summary>
    [SerializeField]
    private Button playButton = null;
    /// <summary>
    /// Playボタングレーアウト
    /// </summary>
    [SerializeField]
    private Graphic[] playButtonGraphic = null;
    /// <summary>
    /// ボールイメージ
    /// </summary>
    [SerializeField]
    private Image ballImage = null;
    /// <summary>
    /// ボールカウンター
    /// </summary>
    [SerializeField]
    private Text ballCount = null;
    /// <summary>
    /// ソウルイメージ
    /// </summary>
    [SerializeField]
    private Image[] soulImage = null;

    /// <summary>
    /// マルチワールドデータ
    /// </summary>
    public class WorldData
    {
        public Master.MultiWorldData worldMasterData = null;
        public MultiPlayApi.TMultiWorld worldServerData = null;
        public MultiPlayApi.TJackpotInfo worldJackpotInfoData = null;
        public MultiPlayApi.TMultiSoulBall multiSoulBallData = null;
        public IAssetLoader bgAssetLoader = null;
    }

    /// <summary>
    /// マルチプレイトップの通信レスポンス
    /// </summary>
    private MultiPlayApi.TopResponseData response = null;
    /// <summary>
    /// 全ワールドのデータ
    /// </summary>
    private WorldData[] worldData = null;
    /// <summary>
    /// 現在フォーカス中のワールドデータ
    /// </summary>
    private WorldData focusedWorldData = null;
    /// <summary>
    /// アセットローダー
    /// </summary>
    private AssetListLoader loader = new AssetListLoader();

    /// <summary>
    /// OnDestroy
    /// </summary>
    private void OnDestroy()
    {
        this.loader.Unload();
    }

    protected override void Awake()
    {
        base.Awake();

        //シーン切り替えアニメーションは手動で消す
        SceneChanger.IsAutoHideLoading = false;
    }

    /// <summary>
    /// Start
    /// </summary>
    private void Start()
    {
        this.stateManager.AddState(new NoneState { scene = this });
        this.stateManager.AddState(new ConnectState { scene = this });
        this.stateManager.AddState(new GetLobyInfoState { scene = this });
        this.stateManager.AddState(new LobbyState { scene = this });

        //プレイ可能なワールドの情報を通信取得
        MultiPlayApi.CallTopApi((response) =>
        {
            this.response = response;
            
            //ロビー情報仮作成
            this.lobbyInfos = response.tMultiWorld
                .Where(x => x.worldStatus >= 0)
                .Select(x => new LobbyInfo { worldId = x.multiWorldId })
                .ToArray();

            // ChangeState
            var state = this.stateManager.GetState<ConnectState>();
            state.nextStateName = typeof(GetLobyInfoState).Name;
            this.stateManager.ChangeState<ConnectState>();
        });
    }

    /// <summary>
    /// 右矢印ボタン押下時
    /// </summary>
    public void OnClickNextButton()
    {
        int i = Array.IndexOf(this.worldData, this.focusedWorldData);
        i = Mathf.Clamp(i + 1, 0, this.worldData.Length - 1);
        this.worldScrollView.SetFocus(i, true);
    }

    /// <summary>
    /// 左矢印ボタン押下時
    /// </summary>
    public void OnClickBackButton()
    {
        int i = Array.IndexOf(this.worldData, this.focusedWorldData);
        i = Mathf.Clamp(i - 1, 0, this.worldData.Length - 1);
        this.worldScrollView.SetFocus(i, true);
    }

    /// <summary>
    /// 魚図鑑開く
    /// </summary>
    public void OnClickFishDictionaryButton()
    {
        SingleStageFishDictionaryDialogContent.Open(this.fishDictionaryDialogContent, this.focusedWorldData.worldMasterData);
    }

    /// <summary>
    /// 挑戦するボタンクリック時
    /// </summary>
    public void OnClickPlayButton()
    {
        (this.stateManager.currentState as LobbyState)?.OnClickPlayButton();
    }

    /// <summary>
    /// ステート基礎
    /// </summary>
    private abstract class MyState : MultiNetworkEventReceiverState
    {
        /// <summary>
        /// シーン
        /// </summary>
        public MultiStageSelectScene scene = null;
        /// <summary>
        /// イベント監視者
        /// </summary>
        public override MultiNetworkEventObserver observer => this.scene.networkEventObserver;

        /// <summary>
        /// 再接続確認ダイアログを開く
        /// </summary>
        protected void ShowReconnectConfirmDialog(string nextStateName)
        {
            //空ステートに退避
            this.manager.ChangeState<NoneState>();

            //再接続確認ダイアログ表示
            var dialog = SharedUI.Instance.ShowSimpleDialog(true);
            var content = dialog.SetAsYesNoMessageDialog(Masters.LocalizeTextDB.Get("ConnectErrorMessage"));
            content.yesNo.yes.onClick = () =>
            {
                //接続ステートへ
                var state = this.manager.GetState<ConnectState>();
                state.nextStateName = nextStateName;
                dialog.onClose = () => this.manager.ChangeState<ConnectState>();
                dialog.Close();
            };
            content.yesNo.no.onClick = () =>
            {
                //Homeへ戻る
                dialog.onClose = () => SceneChanger.ChangeSceneAsync("Home");
                dialog.Close();
            };
        }
    }

    /// <summary>
    /// 空ステート
    /// </summary>
    private class NoneState : MyState
    {

    }

    /// <summary>
    /// マスターサーバー接続ステート
    /// </summary>
    private class ConnectState : MyState
    {
        /// <summary>
        /// 接続成功時コールバック
        /// </summary>
        public string nextStateName = null;

        /// <summary>
        /// Start
        /// </summary>
        public override void Start()
        {
            base.Start();
            PhotonNetwork.ConnectUsingSettings();
        }

        /// <summary>
        /// 接続失敗
        /// </summary>
        public override void OnDisconnected(DisconnectCause cause)
        {
            this.ShowReconnectConfirmDialog(this.nextStateName);
        }

        /// <summary>
        /// 接続成功
        /// </summary>
        public override void OnConnected()
        {
            this.manager.ChangeState<GetLobyInfoState>();
        }
    }

    /// <summary>
    /// ロビー情報取得ステート
    /// </summary>
    private class GetLobyInfoState : MyState
    {
        /// <summary>
        /// 接続切断時
        /// </summary>
        public override void OnDisconnected(DisconnectCause cause)
        {
            this.ShowReconnectConfirmDialog(typeof(GetLobyInfoState).Name);
        }

        /// <summary>
        /// ロビー情報取得成功時
        /// </summary>
        public override void OnLobbyStatisticsUpdate(List<TypedLobbyInfo> lobbyStatistics)
        {
            for (int i = 0; i < this.scene.lobbyInfos.Length; i++)
            {
                var lobbyName = this.scene.lobbyInfos[i].worldId.ToString();

                this.scene.lobbyInfos[i].typedLobbyInfo = lobbyStatistics.Find(x => x.Name == lobbyName);

                if (this.scene.lobbyInfos[i].typedLobbyInfo == null)
                {
                    this.scene.lobbyInfos[i].typedLobbyInfo = new TypedLobbyInfo
                    {
                        Name = lobbyName,
                        Type = LobbyType.Default,
                    };
                }
            }

            //ロビーステートへ
            this.manager.ChangeState<LobbyState>();
        }
    }

    /// <summary>
    /// ロビーステート
    /// </summary>
    private class LobbyState : MyState
    {
        public override void Start()
        {
            // 接続成功後、画面ロード
            this.Load();
        }

        /// <summary>
        /// ロード
        /// </summary>
        private void Load()
        {
            //ワールドID順に並び変えておく
            this.scene.response.tMultiWorld = this.scene.response.tMultiWorld.OrderBy(x => x.multiWorldId).ToArray();
            this.scene.response.tMultiJackpot = this.scene.response.tMultiJackpot.OrderBy(x => x.worldId).ToArray();
            this.scene.response.tMultiSoulBall = this.scene.response.tMultiSoulBall.OrderBy(x => x.worldId).ToArray();

            //ワールドデータ
            this.scene.worldData = new WorldData[this.scene.response.tMultiWorld.Length];

            for (int i = 0; i < this.scene.worldData.Length; i++)
            {
                uint worldId = this.scene.response.tMultiWorld[i].multiWorldId;
                this.scene.worldData[i] = new WorldData();

                this.scene.worldData[i].worldServerData = this.scene.response.tMultiWorld[i];
                this.scene.worldData[i].worldMasterData = Masters.MultiWorldDB.FindById(worldId);
                this.scene.worldData[i].worldJackpotInfoData = this.scene.response.tMultiJackpot[i];
                // tMultiSoulBallの配列の長さチェック
                if(i < this.scene.response.tMultiSoulBall.Length)
                {
                    this.scene.worldData[i].multiSoulBallData = this.scene.response.tMultiSoulBall[i];
                }
            }
            for (int i = 0; i < this.scene.worldData.Length; i++)
            {
                if (this.scene.worldData[i].worldMasterData.isComingSoon > 0)
                {
                    //ComingSoonワールドの場合、ComingSoonじゃないワールドの背景を利用する
                    var prev = this.scene.worldData.Last(x => x.worldMasterData.isComingSoon == 0);
                    this.scene.worldData[i].bgAssetLoader = prev.bgAssetLoader;
                }
                else
                {
                    //背景の読み込み
                    string bgPath = SharkDefine.GetStageSelectBgSpritePath(this.scene.worldData[i].worldMasterData.key);
                    this.scene.loader.Add(this.scene.worldData[i].bgAssetLoader = new AssetLoader<Sprite>(bgPath));
                }
            }

            //ロード実行
            this.scene.loader.Load(this.OnLoaded);
        }

        /// <summary>
        /// ロード完了時
        /// </summary>
        private void OnLoaded()
        {
            SoundManager.Instance.PlayBgm(BgmName.SELECT);

            //ワールド切り替え時コールバック設定
            this.scene.worldScrollView.onPageChange = this.OnWorldChange;

            //ワールドスクロールビュー初期化
            this.scene.worldScrollView.Initialize(
                this.scene.worldPanelPrefab.gameObject,
                this.scene.worldData.Length,
                this.OnUpdateWorldPanel
            );

            // ワールドパンネルロード後、ChangeAnimation終了
            SharedUI.Instance.HideSceneChangeAnimation();
        }

        /// <summary>
        /// ワールドパネル更新時
        /// </summary>
        private void OnUpdateWorldPanel(GameObject gobj, int count)
        {
            var panel = gobj.GetComponent<MultiWorldPanel>();
            panel.Set(this.scene.worldData[count]);
        }

        /// <summary>
        /// ワールド切り替え時
        /// </summary>
        private void OnWorldChange(int pageNo)
        {
            //スクロールビューの範囲内に収める
            pageNo = Mathf.Clamp(pageNo, 0, this.scene.worldData.Length - 1);

            //現在フォーカス中のワールドと異なっているかチェック
            if (this.scene.focusedWorldData == null || this.scene.worldData[pageNo] != this.scene.focusedWorldData)
            {
                //フォーカス中ワールドデータの切り替え
                this.scene.focusedWorldData = this.scene.worldData[pageNo];

                //フォーカス中ワールドのステージ情報でスクロールビューを構築
                if (this.scene.focusedWorldData.worldMasterData.isComingSoon > 0)
                {
                    //ワールド情報パンネルOFF
                    this.scene.infoPanelGameObject.SetActive(false);
                }
                else
                {
                    var localizeText = Masters.LocalizeTextDB;

                    //ワールド情報パンネルON
                    this.scene.infoPanelGameObject.SetActive(true);
                    
                    // メイン情報パンネルセット
                    // ワールド名
                    this.scene.worldNameText.text = this.scene.focusedWorldData.worldMasterData.name;

                    // フォーカスされた、ワルドID                    
                    var worldId = this.scene.focusedWorldData.worldMasterData.id;
                    //フォーカスされた、ワールドのボール・ソウル情報修得
                    uint ball = this.scene.response.tMultiSoulBall
                        .Where(x => x.worldId == worldId)
                        .Select(x => x.ball)
                        .FirstOrDefault();
                    uint soul = this.scene.response.tMultiSoulBall
                        .Where(x => x.worldId == worldId)
                        .Select(x => x.soul)
                        .FirstOrDefault();

                    // ballが9の場合は、何も表示しないために
                    if (ball == 9)
                    {
                        ball = 0;
                    }
                    // soulが9の場合は、何も表示しないために
                    if (soul == 9)
                    {
                        soul = 0;
                    }

                    // 玉をセット
                    if (ball == 0)
                    {
                        this.scene.ballImage.gameObject.SetActive(false);
                        this.scene.ballCount.text = null;
                    }
                    else
                    {
                        this.scene.ballImage.gameObject.SetActive(true);
                        this.scene.ballCount.text = ball.ToString();
                    }

                    // 魂をセット
                    var atlas = SharedUI.Instance.commonAtlas;
                    uint endNum = soul / 3;
                    uint remainder = soul % 3;
                    
                    for (int i = 0; i <this.scene.soulImage.Length; i++)
                    {
                        var img = this.scene.soulImage[i];
                        if (i < remainder)
                        {
                            img.gameObject.SetActive(true);
                            string endNumFormat = string.Format("{0:D2}",(endNum + 1));
                            img.sprite = atlas.GetSprite("Soul_" + endNumFormat);
                        }
                        else
                        {
                            if(endNum == 0)
                            {
                                img.gameObject.SetActive(false);
                            }
                            else
                            {
                                img.gameObject.SetActive(true);
                                string endNumFormat = string.Format("{0:D2}",endNum);
                                img.sprite = atlas.GetSprite("Soul_" + endNumFormat);
                            }
                        }
                    }

                    // 獲得期待倍率
                    // TODO. テストデータ
                    var min = this.scene.focusedWorldData.worldMasterData.minBet;
                    var max = this.scene.focusedWorldData.worldMasterData.maxBet;
                    this.scene.multipleText.text = localizeText.GetFormat("RateRange", min, max);

                    // 接続者数
                    this.scene.totalPlayerNumText.text = this.scene.lobbyInfos[pageNo].typedLobbyInfo.PlayerCount.ToString();
                    // ベッティングコイン(弾丸1発あたりの価格)
                    this.scene.bettingText.text = localizeText.GetFormat("UnitCoin", this.scene.focusedWorldData.worldMasterData.betCoin);

                    // ボタングレーアウトON/OFF
                    this.SetGrayout(!this.scene.focusedWorldData.worldServerData.IsOpen());
                }

                //左右矢印の表示切り替え
                this.scene.leftArrow.SetActive(pageNo > 0);
                this.scene.rightArrow.SetActive(pageNo < this.scene.worldData.Length - 1);
            }
        }

        /// <summary>
        /// ボタングレーアウトON/OFF
        /// </summary>
        private void SetGrayout(bool isGrayout)
        {
            foreach (var graphic in this.scene.playButtonGraphic)
            {
                graphic.material = isGrayout ? SharedUI.Instance.grayScaleMaterial : null;
            }

            this.scene.playButton.enabled = !isGrayout;
        }

        /// <summary>
        /// 挑戦するボタンクリック時
        /// </summary>
        public void OnClickPlayButton()
        {
            if (!this.scene.focusedWorldData.worldServerData.IsOpen())
            {
                Debug.LogWarning("空いてない");
                return;
            }

            //選択中ワールドのID
            uint worldId = this.scene.focusedWorldData.worldMasterData.id;

            var lobbyInfo = this.scene.lobbyInfos.FirstOrDefault(x => x.worldId == worldId);
            if (lobbyInfo == null)
            {
                Debug.LogWarningFormat("worldId={0}に一致するロビー情報が見つからない", worldId);
                return;
            }

            //マルチバトルシーンへ
            SceneChanger.ChangeSceneAsync("MultiBattle", new Battle.MultiBattleScene.SceneDataPack{
                lobyInfo = lobbyInfo,
                soulBall = this.scene.response.tMultiSoulBall.FirstOrDefault(x => x.worldId == worldId)
            });

            //Noneステートへ
            this.manager.ChangeState<NoneState>();
        }

        /// <summary>
        /// 接続失敗
        /// </summary>
        public override void OnDisconnected(DisconnectCause cause)
        {
            Debug.Log("OnDisconnected");
        }
    }
}
