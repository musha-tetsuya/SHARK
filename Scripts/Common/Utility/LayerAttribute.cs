using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// インスペクター上でintをレイヤーのドロップダウン選択表示にするAttribute
/// </summary>
public class LayerAttribute : PropertyAttribute
{
}

#if UNITY_EDITOR
/// <summary>
/// LayerAttributeの描画クラス
/// </summary>
[CustomPropertyDrawer(typeof(LayerAttribute))]
public class LayerAttributeDrawer : PropertyDrawer
{
    /// <summary>
    /// 描画
    /// </summary>
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        property.intValue = EditorGUI.LayerField(position, label, property.intValue);
    }
}
#endif

