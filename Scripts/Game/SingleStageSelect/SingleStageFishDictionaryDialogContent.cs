using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using FishDataEx = System.Tuple<Master.FishData, Master.ModelBase>;

/// <summary>
/// シングルステージ魚図鑑
/// </summary>
public class SingleStageFishDictionaryDialogContent : MonoBehaviour
{
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
    /// ワールド名、ステージ名エリア
    /// </summary>
    [SerializeField]
    private RectTransform nameArea = null;
    /// <summary>
    /// 魚サムネイルアイコンプレハブ
    /// </summary>
    [SerializeField]
    private FishThumbnailIcon thumbnailIconPrefab = null;
    /// <summary>
    /// スクロールビュー
    /// </summary>
    [SerializeField]
    private InfiniteScrollView scrollView = null;
    /// <summary>
    /// フォーカス中魚イメージ
    /// </summary>
    [SerializeField]
    private Image focusedFishImage = null;
    /// <summary>
    /// フォーカス中魚名前テキスト
    /// </summary>
    [SerializeField]
    private Text focusedFishNameText = null;
    /// <summary>
    /// フォーカス中魚説明テキスト
    /// </summary>
    [SerializeField]
    private Text focusedFishDescriptionText = null;
    /// <summary>
    /// フォーカス中魚の倍率とか表記するテキストの背景イメージ
    /// </summary>
    [SerializeField]
    private Image focusedFishSubTextBg = null;
    /// <summary>
    /// フォーカス中魚の倍率とか表記するテキスト
    /// </summary>
    [SerializeField]
    private Text focusedFishSubText = null;
    /// <summary>
    /// 普通魚テキスト色
    /// </summary>
    [SerializeField]
    private Color normalFishTextColor = Color.white;
    /// <summary>
    /// 特殊魚テキスト色
    /// </summary>
    [SerializeField]
    private Color specialFishTextColor = Color.white;

    /// <summary>
    /// 自身のダイアログ
    /// </summary>
    private SimpleDialog dialog = null;
    /// <summary>
    /// 出現魚リスト
    /// </summary>
    private List<FishDataEx> fishDataList = null;
    /// <summary>
    /// フォーカス中魚データ
    /// </summary>
    private FishDataEx focusedFishData = null;
    /// <summary>
    /// ローダー
    /// </summary>
    private AssetListLoader loader = null;
    /// <summary>
    /// 図鑑背景イメージ
    /// </summary>
    [SerializeField]
    private Image worldBgImage = null;

    /// <summary>
    /// 開く
    /// </summary>
    public static void Open(SingleStageFishDictionaryDialogContent prefab, Master.ModelBase master)
    {
        List<FishDataEx> fishDataList = null;
        string worldKey = null;

        //シングルモードの場合
        if (master is Master.SingleStageData)
        {
            var stageData = master as Master.SingleStageData;

            fishDataList = Masters.SingleStageFishDB
                .GetList()
                .Where(x => x.stageId == stageData.id)
                .Select(x => new FishDataEx(Masters.FishDB.FindById(x.fishId), x))
                .ToList();
                
            worldKey = Masters.SingleWorldDB.FindById(stageData.worldId).key;
        }
        //マルチモードの場合
        else if (master is Master.MultiWorldData)
        {
            var worldData = master as Master.MultiWorldData;

            fishDataList = Masters.MultiStageFishDB
                .GetList()
                .Where(x => x.worldId == worldData.id)
                .Select(x => new FishDataEx(Masters.FishDB.FindById(x.fishId), x))
                .ToList();
                
            worldKey = worldData.key;
        }

        var loader = new AssetListLoader(fishDataList.Select(x => new AssetLoader<Sprite>(SharkDefine.GetFishThumbnailSpritePath(x.Item1.key))));
        loader.Add<Sprite>(SharkDefine.GetZukanBgSpritePath(worldKey));

        //ロード中のタッチブロック
        SharedUI.Instance.DisableTouch();

        //ロード
        loader.Load(() =>
        {
            //タッチブロック解除
            SharedUI.Instance.EnableTouch();

            //ダイアログ開く
            var dialog = SharedUI.Instance.ShowSimpleDialog();
            var content = dialog.AddContent(prefab);
            content.Setup(dialog, fishDataList, loader, master);
        });
    }

