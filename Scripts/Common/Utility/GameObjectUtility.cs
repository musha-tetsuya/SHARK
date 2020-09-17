using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameObjectUtility
{
    public static T CreateInstance<T>(GameObject prefab, Transform parent)
    {
        var go = Object.Instantiate(prefab, parent);
        return go.GetComponent<T>();
    }

    /// <summary>
    /// Active切り替え
    /// </summary>
    public static void SetActive(this GameObject[] gobjs, bool value)
    {
        foreach (var gobj in gobjs)
        {
            if (gobj != null)
            {
                gobj.SetActive(value);
            }
        }
    }

    /// <summary>
    /// 自身と全ての子のレイヤーを設定する
    /// </summary>
    public static void SetLayerRecursively(this GameObject gobj, int layer)
    {
        gobj.layer = layer;
        foreach (Transform child in gobj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }
}
