using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ミッションダイアログ
/// </summary>
public class MissionDialog : MonoBehaviour
{
    /// <summary>
    /// タブグループ
    /// </summary>
    [SerializeField]
    private TabGroup tabGroup = null;
    /// <summary>
    /// ミッションのスクロールビュー
    /// </summary>
    [SerializeField]
    private InfiniteScrollView missionScrollView = null;
    /// <summary>
    /// ミッションのコンテンツ
    /// </summary>
    [SerializeField]
    private MissionContent missionContent = null;
    /// <summary>
    /// アイテム受け取りのダイアログ
    /// </summary>
    [SerializeField]
    private MissionReceiveDialog missionReceiveDialog = null;
    /// <summary>
    /// 選択中のタブ
    /// </summary>
    [SerializeField]
    private MissionDialogTab selectedTab = null;

    /// <summary>
    /// ダイアログ
    /// </summary>
    private SimpleDialog dialog = null;
    /// <summary>
    /// ミッションリストレスポンスデータ
    /// </summary>
    private MissionApi.MissionProgressResponseData response = null;
    /// <summary>
    /// バトル中かどうかのフラグ
    /// </summary>
    private bool isBattle => SceneChanger.currentScene is Battle.MultiBattleScene;
    /// <summary>
    /// 遷移先シーン名
    /// </summary>
    public string nextSceneName { get; private set; }
    /// <summary>
    /// シーン遷移時データ
    /// </summary>
    public SceneDataPackBase dataPack { get; private set; }

    /// <summary>
    /// 開く
    /// </summary>
    public static void Open(MissionDialog prefab, Action<MissionDialog> onClose)
    {
        //ミッション一覧取得通信
        MissionApi.CallMissionProgressApi((response) =>
        {
            //ダイアログ開く
            var dialog = SharedUI.Instance.ShowSimpleDialog();
            dialog.titleText.text = Masters.LocalizeTextDB.Get("MissionTitle");
            dialog.closeButtonEnabled = true;

            var content = dialog.AddContent(prefab);
            content.BuildView(response, dialog);

            dialog.onClose += () => onClose?.Invoke(content);
        });
    }

    /// <summary>
    /// Awake
    /// </summary>
    private void Awake()
    {
        this.tabGroup.Setup();
        this.tabGroup.SetActiveTab(this.selectedTab);
    }

    /// <summary>
    /// 表示構築
    /// </summary>
    private void BuildView(MissionApi.MissionProgressResponseData response, SimpleDialog dialog)
    {
        this.response = response;
        this.dialog = dialog;

        for (int i = 0; i < this.tabGroup.tabList.Count; i++)
        {
            var tab = this.tabGroup.tabList[i] as MissionDialogTab;

            //各タブにミッションリストを割り当て
            var server = (tab.category == MissionApi.Category.StartDash) ? this.response.startDashMissionProgress
                       : (tab.category == MissionApi.Category.Daily)     ? this.response.dailyMission
                       : (tab.category == MissionApi.Category.Total)     ? this.response.totalMission
                       : (tab.category == MissionApi.Category.Event)     ? this.response.eventMissionProgress
                       : null;
            tab.Set(server);

            //ミッションが無いタブは非表示
            tab.gameObject.SetActive(tab.missionList.Length > 0);
        }

        //スクロールビュー構築
        this.missionScrollView.Initialize(this.missionContent.gameObject, this.selectedTab.missionList.Length, this.OnUpdateElement);
    }

    /// <summary>
    /// タブクリック時
    /// </summary>
    public void OnClickTab(MissionDialogTab tab)
    {
        SoundManager.Instance.PlaySe(SeName.YES);
        this.selectedTab = tab;
        this.missionScrollView.Initialize(this.missionContent.gameObject, this.selectedTab.missionList.Length, this.OnUpdateElement);
    }

    /// <summary>
    /// スクロールビュー要素更新時
    /// </summary>
    private void OnUpdateElement(GameObject gobj, int index)
    {
        var content = gobj.GetComponent<MissionContent>();
        content.BuildView(this.selectedTab.missionList[index], this.OnClickChallengeButton, this.OnClickReceiveButton);
    }

