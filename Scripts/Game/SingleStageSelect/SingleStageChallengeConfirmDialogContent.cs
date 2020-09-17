using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// シングルステージ挑戦確認ダイアログ内容
/// </summary>
public class SingleStageChallengeConfirmDialogContent : MonoBehaviour
{
    /// <summary>
    /// 報酬データ
    /// </summary>
    private class RewardData : SinglePlayApi.RewardData
    {
        public bool isFirstReward;
    }

    /// <summary>
    /// ワールド名、ステージ名エリア
    /// </summary>
    [SerializeField]
    private RectTransform nameArea = null;
    /// <summary>
    /// ワールド名テキスト
    /// </summary>
    [SerializeField]
    private Text worldNameText = null;
    /// <summary>
    /// ステージ名テキスト
    /// </summary>
    [SerializeField]
    private Text stageNameText = null;
    /// <summary>
    /// 星マーク
    /// </summary>
    [SerializeField]
    private Image[] star = null;
    /// <summary>
    /// 報酬アイコンプレハブ
    /// </summary>
    [SerializeField]
    private SingleBattleResultRewardIcon rewardIconPrefab = null;
    /// <summary>
    /// 非スクロール時報酬アイコン生成先
    /// </summary>
    [SerializeField]
    private HorizontalLayoutGroup nonScrollRewardArea = null;
    /// <summary>
    /// 報酬アイコンスクロールビュー
    /// </summary>
    [SerializeField]
    private InfiniteScrollView rewardScrollView = null;
    /// <summary>
    /// 必要コイン数表示エリア
    /// </summary>
    [SerializeField]
    private RectTransform coinArea = null;
    /// <summary>
    /// 必要コイン数テキスト
    /// </summary>
    [SerializeField]
    private Text needCoinNumText = null;
    /// <summary>
    /// 図鑑ダイアログ内容プレハブ
    /// </summary>
    [SerializeField]
    private SingleStageFishDictionaryDialogContent fishDictionaryDialogContent = null;

    /// <summary>
    /// ステージデータ
    /// </summary>
    private Master.SingleStageData stageData = null;
    /// <summary>
    /// クリア済みステージかどうか
    /// </summary>
    private bool isCleared = false;
    /// <summary>
    /// 自身のダイアログ
    /// </summary>
    private SimpleDialog dialog = null;
    /// <summary>
    /// 報酬リスト
    /// </summary>
    private List<RewardData> rewards = new List<RewardData>();
    /// <summary>
    /// ローダー
    /// </summary>
    private AssetListLoader loader = null;
    /// <summary>
    /// ステージ開始時
    /// </summary>
    private Action<uint> onStageStart = null;

