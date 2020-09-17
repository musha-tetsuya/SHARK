using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// インスペクター上で編集不可表示にするAttribute
/// </summary>
public class DisableAttribute : PropertyAttribute
{

}

#if UNITY_EDITOR
/// <summary>
/// DisableAttributeの描画クラス
/// </summary>
[CustomPropertyDrawer(typeof(DisableAttribute))]
public class DisableAttributeDrawer : PropertyDrawer
{
    /// <summary>
    /// 高さ取得
    /// </summary>
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label, true);
    }

    /// <summary>
    /// 描画
    /// </summary>
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginDisabledGroup(true);
        EditorGUI.PropertyField(position, property, label, true);
        EditorGUI.EndDisabledGroup();
    }
}
#endif