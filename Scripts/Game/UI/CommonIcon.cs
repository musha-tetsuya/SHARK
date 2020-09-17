using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 共通アイコン
/// </summary>
public class CommonIcon : IconBase
{
    /// <summary>
    /// スケール調整
    /// </summary>
    [SerializeField]
    private RectTransform scaleFitter = null;
    /// <summary>
    /// RectTransform
    /// </summary>
    [SerializeField]
    public RectTransform rectTransform = null;
    /// <summary>
    /// 背景イメージ
    /// </summary>
    [SerializeField]
    public Image bgImage = null;
    /// <summary>
    /// アイコンイメージ
    /// </summary>
    [SerializeField]
    public Image iconImage = null;
    /// <summary>
    /// ギアエリア
    /// </summary>
    [SerializeField]
    public GameObject gearArea = null;
    /// <summary>
    /// ギアアイコン
    /// </summary>
    [SerializeField]
    public Image[] gearIcon = null;
    /// <summary>
    /// ギアONアイコン
    /// </summary>
    [SerializeField]
    public Image[] gearOnIcon = null;
    /// <summary>
    /// フレームイメージ
    /// </summary>
    [SerializeField]
    public Image frameImage = null;
    /// <summary>
    /// ランクイメージ
    /// </summary>
    [SerializeField]
    public Image rankImage = null;
    /// <summary>
    /// ランク背景イメージ
    /// </summary>
    [SerializeField]
    public Image rankBgImage = null;
    /// <summary>
    /// 数量テキスト
    /// </summary>
    [SerializeField]
    public Text countText = null;
    
    [Header("ギアアイテム")]
    /// <summary>
    /// ギアエリアオブジェクト
    /// </summary>
    [SerializeField]
    public GameObject gearItemObject = null;
    /// <summary>
    /// ギア背景イメージ
    /// </summary>
    [SerializeField]
    public Image gearBgImage = null;
    /// <summary>
    /// ギアメインイメージ
    /// </summary>
    [SerializeField]
    public Image gearMainImage = null;
    /// <summary>
    /// ギアサーブイメージ
    /// </summary>
    [SerializeField]
    public Image gearSubImage = null;

    /// <summary>
    /// 詳細ダイアログデータ
    /// </summary>    
    [SerializeField]
    private CommonItemInfoDialogData infoDialogData = null;

    /// <summary>
    /// 全グラフィックオブジェクト
    /// </summary>
    private Graphic[] graphics = null;

    /// <summary>
    /// 親のサイズに合わせる
    /// </summary>
    private void Start()
    {
        this.AdjustScale();
    }

    /// <summary>
    /// スケール調整
    /// </summary>
    private void AdjustScale()
    {
        var scale = Vector3.one;
        scale.x = this.rectTransform.sizeDelta.x / this.scaleFitter.sizeDelta.x;
        scale.y = this.rectTransform.sizeDelta.y / this.scaleFitter.sizeDelta.y;
        this.scaleFitter.localScale = scale;
    }

    /// <summary>
    /// セット
    /// </summary>
    public void Set(uint itemType, uint itemId, bool setLongPress)
    {
        var itemInfo = CommonIconUtility.GetItemInfo(itemType, itemId);
        this.Set(itemInfo, setLongPress);
    }

    /// <summary>
    /// セット
    /// </summary>
    public void Set(IItemInfo itemInfo, bool setLongPress)
    {
        if (setLongPress)
        {
            this.onLongPress = () =>
            {
                this.infoDialogData.OpenDialog(itemInfo);
            };
        }

        if (itemInfo is Master.GearData)
        {
            var gearData = itemInfo as Master.GearData;

            //ギア表示ON
            this.SetGearCommonIcon(true);

            //スプライト設定
            this.SetGearSprite(
                bgSprite: CommonIconUtility.GetGearBgSprite(gearData.partsType),
                mainSprite: CommonIconUtility.GetGearMainImageSprite(gearData.key),
                subSprite: CommonIconUtility.GetGearSubImageSprite(gearData.subKey)
            );
        }
        else
        {
            //ギア表示OFF
            this.SetGearCommonIcon(false);

            Sprite sprite = null;

            if (itemInfo.IsCommonSprite())
            {
                sprite = SharedUI.Instance.commonAtlas.GetSprite(itemInfo.GetSpritePath());
            }
            else
            {
                var spritePath = itemInfo.GetSpritePath();
                sprite = AssetManager.FindHandle<Sprite>(spritePath).asset as Sprite;
            }

            //スプライト設定
            this.SetIconSprite(sprite);
        }

        //ランク設定
        var rank = itemInfo.GetRank();
        this.SetRank(rank);
    }

    /// <summary>
    /// アイコン画像差し替え
    /// </summary>
    public void SetIconSprite(Sprite sprite)
    {
        this.iconImage.sprite = sprite;
    }

