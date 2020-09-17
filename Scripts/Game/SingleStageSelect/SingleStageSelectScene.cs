using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// シングルステージセレクトシーン
/// </summary>
public class SingleStageSelectScene : SceneBase
{
    /// <summary>
    /// 最新ステージID
    /// </summary>
    private static uint latestStageId = 0;

    /// <summary>
    /// ワールドデータ
    /// </summary>
    public class WorldData
    {
        public Master.SingleWorldData worldMasterData = null;
        public Master.SingleStageData[] stageMasterData = null;
        public SinglePlayApi.TSingleWorld worldServerData = null;
        public SinglePlayApi.TSingleStage[] stageServerData = null;
        public IAssetLoader bgAssetLoader = null;
    }

    /// <summary>
    /// ワールドパネルプレハブ
    /// </summary>
    [SerializeField]
    private SingleWorldPanel worldPanelPrefab = null;
    /// <summary>
    /// ワールドスクロールビュー
    /// </summary>
    [SerializeField]
    private InfiniteScrollView worldScrollView = null;
    /// <summary>
    /// ステージパネルプレハブ
    /// </summary>
    [SerializeField]
    private SingleStagePanel stagePanelPrefab = null;
    /// <summary>
    /// ステージスクロールビュー
    /// </summary>
    [SerializeField]
    private InfiniteScrollView stageScrollView = null;
    /// <summary>
    /// ワールド名プレート
    /// </summary>
    [SerializeField]
    private GameObject worldNamePlate = null;
    /// <summary>
    /// ワールド名テキスト
    /// </summary>
    [SerializeField]
    private Text worldNameText = null;
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
    /// <summary>
    /// ステージ挑戦確認ダイアログ内容プレハブ
    /// </summary>
    [SerializeField]
    private SingleStageChallengeConfirmDialogContent confirmDialogContent = null;

    /// <summary>
    /// シングルプレイトップの通信レスポンス
    /// </summary>
    private SinglePlayApi.TopResponseData response = null;
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