    /// <summary>
    /// 開く
    /// </summary>
    public static void Open(
        SingleStageChallengeConfirmDialogContent prefab,
        Master.SingleStageData stageData,
        Rank rank,
        Action<uint> onStageStart)
    {
        bool isCleared = rank > Rank.None;

        //報酬リスト
        List<RewardData> rewards = new List<RewardData>();

        //このステージで入手可能な報酬一覧
        var lotDatas = Masters.SingleStageRewardDB
            .GetList()
            .Where(x => x.groupId == stageData.rewardGroupId)
            .SelectMany(x1 => Masters.SingleStageRewardLotDB.GetList().FindAll(x2 => x2.lotGroupId == x1.lotGroupId))
            .ToArray();
        foreach (var data in lotDatas)
        {
            if (!rewards.Exists(x => x.itemType == data.itemType && x.itemId == data.itemId))
            {
                rewards.Add(new RewardData{ itemType = data.itemType, itemId = data.itemId, itemNum = data.itemNum });
            }
        }

        //初回報酬
        var firstReward = Masters.SingleStageFirstRewardDB
            .GetList()
            .Where(x => x.groupId == stageData.rewardFirstGroupId)
            .Select(x => new RewardData{ itemType = x.itemType, itemId = x.itemId, itemNum = x.amount, isFirstReward = true });

        if (isCleared)
        {
            //初回報酬入手済みなら末尾に追加
            rewards.AddRange(firstReward);
        }
        else
        {
            //初回報酬未入手なら先頭に追加
            rewards.InsertRange(0, firstReward);
        }

        //ローダー準備
        var loader = new AssetListLoader(rewards
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

            //ダイアログ開く
            var dialog = SharedUI.Instance.ShowSimpleDialog();
            var content = dialog.AddContent(prefab);
            content.Set(dialog, stageData, rank, rewards, loader, onStageStart);
        });
    }

    /// <summary>
    /// 内容構築
    /// </summary>
    private void Set(
        SimpleDialog dialog,
        Master.SingleStageData stageData,
        Rank rank,
        List<RewardData> rewards,
        AssetListLoader loader,
        Action<uint> onStageStart)
    {
        this.dialog = dialog;
        this.stageData = stageData;
        this.isCleared = rank > Rank.None;
        this.rewards = rewards;
        this.loader = loader;
        this.onStageStart = onStageStart;
        var worldData = Masters.SingleWorldDB.FindById(this.stageData.worldId);

        //ワールド名、ステージ名
        this.worldNameText.text = worldData.name;
        this.stageNameText.text = this.stageData.name;
        LayoutRebuilder.ForceRebuildLayoutImmediate(this.nameArea);

        //星マーク
        for (int i = 0; i < this.star.Length; i++)
        {
            this.star[i].enabled = ((int)rank - 2) > i;
        }

        //報酬データが8個以下なら
        if (this.rewards.Count < 9)
        {
            //非スクロールエリアに報酬アイコンを生成する
            this.nonScrollRewardArea.gameObject.SetActive(true);
            this.rewardScrollView.gameObject.SetActive(false);

            for (int i = 0; i < this.rewards.Count; i++)
            {
                var icon = Instantiate(this.rewardIconPrefab, this.nonScrollRewardArea.transform, false);
                this.OnUpdateRewardIconView(icon.gameObject, i);
            }
        }
        //報酬データが9個以上なら
        else
        {
            //スクロールエリアに報酬アイコンを生成する
            this.nonScrollRewardArea.gameObject.SetActive(false);
            this.rewardScrollView.gameObject.SetActive(true);
            this.rewardScrollView.Initialize(this.rewardIconPrefab.gameObject, this.rewards.Count, this.OnUpdateRewardIconView);
        }

        //必要コイン数
        this.needCoinNumText.text = this.stageData.needCoin.ToString();
        LayoutRebuilder.ForceRebuildLayoutImmediate(this.coinArea);

        //YesNoボタン追加：挑戦する、キャンセル
        var btnGroup = this.dialog.AddButton(1);
        var challengeButton = btnGroup.buttons[0];
        challengeButton.text.text = Masters.LocalizeTextDB.Get("Challenge");
        challengeButton.image.sprite = SharedUI.Instance.commonAtlas.GetSprite(SharkDefine.NO_BTN_SPRITE_NAME);
        challengeButton.onClick = this.OnClickChallengeButton;

        //必要コイン数に満たない場合
        var userData = UserData.Get();
        if (userData == null || userData.coin < this.stageData.needCoin)
        {
            //グレースケールにして挑戦するボタンを押せなくする
            challengeButton.button.interactable = false;
            challengeButton.image.material =
            challengeButton.text.material = SharedUI.Instance.grayScaleMaterial;
        }

        //Closeボタン追加
        this.dialog.closeButtonEnabled = true;
    }

    /// <summary>
    /// OnDestroy
    /// </summary>
    private void OnDestroy()
    {
        this.loader.Unload();
        this.loader.Clear();
    }

    /// <summary>
    /// 報酬アイコン表示構築
    /// </summary>
    private void OnUpdateRewardIconView(GameObject gobj, int i)
    {
        var icon = gobj.GetComponent<SingleBattleResultRewardIcon>();
        bool isFirstReward = this.rewards[i].isFirstReward;
        bool isGrayout = isFirstReward && this.isCleared;
        icon.Setup(this.rewards[i], isFirstReward);
        icon.icon.SetGrayScale(isGrayout);
    }

    /// <summary>
    /// Infoボタンクリック時
    /// </summary>
    public void OnClickInfoButton()
    {
        //魚図鑑開く
        SingleStageFishDictionaryDialogContent.Open(this.fishDictionaryDialogContent, this.stageData);
    }

    /// <summary>
    /// 挑戦するボタンクリック時
    /// </summary>
    private void OnClickChallengeButton()
    {
        //ステージ開始通信
        SinglePlayApi.CallStartApi(this.stageData.id, () =>
        {
            this.dialog.onClose = () => this.onStageStart?.Invoke(this.stageData.id);
            this.dialog.Close();
        });
    }
}
