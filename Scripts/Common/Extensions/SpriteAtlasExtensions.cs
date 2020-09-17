using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// スプライトアトラス拡張
/// </summary>
public static class SpriteAtlasExtensions
{
#if UNITY_EDITOR
    /// <summary>
    /// 参照スプライトの取得
    /// </summary>
    /// <para>editor only</para>
    public static Sprite GetReferenceSprite(this SpriteAtlas atlas, string spriteName)
    {
        var so = new SerializedObject(atlas);
        var packedSprites = so.FindProperty("m_PackedSprites");
        var spriteNames = so.FindProperty("m_PackedSpriteNamesToIndex");

        for (int i = 0; i < spriteNames.arraySize; i++)
        {
            if (spriteNames.GetArrayElementAtIndex(i).stringValue == spriteName)
            {
                return (Sprite)packedSprites.GetArrayElementAtIndex(i).objectReferenceValue;
            }
        }

        return null;
    }
#endif
}