    /// <summary>
    /// Awake
    /// </summary>
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
        //通信でワールドやステージの進行状況を取得
        SinglePlayApi.CallTopApi((response) =>
        {
            this.response = response;
            this.Load();
        });
    }

    /// <summary>
    /// ロード
    /// </summary>
    private void Load()
    {
        //ワールドID順に並び変えておく
        this.response.tSingleWorld = this.response.tSingleWorld.OrderBy(x => x.worldId).ToArray();

        //ワールドデータ
        this.worldData = new WorldData[this.response.tSingleWorld.Length];

        for (int i = 0; i < this.worldData.Length; i++)
        {
            uint worldId = this.response.tSingleWorld[i].worldId;
            this.worldData[i] = new WorldData();
            this.worldData[i].worldServerData = this.response.tSingleWorld[i];
            this.worldData[i].worldMasterData = Masters.SingleWorldDB.FindById(worldId);
            this.worldData[i].stageMasterData = Masters.SingleStageDB.GetList().Where(x => x.worldId == worldId).ToArray();
        }

        for (int i = 0; i < this.worldData.Length; i++)
        {
            /*uint worldId = this.response.tSingleWorld[i].worldId;
            this.worldData[i] = new WorldData();
            this.worldData[i].worldServerData = this.response.tSingleWorld[i];
            this.worldData[i].worldMasterData = Masters.SingleWorldDB.FindById(worldId);
            this.worldData[i].stageMasterData = Masters.SingleStageDB.GetList().Where(x => x.worldId == worldId).ToArray();*/
            this.worldData[i].stageServerData = this.worldData[i].stageMasterData
                .Select(stageMaster =>
                {
                    var result = this.response.tSingleStage.FirstOrDefault(stageServer => stageServer.stageId == stageMaster.id);
                    if (result == null)
                    {
                        result = new SinglePlayApi.TSingleStage();
                        result.stageId = stageMaster.id;
                        result.stageStatus = (uint)SinglePlayApi.Status.NotOpen;
                        result.clearRank = (uint)Rank.None;
                    }
                    else if (stageMaster.type == (uint)Master.SingleStageData.StageType.Battle && result.IsOpen())
                    {
                        this.loader.Add<Sprite>(SharkDefine.GetSingleStageIconSpritePath(stageMaster.key));
                    }
                    return result;
                })
                .ToArray();

            if (this.worldData[i].worldMasterData.isComingSoon > 0)
            {
                //ComingSoonワールドの場合、ComingSoonじゃないワールドの背景を利用する
                var prev = this.worldData.Last(x => x.worldMasterData.isComingSoon == 0);
                this.worldData[i].bgAssetLoader = prev.bgAssetLoader;
            }
            else
            {
                //背景の読み込み
                string bgPath = SharkDefine.GetStageSelectBgSpritePath(this.worldData[i].worldMasterData.key);
                this.loader.Add(this.worldData[i].bgAssetLoader = new AssetLoader<Sprite>(bgPath));
            }
        }

        //ロード実行
        this.loader.Load(this.OnLoaded);
    }

    /// <summary>
    /// ロード完了時
    /// </summary>
    private void OnLoaded()
    {
        //ローディング表示消す
        SharedUI.Instance.HideSceneChangeAnimation();

        SoundManager.Instance.PlayBgm(BgmName.SELECT);

        //ワールド切り替え時コールバック設定
        this.worldScrollView.onPageChange = this.OnWorldChange;

        //ワールドスクロールビュー初期化
        this.worldScrollView.Initialize(
            this.worldPanelPrefab.gameObject,
            this.worldData.Length,
            this.OnUpdateWorldPanel
        );

        if (latestStageId > 0)
        {
            //最新ステージのワールドにフォーカス
            uint worldId = Masters.SingleStageDB.FindById(latestStageId).worldId;
            int worldElementId = Array.FindIndex(this.worldData, x => x.worldMasterData.id == worldId);
            this.worldScrollView.SetFocus(worldElementId);

            //最新ステージにフォーカス
            int stageElementId = Array.FindIndex(this.worldData[worldElementId].stageMasterData, x => x.id == latestStageId);
            this.stageScrollView.SetFocus(stageElementId);
        }
    }

    /// <summary>
    /// ワールドパネル更新時
    /// </summary>
    private void OnUpdateWorldPanel(GameObject gobj, int count)
    {
        var panel = gobj.GetComponent<SingleWorldPanel>();
        panel.Set(this.worldData[count]);
    }

    /// <summary>
    /// ワールド切り替え時
    /// </summary>
    private void OnWorldChange(int pageNo)
    {
        //スクロールビューの範囲内に収める
        pageNo = Mathf.Clamp(pageNo, 0, this.worldData.Length - 1);

        //現在フォーカス中のワールドと異なっているかチェック
        if (this.focusedWorldData == null || this.worldData[pageNo] != this.focusedWorldData)
        {
            //フォーカス中ワールドデータの切り替え
            this.focusedWorldData = this.worldData[pageNo];

            //フォーカス中ワールドのステージ情報でスクロールビューを構築
            if (this.focusedWorldData.worldMasterData.isComingSoon > 0)
            {
                //ワールド名プレート表示OFF
                this.worldNamePlate.SetActive(false);

                this.stageScrollView.Initialize(
                    this.stagePanelPrefab.gameObject,
                    0,
                    null
                );
            }
            else
            {
                //ワールド名プレート表示ON
                this.worldNamePlate.SetActive(true);
                this.worldNameText.text = this.focusedWorldData.worldMasterData.name;

                this.stageScrollView.Initialize(
                    this.stagePanelPrefab.gameObject,
                    this.focusedWorldData.stageMasterData.Length,
                    this.OnUpdateStagePanel
                );
            }

            //左右矢印の表示切り替え
            this.leftArrow.SetActive(pageNo > 0);
            this.rightArrow.SetActive(pageNo < this.worldData.Length - 1);
        }
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
    /// ステージパネル更新時
    /// </summary>
    private void OnUpdateStagePanel(GameObject gobj, int count)
    {
        var master = this.focusedWorldData.stageMasterData[count];
        var server = this.focusedWorldData.stageServerData[count];
        int battleStageIndex = this.GetBattleStageIndex(this.focusedWorldData, master);
        bool isLast = count == this.focusedWorldData.stageServerData.Length - 1;

        var panel = gobj.GetComponent<SingleStagePanel>();
        panel.Set(master, server, battleStageIndex, isLast, this.OnClickStagePanel);
    }

    /// <summary>
    /// ステージパネルクリック時
    /// </summary>
    private void OnClickStagePanel(SingleStagePanel stagePanel)
    {
        SoundManager.Instance.PlaySe(SeName.YES);

        if (stagePanel.isStory)
        {
            //TODO: ストーリーステージの場合の処理
            return;
        }

        //ダイアログ開く
        SingleStageChallengeConfirmDialogContent.Open(
            this.confirmDialogContent,
            stagePanel.master,
            (Rank)stagePanel.server.clearRank,
            (stageId) =>
            {
                //シングルバトルへ
                latestStageId = stageId;
                SceneChanger.ChangeSceneAsync("SingleBattle", new ToBattleSceneDataPack{ stageId = stageId });
            });
    }

    /// <summary>
    /// ワールド内で何番目のバトルステージかを取得
    /// </summary>
    private int GetBattleStageIndex(WorldData worldData, Master.SingleStageData battleStageData)
    {
        for (int i = 0, j = 0; i < worldData.stageMasterData.Length; i++)
        {
            if (worldData.stageMasterData[i].type == (int)Master.SingleStageData.StageType.Battle)
            {
                if (worldData.stageMasterData[i] == battleStageData)
                {
                    return j;
                }
                j++;
            }
        }
        return -1;
    }
}
