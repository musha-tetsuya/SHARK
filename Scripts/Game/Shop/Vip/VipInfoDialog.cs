using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class VipInfoDialog : MonoBehaviour
{
    /// <summary>
    /// Vipの経験値表示
    /// </summary>
    [SerializeField]
    private VipExpGauge vipExpGauge = null;
    /// <summary>
    /// 現在選択しているVipランク表示のテキスト
    /// </summary>
    [SerializeField]
    private Text openVipTitleText = null;
    /// <summary>
    /// チェックマークのGameObject
    /// </summary>
    [SerializeField]
    private GameObject checkMarkImage = null;
    /// <summary>
    /// 左矢印ボタン
    /// </summary>
    [SerializeField]
    private Button leftArrowButton = null;
    /// <summary>
    /// 左矢印ボタンの画像
    /// </summary>
    [SerializeField]
    private Image leftArrowButtonImage = null;
    /// <summary>
    /// 右矢印ボタン
    /// </summary>
    [SerializeField]
    private Button rightArrowButton = null;
    /// <summary>
    /// 右矢印ボタンの画像
    /// </summary>
    [SerializeField]
    private Image rightArrowButtonImage = null;
    /// <summary>
    /// 通常報酬のコンテンツ
    /// </summary>
    [SerializeField]
    private VipNormalRewardContent normalRewardContent = null;
    /// <summary>
    /// 通常報酬のスクロールビュー
    /// </summary>
    [SerializeField]
    private InfiniteScrollView normalRewardScrollView = null;
    /// <summary>
    /// 到達報酬のコンテンツ
    /// </summary>
    [SerializeField]
    private VipAchievementRewardContent achievementRewardContent = null;
    /// <summary>
    /// 到達報酬のスクロールビュー
    /// </summary>
    [SerializeField]
    private InfiniteScrollView achievementRewardScrollView = null;
    /// <summary>
    /// 受け取りボタン
    /// </summary>
    [SerializeField]
    private Button receiveButton = null;
    /// <summary>
    /// 受け取りボタンの画像
    /// </summary>
    [SerializeField]
    private Image receiveButtonImage = null;
    /// <summary>
    /// 受け取りボタンのテキスト
    /// </summary>
    [SerializeField]
    private Text receiveButtonText = null;

    /// <summary>
    /// アセットローダー
    /// </summary>
    private AssetListLoader assetLoader = new AssetListLoader();
    /// <summary>
    /// 現在開いているランク
    /// </summary>
    private int focusRank;
    /// <summary>
    /// 開くことのできるランクの上限
    /// </summary>
    private uint maxLevel;
    /// <summary>
    /// クリア済みのランク
    /// </summary>
    private uint currentRank;
    /// <summary>
    /// vipLevel, receiveFlg
    /// </summary>
    private List<VipApi.TVipReward> tVipReward = null;

    /// <summary>
    /// 通常報酬のリスト
    /// </summary>
    private List<Master.VipBenefitData> normalRewards = new List<Master.VipBenefitData>();
    /// <summary>
    /// 到達報酬のリスト
    /// </summary>
    private List<Master.VipRewardData> achievementRewards = new List<Master.VipRewardData>();

    /// <summary>
    /// OnDestroy
    /// </summary>
    private void OnDestroy()
    {
        this.assetLoader.Unload();
    }

    /// <summary>
    /// vip詳細開く
    /// </summary>
    public static void Open(VipInfoDialog prefab)
    {
        // 通信で、vipLevel, 補償取得の可否取得
        VipApi.CallVipLevelApi((response) =>
        {
            var loader = new AssetListLoader(Masters.VipRewardDB
            .GetList()
            .Select(x => CommonIconUtility.GetItemInfo(x.itemType, x.itemId))
            .Where(x => !x.IsCommonSprite())
            .Select(x => new AssetLoader<Sprite>(x.GetSpritePath()))
            );

            // タッチブロック
            SharedUI.Instance.DisableTouch();
            
            // 読み込み開始
            loader.Load(() =>
            {   
                //タッチブロック解除
                SharedUI.Instance.EnableTouch();
                // ロード後
                var dialog = SharedUI.Instance.ShowSimpleDialog();
                dialog.closeButtonEnabled = true;
                dialog.titleText.text = Masters.LocalizeTextDB.Get("VipInfoTitle");
                var content = dialog.AddContent(prefab);
                content.assetLoader = loader;
                content.Set(response);
            });
        });
    }

    /// <summary>
    /// 内容構築
    /// </summary>
    public void Set(VipApi.VipLevelCheckResponseData response)
    {
        List<Master.VipLevelData> vipLevels = Masters.VipLevelDB.GetList();
        // vipLevel, receiveFlg 修得
        this.tVipReward = response.tVipReward.ToList();
        // 現在自分のvip Level
        this.currentRank = response.tUsers.vipLevel;
        // ダイアログの補償情報確認レベル
        this.focusRank = (int)this.currentRank;
        // vipLevelの最大値
        this.maxLevel = vipLevels.Max(x => x.vipLevel);
        // 次のレベルアップまでに必要な経験値
        uint nextExp = vipLevels.Where(x => x.vipLevel == this.currentRank).Select(x => x.nextExp).FirstOrDefault();

        //経験値関連の設定
        this.vipExpGauge.SetRank(this.currentRank);
        this.vipExpGauge.SetExp(response.tUsers.vipExp, nextExp);

        //矢印ボタンの設定
        if (this.focusRank <= 1)
        {
            this.focusRank = 1;
            this.leftArrowButton.interactable = false;
            this.leftArrowButtonImage.material = SharedUI.Instance.grayScaleMaterial;
        }
        if (this.focusRank >= this.maxLevel)
        {
            this.focusRank = (int)this.maxLevel;
            this.rightArrowButton.interactable = false;
            this.rightArrowButtonImage.material = SharedUI.Instance.grayScaleMaterial;
        }

        OpenPage();
    }

    /// <summary>
    /// 選択されているランクのページを設定
    /// </summary>
    private void OpenPage()
    {
        openVipTitleText.text = Masters.LocalizeTextDB.GetFormat("VipOpenLevelTitle", this.focusRank);

        //　クリア時のチェックマークの表示設定
        checkMarkImage.SetActive(this.focusRank <= this.currentRank);
        // クリア状態による、受取ボタン表示・非表示
        this.receiveButton.gameObject.SetActive(this.focusRank <=this.currentRank);

        // 補償取得の可否
        bool receiveFlg = this.tVipReward.Where(x => x.vipLevel == this.focusRank).Select(x => x.receiveFlg == 0).FirstOrDefault();
        
        // 取得可能
        if (receiveFlg)
        {
            receiveButton.interactable = true;
            receiveButtonImage.material = null;
            receiveButtonText.text = Masters.LocalizeTextDB.Get("Receive");
        }
        // 取得不可能
        else
        {
            receiveButton.interactable = false;
            receiveButtonImage.material = SharedUI.Instance.grayScaleMaterial;
            receiveButtonText.text = Masters.LocalizeTextDB.Get("Received");
        }

        // リスト初期化
        normalRewards.Clear();
        achievementRewards.Clear();

        // 能力値系補償
        this.normalRewards = Masters.VipBenefitDB.GetList().Where(x => x.vipLevel == this.focusRank).ToList();
        // アイテム系補償
        this.achievementRewards = Masters.VipRewardDB.GetList().Where(x => x.vipLevel == this.focusRank).ToList();
        // スクロールビューセット
        this.normalRewardScrollView.Initialize(this.normalRewardContent.gameObject, this.normalRewards.Count, this.OnUpdateNormalContent);
        this.achievementRewardScrollView.Initialize(this.achievementRewardContent.gameObject, this.achievementRewards.Count, this.OnUpdateAchievementContent);
    }

    /// <summary>
    /// 通常報酬の更新
    /// </summary>
    private void OnUpdateNormalContent(GameObject gobj, int index)
    {
        Master.VipBenefitData benefit = normalRewards[index];
        var benefitTypeName = Masters.VipBenefitTypeDB.FindById(benefit.benefitTypeId).benefitTypeName;
        VipNormalRewardContent content = gobj.GetComponent<VipNormalRewardContent>();
        content.SetInfo(benefitTypeName);
    }

    /// <summary>
    /// 到達報酬の更新
    /// </summary>
    private void OnUpdateAchievementContent(GameObject gobj, int index)
    {
        Master.VipRewardData reward = achievementRewards[index];
        VipAchievementRewardContent content = gobj.GetComponent<VipAchievementRewardContent>();
        content.SetInfo(reward.itemType, reward.itemId, reward.itemNum);
    }

    /// <summary>
    /// 矢印ボタンが押された際に呼ばれるコールバック
    /// </summary>
    public void OnClickAllow(int addRank)
    {
        //ページのindexを更新する
        this.focusRank += addRank;

        //矢印ボタンの設定
        leftArrowButton.interactable = this.focusRank > 1;
        leftArrowButtonImage.material = leftArrowButton.interactable ? null : SharedUI.Instance.grayScaleMaterial;

        rightArrowButton.interactable = this.focusRank < this.maxLevel;
        rightArrowButtonImage.material = rightArrowButton.interactable ? null : SharedUI.Instance.grayScaleMaterial;

        //ページを開く
        OpenPage();
    }

    /// <summary>
    /// 到達報酬の受け取りが押された際に呼ばれるコールバック
    /// </summary>
    public void OnClickReceive()
    {
        VipApi.CallVipRewardGetApi((uint)this.focusRank
        , () =>
        {
            VipApi.CallVipLevelApi((response) =>
            {
                Set(response);
                var dialog = SharedUI.Instance.ShowSimpleDialog();
                var content = dialog.SetAsMessageDialog(Masters.LocalizeTextDB.Get("RewardVipAchievementReward"));
                content.buttonGroup.buttons[0].onClick = dialog.Close;
            });
        });
    }
}
