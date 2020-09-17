using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Battle {

/// <summary>
/// リザルトポップアップ内容
/// </summary>
public class SingleBattleResultPopupContent : MonoBehaviour
{
    /// <summary>
    /// 星マーク
    /// </summary>
    [SerializeField]
    private Image[] starImage = null;
    /// <summary>
    /// 報酬アイコンプレハブ
    /// </summary>
    [SerializeField]
    private SingleBattleResultRewardIcon rewardIconPrefab = null;
    /// <summary>
    /// 非スクロール時報酬アイコン生成先
    /// </summary>
    [SerializeField]
    private HorizontalLayoutGroup horizontalLayoutGroup = null;
    /// <summary>
    /// 報酬アイコンスクロールビュー
    /// </summary>
    [SerializeField]
    private InfiniteScrollView scrollView= null;

    /// <summary>
    /// 自身のダイアログ
    /// </summary>
    private SimpleDialog dialog = null;
    /// <summary>
    /// リザルト通信レスポンスデータ
    /// </summary>
    private SinglePlayApi.ClearResponseData response = null;
    /// <summary>
    /// 報酬一覧
    /// </summary>
    private SinglePlayApi.RewardData[] rewards = null;
    /// <summary>
    /// ローダー
    /// </summary>
    private AssetListLoader loader = null;

    /// <summary>
    /// 開く
    /// </summary>
    public static void Open(
        SingleBattleResultPopupContent prefab,
        Master.SingleStageData stageData,
        SinglePlayApi.ClearResponseData response,
        Rank clearRank,
        Action onClose)
    {
        //報酬
        var rewards = response.firstReward.Concat(response.normalReward).ToArray();

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
            content.Setup(dialog, stageData, response, clearRank, rewards, loader, onClose);
        });
    }

    /// <summary>
    /// セットアップ
    /// </summary>
    private void Setup(
        SimpleDialog dialog,
        Master.SingleStageData stageData,
        SinglePlayApi.ClearResponseData response,
        Rank clearRank,
        SinglePlayApi.RewardData[] rewards,
        AssetListLoader loader,
        Action onClose)
    {
        this.dialog = dialog;
        this.dialog.onClose = onClose;
        this.response = response;
        this.rewards = rewards;
        this.loader = loader;

        //星マーク
        for (int i = 0; i < this.starImage.Length; i++)
        {
            this.starImage[i].enabled = i < (int)clearRank - 2;
        }

        //通常報酬が8個以下なら
        if (this.rewards.Length < 9)
        {
            //非スクロールエリアに報酬アイコンを生成する
            this.horizontalLayoutGroup.gameObject.SetActive(true);
            this.scrollView.gameObject.SetActive(false);

            for (int i = 0; i < this.rewards.Length; i++)
            {
                var icon = Instantiate(this.rewardIconPrefab, this.horizontalLayoutGroup.transform, false);
                this.OnUpdateRewardIconView(icon.gameObject, i);
            }
        }
        //通常報酬が9個以上なら
        else
        {
            //スクロールエリアに報酬アイコンを生成する
            this.horizontalLayoutGroup.gameObject.SetActive(true);
            this.scrollView.gameObject.SetActive(true);
            this.scrollView.Initialize(this.rewardIconPrefab.gameObject, this.rewards.Length, this.OnUpdateRewardIconView);
        }

        //タイトルテキスト設定
        this.dialog.titleText.text = Masters.LocalizeTextDB.Get("Result");

        //ステージセレクトへ戻るボタンの追加
        var button = this.dialog.AddOKButton();
        button.text.text = Masters.LocalizeTextDB.Get("ToStageSelect");
        button.onClick = this.OnClickOKButton;
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
        icon.Setup(this.rewards[i], i < this.response.firstReward.Length);
    }

    /// <summary>
    /// OKボタン押下時
    /// </summary>
    public void OnClickOKButton()
    {
        if (!this.dialog.isClose)
        {
            this.dialog.Close();
        }
    }
}

}