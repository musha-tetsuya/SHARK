using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ミッションダイアログスクロールビュー要素
/// </summary>
public class MissionContent : MonoBehaviour
{
    /// <summary>
    /// アイテムタイプとアイコンスプライトの関係
    /// </summary>
    [Serializable]
    public struct ItemTypeToIconSprite
    {
        /// <summary>
        /// アイテムタイプ
        /// </summary>
        [SerializeField]
        public ItemType itemType;
        /// <summary>
        /// アイコンスプライト
        /// </summary>
        [SerializeField]
        public Sprite sprite;
    }

    /// <summary>
    /// ミッションの名称のText
    /// </summary>
    [SerializeField]
    private Text missionNameText = null;
    /// <summary>
    /// アイコンイメージ
    /// </summary>
    [SerializeField]
    public Image iconImage = null;
    /// <summary>
    /// アイテムの名称のText
    /// </summary>
    [SerializeField]
    private Text itemNameText = null;
    /// <summary>
    /// アイテムの個数Text
    /// </summary>
    [SerializeField]
    private Text itemCountText = null;
    /// <summary>
    /// 受け取り期限のコンテンツ
    /// </summary>
    [SerializeField]
    private GameObject limitDateContent = null;
    /// <summary>
    /// 受け取り期限のText
    /// </summary>
    [SerializeField]
    private Text limitDateText = null;
    /// <summary>
    /// 進捗表示のバーの画像
    /// </summary>
    [SerializeField]
    private Image progressBarImage = null;
    /// <summary>
    /// 進捗表示のText
    /// </summary>
    [SerializeField]
    private Text progressText = null;
    /// <summary>
    /// 個別受け取りのボタン
    /// </summary>
    [SerializeField]
    private Button receiveButton = null;
    /// <summary>
    /// 挑戦のボタン
    /// </summary>
    [SerializeField]
    private Button challengeButton = null;
    /// <summary>
    /// 受け取り済みのText
    /// </summary>
    [SerializeField]
    private Text receivedText = null;
    /// <summary>
    /// アイテムタイプとアイコンスプライトの関係
    /// </summary>
    [SerializeField]
    private ItemTypeToIconSprite[] itemTypeToIconSprites = null;

    /// <summary>
    /// サーバーデータ
    /// </summary>
    public MissionApi.MissionProgress server { get; private set; }
    /// <summary>
    /// 挑戦するボタンクリック時コールバック
    /// </summary>
    private Action<MissionContent> onClickChallengeButton = null;
    /// <summary>
    /// 受け取りボタンクリック時コールバック
    /// </summary>
    private Action<MissionContent> onClickReceiveButton = null;

    /// <summary>
    /// 表示構築
    /// </summary>
    public void BuildView(
        MissionApi.MissionProgress server,
        Action<MissionContent> onClickChallengeButton,
        Action<MissionContent> onClickReceiveButton)
    {
        this.server = server;
        this.onClickChallengeButton = onClickChallengeButton;
        this.onClickReceiveButton = onClickReceiveButton;
        var rewardData = Masters.MissionRewardDB.FindById(this.server.missionRewardId);
        var rewardItemInfo = CommonIconUtility.GetItemInfo(rewardData.itemType, rewardData.itemId);

        //ミッション名
        this.missionNameText.text = Masters.MissionTypeDB.FindById(this.server.missionTypeId).missionName;

        switch (this.server.missionTypeId)
        {
            //シングルモードステージ△△を〇〇回クリアする
            case (uint)Master.MissionData.MainType.SingleSelectStageClear:
                var stageName = Masters.SingleStageDB.FindById(this.server.missionTypeSubId.Value).name;
                this.missionNameText.text = string.Format(this.missionNameText.text, this.server.clearCondition, stageName);
                break;

            //マルチモードでバトルアイテム△△を〇〇個使用する
            case (uint)Master.MissionData.MainType.MultiConsumSelectItem:
                var itemName = Masters.BattleItemDB.FindById(this.server.missionTypeSubId.Value).name;
                this.missionNameText.text = string.Format(this.missionNameText.text, this.server.clearCondition, itemName);
                break;

            //マルチモードで特定の魚△△を〇〇匹捕まえる
            case (uint)Master.MissionData.MainType.MultiCatchSelectFish:
                var fishName = Masters.FishDB.FindById(this.server.missionTypeSubId.Value).name;
                this.missionNameText.text = string.Format(this.missionNameText.text, this.server.clearCondition, fishName);
                break;

            default:
                this.missionNameText.text = string.Format(this.missionNameText.text, this.server.clearCondition);
                break;
        }

        //アイコンイメージ
        this.iconImage.sprite = this.itemTypeToIconSprites.First(x => (uint)x.itemType == rewardData.itemType).sprite;

        //アイテム名テキスト
        this.itemNameText.text = rewardItemInfo.GetName();

        //アイテム個数テキスト
        this.itemCountText.text = rewardData.itemNum.ToString("#,0");

        //通算ミッション以外は期限表示有り
        this.limitDateContent.SetActive(this.server.category != MissionApi.Category.Total);

        if (this.limitDateContent.activeSelf)
        {
            //残り時間表示
            var span = TimeSpan.FromSeconds(this.server.endTime ?? 0);
            this.limitDateText.text = (span.Days > 0)  ? Masters.LocalizeTextDB.GetFormat("ReceiveLimitedToDay", span.Days)
                                    : (span.Hours > 0) ? Masters.LocalizeTextDB.GetFormat("ReceiveLimitedToHour", span.Hours)
                                    :                    Masters.LocalizeTextDB.GetFormat("ReceiveLimitedToMinites", span.Minutes);
        }

        //進捗ゲージ表示
        if (this.server.status == MissionApi.Status.NotClear)
        {
            this.progressText.text = string.Format("{0}/{1}", this.server.missionCount, this.server.clearCondition);
            this.progressBarImage.color = Color.blue;
            this.progressBarImage.fillAmount = (float)this.server.missionCount / this.server.clearCondition;
        }
        else
        {
            this.progressText.text = Masters.LocalizeTextDB.Get("MissionAchieved");
            this.progressBarImage.color = Color.red;
            this.progressBarImage.fillAmount = 1.0f;
        }

        //挑戦ボタンの表示切り替え
        this.challengeButton.gameObject.SetActive(this.server.status == MissionApi.Status.NotClear);

        //受け取りボタンの表示切り替え
        this.receiveButton.gameObject.SetActive(this.server.status == MissionApi.Status.ClearNotReceived);

        //受け取り済みテキストの表示切り替え
        this.receivedText.gameObject.SetActive(this.server.status == MissionApi.Status.ClearReceived);
    }

    /// <summary>
    /// 挑戦するのクリック時
    /// </summary>
    public void OnClickChallengeButton()
    {
        this.onClickChallengeButton?.Invoke(this);
    }

    /// <summary>
    /// 個別受け取りのクリック時
    /// </summary>
    public void OnClickReceiveButton()
    {
        this.onClickReceiveButton?.Invoke(this);
    }
}