    /// <summary>
    /// ギアスロット設定
    /// </summary>
    public void SetGearSlot(UserPartsData partsData)
    {
        uint defaultCount = partsData.itemType == (uint)ItemType.Battery ? Masters.BatteryDB.FindById(partsData.itemId).defaultGearSlotSize
                          : partsData.itemType == (uint)ItemType.Barrel ? Masters.BarrelDB.FindById(partsData.itemId).defaultGearSlotSize
                          : partsData.itemType == (uint)ItemType.Bullet ? Masters.BulletDB.FindById(partsData.itemId).defaultGearSlotSize
                          : 0;
        uint count = defaultCount + partsData.gearSlotExpandCount;

        int size = partsData.gearMasterIds.Length;
        for (int i = 0; i < this.gearIcon.Length; i++)
        {
            this.gearIcon[i].enabled = i < count;
            if(i < size)
            {
                this.gearOnIcon[i].gameObject.SetActive(true);
            }
            else
            {
                this.gearOnIcon[i].gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// 枠画像差し替え
    /// </summary>
    public void SetFrameSprite(Sprite sprite)
    {
        this.frameImage.sprite = sprite;
    }

    /// <summary>
    /// ランク画像差し替え
    /// </summary>
    public void SetRankSprite(Sprite sprite)
    {
        this.rankImage.sprite = sprite;
    }

    /// <summary>
    /// ランク背景画像差し替え
    /// </summary>
    public void SetRankBgSprite(Sprite sprite)
    {
        this.rankBgImage.sprite = sprite;
    }

    /// <summary>
    /// 一括ランク設定
    /// </summary>
    public void SetRank(Rank rank)
    {
        this.SetFrameSprite(CommonIconUtility.GetRarityFrameSprite(rank));
        this.rankImage.enabled =
        this.rankBgImage.enabled = rank != Rank.None;

        if (rank != Rank.None)
        {
            this.SetRankSprite(CommonIconUtility.GetRaritySprite(rank));
            this.SetRankBgSprite(CommonIconUtility.GetRarityBgSprite(rank));
        }
    }

    /// <summary>
    /// 数量テキスト設定
    /// </summary>
    public void SetCountText(uint count)
    {
        this.countText.text = string.Format("×{0}", count);
    }

    /// <summary>
    /// ボタンの有効無効切り替え
    /// </summary>
    public void SetButtonInteractable(bool interactable)
    {
        this.button.interactable = interactable;
    }

    /// <summary>
    /// グレースケールON/OFF切り替え
    /// </summary>
    public void SetGrayScale(bool isGrayScale)
    {
        if (this.graphics == null)
        {
            this.graphics = GetComponentsInChildren<Graphic>(true);
        }

        var material = isGrayScale ? SharedUI.Instance.grayScaleMaterial : null;

        foreach (var graphic in this.graphics)
        {
            graphic.material = material;
        }
    }

    /// <summary>
    /// TODO.ギアスプライトセット
    /// </summary>
    public void SetGearSprite(Sprite bgSprite, Sprite mainSprite, Sprite subSprite)
    {
        this.gearBgImage.sprite = bgSprite;
        this.gearMainImage.sprite = mainSprite;
        this.gearSubImage.sprite = subSprite;
    }

    /// <summary>
    /// TODO.ギアエリアセット
    /// </summary>
    public void SetGearCommonIcon(bool isGear)
    {
        if(isGear == true)
        {
            this.gearItemObject.SetActive(true);
            this.bgImage.gameObject.SetActive(false);
            this.iconImage.gameObject.SetActive(false);
            this.frameImage.gameObject.SetActive(false);
            this.gearArea.gameObject.SetActive(false);
            this.countText.text = null;
        }
        else
        {
            this.gearItemObject.SetActive(false);
            this.bgImage.gameObject.SetActive(true);
            this.iconImage.gameObject.SetActive(true);
            this.frameImage.gameObject.SetActive(true);
            this.countText.text = null;
        }
    }

    /// <summary>
    /// 枠と背景の可視設定
    /// </summary>
    public void SetFrameVisible(bool visible)
    {
        this.bgImage.enabled =
        this.frameImage.enabled = visible;
    }

#if UNITY_EDITOR
    /// <summary>
    /// カスタムインスペクタ―
    /// </summary>
    [CustomEditor(typeof(CommonIcon))]
    private class MyInspector : Editor
    {
        /// <summary>
        /// OnEnable
        /// </summary>
        private void OnEnable()
        {
            (this.target as CommonIcon).AdjustScale();
        }

        /// <summary>
        /// OnInspectorGUI
        /// </summary>
        public override void OnInspectorGUI()
        {
            this.serializedObject.Update();
            base.OnInspectorGUI();
            (this.target as CommonIcon).AdjustScale();
            this.serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}
