using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FishDataEx = System.Tuple<Master.FishData, Master.ModelBase>;

/// <summary>
/// 魚図鑑のサムネイルアイコン
/// </summary>
public class FishThumbnailIcon : MonoBehaviour
{
    /// <summary>
    /// サムネイルイメージ
    /// </summary>
    [SerializeField]
    private Image thumbnailImage = null;
    /// <summary>
    /// 名前テキスト
    /// </summary>
    [SerializeField]
    private Text nameText = null;
    /// <summary>
    /// 倍率とか表記するテキスト
    /// </summary>
    [SerializeField]
    private Text subText = null;
    /// <summary>
    /// 選択中マーク
    /// </summary>
    [SerializeField]
    private Image selectedImage = null;
    /// <summary>
    /// 背景（魚のタイプで差し替え）
    /// </summary>
    [SerializeField]
    private Image bgImage = null;
    /// <summary>
    /// 背景影（魚のタイプで差し替え）
    /// </summary>
    [SerializeField]
    private Image shadowImage = null;
    /// <summary>
    /// 背景フレーム（魚のタイプで差し替え）
    /// </summary>
    [SerializeField]
    private Image frameImage = null;

    /// <summary>
    /// 魚データ
    /// </summary>
    public FishDataEx fishData { get; private set; }
    /// <summary>
    /// クリック時コールバック
    /// </summary>
    private Action<FishThumbnailIcon> onClick = null;

    /// <summary>
    /// セットアップ
    /// </summary>
    public void Setup(FishDataEx fishData, bool isSelected, Action<FishThumbnailIcon> onClick)
    {
        //選択中マーク
        this.selectedImage.enabled = isSelected;

        if (this.fishData == fishData)
        {
            return;
        }

        this.fishData = fishData;

        //魚の名前
        this.nameText.text = this.fishData.Item1.name;

        //クリック時コールバック設定
        this.onClick = onClick;

        //特殊魚かどうか
        bool isSpecialFish = false;

        //マルチモードの場合
        if (this.fishData.Item2 is Master.MultiStageFishData)
        {
            var multiFishStatus = this.fishData.Item2 as Master.MultiStageFishData;
            if (isSpecialFish = multiFishStatus.specialFishId > 0)
            {
                this.subText.text = Masters.LocalizeTextDB.Get("Special");
            }
            else
            {
                this.subText.text = Masters.LocalizeTextDB.GetFormat("Rate", multiFishStatus.rate * 0.01f);
            }
        }

        //特殊魚の場合赤い下地
        if (isSpecialFish)
        {
            this.bgImage.sprite = SharedUI.Instance.commonAtlas.GetSprite("CmFrm_010_0033");
            this.shadowImage.sprite = SceneChanger.currentScene.sceneAtlas.GetSprite("SsCm_020_0003");
            this.frameImage.sprite = SceneChanger.currentScene.sceneAtlas.GetSprite("SsCm_020_0004");
        }
        //普通魚の場合青い下地
        else
        {
            this.bgImage.sprite = SharedUI.Instance.commonAtlas.GetSprite("CmFrm_010_0030");
            this.shadowImage.sprite = SceneChanger.currentScene.sceneAtlas.GetSprite("SsCm_020_0001");
            this.frameImage.sprite = SceneChanger.currentScene.sceneAtlas.GetSprite("SsCm_020_0002");
        }

        //スプライトのパス
        string spritePath = SharkDefine.GetFishThumbnailSpritePath(this.fishData.Item1.key);

        //ロード済みのはずのスプライトでセット
        this.thumbnailImage.sprite = AssetManager.FindHandle<Sprite>(spritePath).asset as Sprite;
    }

    /// <summary>
    /// クリック時
    /// </summary>
    public void OnClick()
    {
        this.onClick?.Invoke(this);
    }
}
