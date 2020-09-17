using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// マルチバトルショップのタブ
/// </summary>
public class MultiBattleShopTab : ShopMainTab
{
    /// <summary>
    /// 取り扱う商品グループ
    /// </summary>
    [SerializeField]
    public ShopScene.ShopGroupType[] shopGroupTypes = null;

    /// <summary>
    /// 商品リスト
    /// </summary>
    public ProductBase[] products = null;
}
