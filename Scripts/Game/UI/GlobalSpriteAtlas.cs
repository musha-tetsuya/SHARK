using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 常駐アトラス
/// </summary>
public static class GlobalSpriteAtlas
{
    /// <summary>
    /// アトラスタイプ
    /// </summary>
    public enum AtlasType
    {
        Localization,
    }

    /// <summary>
    /// アトラスリスト
    /// </summary>
    private static Dictionary<int, AtlasSpriteCache> atlasList = new Dictionary<int, AtlasSpriteCache>();

    /// <summary>
    /// アトラスセット
    /// </summary>
    public static void SetAtlas(AtlasType atlasType, AtlasSpriteCache atlas)
    {
        atlasList[(int)atlasType] = atlas;
    }

    /// <summary>
    /// アトラス取得
    /// </summary>
    public static AtlasSpriteCache GetAtlas(AtlasType atlasType)
    {
        if (atlasList.ContainsKey((int)atlasType))
        {
            return atlasList[(int)atlasType];
        }
        return null;
    }
}
