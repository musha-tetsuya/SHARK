using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// レベルアップ演出
/// </summary>
public class UILevelUp : AnimationEventReceiver, IPointerClickHandler
{
    /// <summary>
    /// 数値グループ
    /// </summary>
    [Serializable]
    private class NumGroup
    {
        /// <summary>
        /// ルート
        /// </summary>
        [SerializeField]
        public GameObject root = null;
        /// <summary>
        /// 数値イメージ
        /// </summary>
        [SerializeField]
        public Image[] numImages = null;
        /// <summary>
        /// アニメーションステート名
        /// </summary>
        [SerializeField]
        public string stateName = null;
    }

    /// <summary>
    /// 数値グループリスト
    /// </summary>
    [SerializeField]
    private NumGroup[] numGroups = null;
    /// <summary>
    /// アイテムアイコン生成先
    /// </summary>
    [SerializeField]
    private RectTransform itemIconArea = null;
    /// <summary>
    /// アイテムアイコンプレハブ
    /// </summary>
    [SerializeField]
    private CommonIcon itemIconPrefab = null;
    /// <summary>
    /// アニメーション終了カウント
    /// </summary>
    [SerializeField]
    private int finishedCount = 1;

    /// <summary>
    /// ローダー
    /// </summary>
    private AssetListLoader loader = null;
    /// <summary>
    /// 終了時コールバック
    /// </summary>
    private Action onClose = null;

    /// <summary>
    /// 開く
    /// </summary>
    public static void Open(MultiPlayApi.LogData logData, UILevelUp prefab, RectTransform parent, Action onClose)
    {
        //API実行
        MultiPlayApi.CallLevelUpApi(logData, (response) =>
        {
            //レベルアップ前後の値
            //uint beforeLevel = UserData.Get().lv;
            uint afterLevel = response.tUsers.level;
            UserData.Get().lv = afterLevel;

            //ローダー
            var loader = new AssetListLoader();

            if (response.mLevelReward != null)
            {
                //汎用スプライトじゃなければローダーに積む
                loader.AddRange(response.mLevelReward
                    .Select(x => CommonIconUtility.GetItemInfo(x.itemType, x.itemId))
                    .Where(x => !x.IsCommonSprite())
                    .Select(x => new AssetLoader<Sprite>(x.GetSpritePath())));

                //報酬付与
                foreach (var reward in response.mLevelReward)
                {
                    UserData.Get().AddItem((ItemType)reward.itemType, reward.itemId, reward.itemNum);
                }
            }

            //ロード中のタッチブロック
            SharedUI.Instance.DisableTouch();

            //ロード開始
            loader.Load(() =>
            {
                //タッチブロック解除
                SharedUI.Instance.EnableTouch();

                //レベルアップダイアログ開く
                var dialog = Instantiate(prefab, parent, false);
                dialog.Setup(response, loader, onClose);
            });
        });
    }

    /// <summary>
    /// セットアップ
    /// </summary>
    public void Setup(MultiPlayApi.LevelUpResponseData response, AssetListLoader loader, Action onClose)
    {
        this.loader = loader;
        this.onClose = onClose;

        //レベルの桁数によってOn/Off切り替わる
        int lv = (int)response.tUsers.level;
        int n = (100 <= lv) ? 2 : (10 <= lv && lv < 100) ? 1 : 0;
        this.numGroups[0].root.SetActive(n == 0);
        this.numGroups[1].root.SetActive(n == 1);
        this.numGroups[2].root.SetActive(n == 2);

        //数値イメージ差し替え
        for (int i = 0; i < this.numGroups[n].numImages.Length; i++)
        {
            int num = lv / (int)Mathf.Pow(10, i) % 10;
            string spriteName = string.Format("BtMlti_030_0005_{0}", num);
            this.numGroups[n].numImages[i].sprite = SceneChanger.currentScene.sceneAtlas.GetSprite(spriteName);
        }

        //アニメーション再生
        this.animator.Play(this.numGroups[n].stateName, this.animator.GetLayerIndex("Text_Count"), 0f);

        //アイテム無しならここでreturn
        if (response.mLevelReward == null || response.mLevelReward.Length == 0)
        {
            return;
        }

        //アイテム生成
        foreach (var item in response.mLevelReward)
        {
            var itemIcon = Instantiate(this.itemIconPrefab, this.itemIconArea, false);

            //CommonIcon表示構築
            itemIcon.Set(item.itemType, item.itemId, true);

            //数量テキストセット
            if (item.itemNum > 1)
            {
                itemIcon.SetCountText(item.itemNum);
            }
            else
            {
                itemIcon.countText.text = null;
            }
        }
    }

    /// <summary>
    /// 画面クリック時
    /// </summary>
    void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
    {
        //閉じる
        this.animator.SetBool("isOut", true);
    }

    /// <summary>
    /// アニメーション終了時
    /// </summary>
    protected override void OnFinished(string tag)
    {
        this.finishedCount--;

        if (this.finishedCount == 0)
        {
            this.onClose?.Invoke();
            this.onClose = null;
            Destroy(this.gameObject);
        }
    }

    /// <summary>
    /// OnDestroy
    /// </summary>
    private void OnDestroy()
    {
        this.loader?.Unload();
    }
}
