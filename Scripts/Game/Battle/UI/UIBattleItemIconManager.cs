using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace Battle {

/// <summary>
/// バトルアイテムアイコンUI管理
/// </summary>
[Serializable]
public class UIBattleItemIconManager
{
    /// <summary>
    /// アイコンプレハブ
    /// </summary>
    [SerializeField]
    private UIBattleItemIcon iconPrefab = null;
    /// <summary>
    /// アイコン生成先
    /// </summary>
    [SerializeField]
    private HorizontalLayoutGroup iconArea = null;

    /// <summary>
    /// アイコンリスト
    /// </summary>
    [NonSerialized]
    public List<UIBattleItemIcon> icons = new List<UIBattleItemIcon>();

    /// <summary>
    /// アイテム情報セット
    /// </summary>
    public void Set(UserItemData[] userItemDatas, Action<UIBattleItemIcon> onClick)
    {
        for (int i = 0; i < userItemDatas.Length; i++)
        {
            var icon = this.icons.Find(x => x.userItemData == userItemDatas[i]);

            if (icon == null)
            {
                //リストに存在しないのでアイコン生成
                icon = GameObject.Instantiate(this.iconPrefab, this.iconArea.transform, false);
                icon.Init(userItemDatas[i], onClick);
                this.icons.Add(icon);
            }
            else
            {
                //既存なので情報変化による表示の更新
                icon.RefleshStockCountText();
                icon.RefleshButtonInteractable();
            }

            icon.transform.SetAsLastSibling();
        }
    }

    /// <summary>
    /// 必要ならリソースロード
    /// </summary>
    public void LoadIfNeed(Action onLoaded = null)
    {
        //ロードが必要なアイコンたち
        var needLoadIcons = this.icons.FindAll(x => x.loader.Count == 0);

        if (needLoadIcons.Count == 0)
        {
            //ロードが必要なアイコンは無い
            onLoaded?.Invoke();
            return;
        }

        //ロード
        for (int i = 0; i < needLoadIcons.Count; i++)
        {
            if (i < needLoadIcons.Count - 1)
            {
                needLoadIcons[i].Load(null);
            }
            else
            {
                needLoadIcons[i].Load(onLoaded);
            }
        }
    }

    /// <summary>
    /// セットアップ
    /// </summary>
    public void Setup()
    {
        bool isVisibleBanMark = this.icons.Exists(x => x.IsVisibleBanMark());

        for (int i = 0; i < this.icons.Count; i++)
        {
            this.icons[i].Setup();
            
            if (this.icons[i].IsTurretController())
            {
                this.icons[i].SetVisibleBanMark(isVisibleBanMark);
            }
        }
    }

    /// <summary>
    /// Run
    /// </summary>
    public void Run(float deltaTime)
    {
        for (int i = 0; i < this.icons.Count; i++)
        {
            this.icons[i].Run(deltaTime);
        }
    }
}

}