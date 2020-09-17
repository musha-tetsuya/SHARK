using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// タブグループ
/// </summary>
public class TabGroup : MonoBehaviour
{
    /// <summary>
    /// タブリスト
    /// </summary>
    [NonSerialized]
    public List<TabBase> tabList = new List<TabBase>();

    /// <summary>
    /// セットアップ
    /// </summary>
    public void Setup()
    {
        var tabs = GetComponentsInChildren<TabBase>(true);

        foreach (var tab in tabs)
        {
            this.tabList.Add(tab);
            tab.onClick.AddListener(() => this.SetActiveTab(tab));
        }
    }

    /// <summary>
    /// アクティブタブの切り替え
    /// </summary>
    public void SetActiveTab(TabBase tab)
    {
        for (int i = 0; i < this.tabList.Count; i++)
        {
            this.tabList[i].SetActive(this.tabList[i] == tab);
        }
    }

    /// <summary>
    /// 条件に一致するタブの検索
    /// </summary>
    public TabBase Find(Predicate<TabBase> match)
    {
        return this.tabList.Find(match);
    }
}
