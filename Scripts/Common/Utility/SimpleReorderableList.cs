﻿#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

/// <summary>
/// SimpleReorderableList
/// </summary>
public class SimpleReorderableList
{
    /// <summary>
    /// ReorderableList
    /// </summary>
    protected ReorderableList reorderableList { get; private set; }
    /// <summary>
    /// フィールド名一覧
    /// </summary>
    private string[] fieldNames = null;
    /// <summary>
    /// 要素サイズ
    /// </summary>
    private int elementHeightSize = 2;

    /// <summary>
    /// construct
    /// </summary>
    public SimpleReorderableList(SerializedProperty property)
    {
        this.reorderableList = new ReorderableList(property.serializedObject, property);
        this.reorderableList.elementHeightCallback = this.GetElementHeight;
        this.reorderableList.drawElementCallback = this.DrawElement;
    }

    /// <summary>
    /// construct
    /// </summary>
    public SimpleReorderableList(SerializedProperty property, Type type)
        : this(property)
    {
        this.fieldNames = type
            .GetFields(BindingFlags.Instance | BindingFlags.Public)
            .Select(info => info.Name)
            .ToArray();

        this.elementHeightSize = this.fieldNames.Length + 1;
    }

    /// <summary>
    /// リスト描画
    /// </summary>
    public void DoLayoutList()
    {
        this.reorderableList.DoLayoutList();
    }

    /// <summary>
    /// リスト要素高さ取得
    /// </summary>
    private float GetElementHeight(int index)
    {
        var property = this.reorderableList.serializedProperty.GetArrayElementAtIndex(index);

        float height = EditorGUIUtility.standardVerticalSpacing
                     + EditorGUIUtility.singleLineHeight;

        if (property.isExpanded)
        {
            height *= this.elementHeightSize;
        }

        return height;
    }

    /// <summary>
    /// リスト要素描画
    /// </summary>
    private void DrawElement(SerializedProperty property, ref Rect pos)
    {
        pos.y += EditorGUIUtility.standardVerticalSpacing;
        pos.y += EditorGUIUtility.singleLineHeight;
        pos.height = EditorGUIUtility.singleLineHeight;
        EditorGUI.PropertyField(pos, property);
    }

    /// <summary>
    /// リスト要素描画
    /// </summary>
    private void DrawElement(Rect position, int index, bool isActive, bool isFocused)
    {
        var property = this.reorderableList.serializedProperty.GetArrayElementAtIndex(index);
        var pos = position;

        AddIndent(ref pos);

        //折りたたみ表示
        pos.y += EditorGUIUtility.standardVerticalSpacing;
        pos.height = EditorGUIUtility.singleLineHeight;
        property.isExpanded = EditorGUI.Foldout(pos, property.isExpanded, "Element " + index);

        //開かれているとき
        if (property.isExpanded)
        {
            AddIndent(ref pos);

            if (this.fieldNames == null)
            {
                this.DrawElement(property, ref pos);
            }
            else
            {
                //フィールド一覧表示
                foreach (string path in this.fieldNames)
                {
                    this.DrawElement(property.FindPropertyRelative(path), ref pos);
                }
            }
        }
    }

    /// <summary>
    /// インデント追加
    /// </summary>
    private static void AddIndent(ref Rect position)
    {
        position.x += 12f;
        position.width -= 12f;
    }
}
#endif