    /// <summary>
    /// 挑戦するのクリック時
    /// </summary>
    private void OnClickChallengeButton(MissionContent content)
    {
        switch ((Master.MissionData.MainType)content.server.missionTypeId)
        {
            case Master.MissionData.MainType.SingleGetSter:
            case Master.MissionData.MainType.SingleTotalStageClear:
            case Master.MissionData.MainType.SingleSelectStageClear:
                //シングルのステージ選択に遷移
                if (!HomeScene.isMaxPossession)
                {
                    this.nextSceneName = "SingleStageSelect";
                    this.dataPack = null;
                    this.dialog.Close();

                    if (!this.isBattle)
                    {
                        this.dialog.onClose += () => SceneChanger.ChangeSceneAsync("SingleStageSelect");
                    }
                }
                else
                {
                    HomeScene.OpenPresentBoxOverDialog();
                }
                break;
            case Master.MissionData.MainType.MultiGetCoin:
            case Master.MissionData.MainType.MultiConsumCoin:
            case Master.MissionData.MainType.MultiConsumTotalItem:
            case Master.MissionData.MainType.MultiConsumSelectItem:
            case Master.MissionData.MainType.MultiCatchTotalFish:
            case Master.MissionData.MainType.MultiCatchSelectFish:
            case Master.MissionData.MainType.MultiGetRyugyoku:
            case Master.MissionData.MainType.MultiPlaySlot:
            case Master.MissionData.MainType.MultiGetJP:
            case Master.MissionData.MainType.MultiPlayFVAttack:
                //マルチのステージ選択に遷移
                if (!HomeScene.isMaxPossession)
                {
                    this.nextSceneName = "MultiStageSelect";
                    this.dataPack = null;
                    this.dialog.Close();

                    if (!this.isBattle)
                    {
                        this.dialog.onClose += () => SceneChanger.ChangeSceneAsync("MultiStageSelect");
                    }
                }
                else
                {
                    HomeScene.OpenPresentBoxOverDialog();
                }
                break;
            case Master.MissionData.MainType.DecompositionGear:
                this.nextSceneName = "ItemInventory";
                this.dataPack = null;
                this.dialog.Close();

                if (!isBattle)
                {
                    this.dialog.onClose += () => SceneChanger.ChangeSceneAsync("ItemInventory");
                }
                break;
            case Master.MissionData.MainType.ReachLevel:
            case Master.MissionData.MainType.LoginTotal:
            case Master.MissionData.MainType.LoginSelect:
            case Master.MissionData.MainType.LinkedAccount:
                //HOMEに遷移
                this.nextSceneName = "Home";
                this.dataPack = null;
                this.dialog.Close();
                break;
            case Master.MissionData.MainType.GetTotalGear:
            case Master.MissionData.MainType.GetBatteryGear:
            case Master.MissionData.MainType.GetBarrelGear:
            case Master.MissionData.MainType.GetBulletGear:
            case Master.MissionData.MainType.GetTypesBattery:
                //ショップ(砲台関連のタブ)
                if (!HomeScene.isMaxPossession)
                {
                    this.nextSceneName = "Shop";
                    this.dataPack = new ToShopSceneDataPack { pageType = ShopScene.PageType.ToolGroup };
                    this.dialog.Close();

                    if (!this.isBattle)
                    {
                        this.dialog.onClose += () => SceneChanger.ChangeSceneAsync("Shop", this.dataPack);
                    }
                }
                else
                {
                    HomeScene.OpenPresentBoxOverDialog();
                }
                break;
            case Master.MissionData.MainType.ReachVIPRank:
                //ショップ(ジェムのタブ)
                if (!HomeScene.isMaxPossession)
                {
                    this.nextSceneName = "Shop";
                    this.dataPack = new ToShopSceneDataPack { pageType = ShopScene.PageType.Gem };
                    this.dialog.Close();

                    if (!isBattle)
                    {
                        this.dialog.onClose += () => SceneChanger.ChangeSceneAsync("Shop", this.dataPack);
                    }
                }
                else
                {
                    HomeScene.OpenPresentBoxOverDialog();
                }
                break;
        }
    }

    /// <summary>
    /// 個別受け取りのクリック時
    /// </summary>
    private void OnClickReceiveButton(MissionContent content)
    {
        //受け取り通信
        MissionApi.CallGetMissionRewardApi(content.server, (response) =>
        {
            //ダイアログ生成
            var dialog = SharedUI.Instance.ShowSimpleDialog();
            dialog.titleText.text = Masters.LocalizeTextDB.Get("ReceiveMissionTitle");
            dialog.closeButtonEnabled = true;

            var dialogContent = dialog.AddContent(this.missionReceiveDialog);
            dialogContent.BuildView(content);

            //ダイアログ閉じたら対象のミッションリストを更新
            dialog.onClose += () =>
            {
                switch (content.server.category)
                {
                    case MissionApi.Category.Total:
                        this.response.totalMission = response.totalMission;
                        this.selectedTab.Set(this.response.totalMission);
                        break;

                    case MissionApi.Category.Daily:
                        this.response.dailyMission = response.dailyMission;
                        this.selectedTab.Set(this.response.dailyMission);
                        break;

                    case MissionApi.Category.StartDash:
                        this.response.startDashMissionProgress = response.startDashMissionProgress;
                        this.selectedTab.Set(this.response.startDashMissionProgress);
                        break;

                    case MissionApi.Category.Event:
                        this.response.eventMissionProgress = response.eventMissionProgress;
                        this.selectedTab.Set(this.response.eventMissionProgress);
                        break;
                }

                this.missionScrollView.Initialize(this.missionContent.gameObject, this.selectedTab.missionList.Length, this.OnUpdateElement);
            };
        });
    }
}