    /// <summary>
    /// セットアップ
    /// </summary>
    private void Setup(SimpleDialog dialog, List<FishDataEx> fishDataList, AssetListLoader loader,　Master.ModelBase master)
    {
        this.dialog = dialog;
        this.fishDataList = fishDataList;
        this.loader = loader;

        //シングルモード内容構築
        if (master is Master.SingleStageData)
        {
            var stageData = master as Master.SingleStageData;
            //ワールド名
            this.worldNameText.text = Masters.SingleWorldDB.FindById(stageData.worldId).name;
            //ステージ名
            this.stageNameText.text = stageData.name;
            //特殊魚マーク表示OFF
            this.focusedFishSubTextBg.gameObject.SetActive(false);
            // シングルワールドマスターデータキー
            var worldKey = Masters.SingleWorldDB.FindById(stageData.worldId).key;
            // 図鑑の背景イメージ、パース
            string spritePath = SharkDefine.GetZukanBgSpritePath(worldKey);
            // 図鑑の背景イメージ、ロード
            this.worldBgImage.sprite = this.loader[spritePath].handle.asset as Sprite;

        }
        //マルチモード内容構築
        else if (master is Master.MultiWorldData)
        {
            var worldData = master as Master.MultiWorldData;
            //ワールド名
            this.worldNameText.text = worldData.name;
            //ステージ名非表示
            this.stageNameText.gameObject.SetActive(false);
            // 図鑑の背景イメージ、パース
            string spritePath = SharkDefine.GetZukanBgSpritePath(worldData.key);
            // 図鑑の背景イメージ、ロード
            this.worldBgImage.sprite = this.loader[spritePath].handle.asset as Sprite;
        }

        //ダイアログタイトル「図鑑」
        this.dialog.titleText.text = Masters.LocalizeTextDB.Get("Dictionary");

        //Closeボタン表示ON
        this.dialog.closeButtonEnabled = true;

        //ワールド名、ステージ名の幅に合わせてレイアウト調整
        LayoutRebuilder.ForceRebuildLayoutImmediate(this.nameArea);

        //初期フォーカス魚設定
        if (this.fishDataList.Count > 0)
        {
            this.SetFocusedFish(this.fishDataList[0]);
        }

        //スクロールビュー構築
        this.scrollView.Initialize(
            this.thumbnailIconPrefab.gameObject,
            this.fishDataList.Count,
            this.OnUpdateFishThumbnailIconView
        );
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
    /// 魚サムネイルアイコン表示構築
    /// </summary>
    private void OnUpdateFishThumbnailIconView(GameObject gobj, int i)
    {
        var icon = gobj.GetComponent<FishThumbnailIcon>();
        var isSelected = (this.fishDataList[i] == this.focusedFishData);
        icon.Setup(this.fishDataList[i], isSelected, this.OnClickFishThumbnailIcon);
    }

    /// <summary>
    /// フォーカス中魚をセットする
    /// </summary>
    private void SetFocusedFish(FishDataEx fishData)
    {
        this.focusedFishData = fishData;

        //魚の名前
        this.focusedFishNameText.text = fishData.Item1.name;

        //魚の説明文
        this.focusedFishDescriptionText.text = fishData.Item1.description;

        //マルチ魚だったら
        if (fishData.Item2 is Master.MultiStageFishData)
        {
            var multiFishStatus = fishData.Item2 as Master.MultiStageFishData;
            if (multiFishStatus.specialFishId > 0)
            {
                //特殊魚の場合「特殊」表示
                this.focusedFishSubTextBg.sprite = SceneChanger.currentScene.sceneAtlas.GetSprite("SsCm_020_0011");
                this.focusedFishSubText.text = Masters.LocalizeTextDB.Get("Special");
                this.focusedFishSubText.color = this.specialFishTextColor;
            }
            else
            {
                //普通魚の場合「{0}倍」表示
                this.focusedFishSubTextBg.sprite = SceneChanger.currentScene.sceneAtlas.GetSprite("SsCm_020_0010");
                this.focusedFishSubText.text = Masters.LocalizeTextDB.GetFormat("Rate", multiFishStatus.rate * 0.01f);
                this.focusedFishSubText.color = this.normalFishTextColor;
            }
        }

        //スプライトのパス
        string spritePath = SharkDefine.GetFishThumbnailSpritePath(this.focusedFishData.Item1.key);

        //スプライトセット
        this.focusedFishImage.sprite = this.loader[spritePath].handle.asset as Sprite;
    }

    /// <summary>
    /// 魚サムネイルアイコンクリック時
    /// </summary>
    private void OnClickFishThumbnailIcon(FishThumbnailIcon icon)
    {
        if (icon.fishData != this.focusedFishData)
        {
            SoundManager.Instance.PlaySe(SeName.YES);
            this.SetFocusedFish(icon.fishData);
            this.scrollView.UpdateElement();
        }
    }
}
