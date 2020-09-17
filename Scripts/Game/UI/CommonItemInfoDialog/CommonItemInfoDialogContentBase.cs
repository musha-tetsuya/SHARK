using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// アイテム詳細ダイアログ内容基底
/// </summary>
public abstract class CommonItemInfoDialogContentBase : MonoBehaviour
{
    /// <summary>
    /// セットアップ
    /// </summary>
    public virtual void Setup(IItemInfo itemInfo){}

    /// <summary>
    /// セットアップ
    /// </summary>
    public virtual void Setup(ProductBase product){}
}
