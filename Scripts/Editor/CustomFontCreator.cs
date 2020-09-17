#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

/// <summary>
/// カスタムフォント作成
/// </summary>
public class CustomFontCreator : EditorWindow
{
    /// <summary>
    /// フォント用マテリアル
    /// </summary>
    private Material fontMaterial = null;

    /// <summary>
    /// 分割数
    /// </summary>
    private Vector2Int splitSize = new Vector2Int(1, 1);

    /// <summary>
    /// カスタムフォント作成ウィンドウを開く
    /// </summary>
    [MenuItem("CONTEXT/Material/Create CustomFont")]
    private static void Open(MenuCommand command)
    {
        var window = EditorWindow.GetWindow<CustomFontCreator>();
        window.fontMaterial = command.context as Material;
    }

    /// <summary>
    /// OnGUI
    /// </summary>
    private void OnGUI()
    {
        //マテリアル設定
        this.fontMaterial = EditorGUILayout.ObjectField("Font Material", this.fontMaterial, typeof(Material), false) as Material;

        //分割数設定
        this.splitSize = EditorGUILayout.Vector2IntField("Split Size", this.splitSize);

        //作成
        if (GUILayout.Button("　Create　", GUILayout.ExpandWidth(false)))
        {
            if (this.fontMaterial == null)
            {
                Debug.LogError("Font Material is null.");
                return;
            }

            if (this.fontMaterial.mainTexture == null)
            {
                Debug.LogError("Font Material MainTexture is null.");
                return;
            }

            if (this.splitSize.x <= 0)
            {
                Debug.LogError("SplitSize.x <= 0");
                return;
            }

            if (this.splitSize.y <= 0)
            {
                Debug.LogError("SplitSize.y <= 0");
                return;
            }

            //パス
            string path = Path.ChangeExtension(AssetDatabase.GetAssetPath(this.fontMaterial), "fontsettings");
            //フォント
            var font = AssetDatabase.LoadAssetAtPath<Font>(path);
            if (font == null)
            {
                font = new Font();
                AssetDatabase.CreateAsset(font, path);
            }
            //基礎UV値
            var uv = new Vector2(1f / this.splitSize.x, 1f / this.splitSize.y);
            //基礎サイズ
            var size = new Vector2Int(this.fontMaterial.mainTexture.width / this.splitSize.x, this.fontMaterial.mainTexture.height / this.splitSize.y);
            //文字情報
            var characterInfo = new CharacterInfo[this.splitSize.x * this.splitSize.y];

            //文字情報作成
            for (int x = 0; x < this.splitSize.x; x++)
            {
                for (int y = 0; y < this.splitSize.y; y++)
                {
                    int i = this.splitSize.x * y + x;
                    var uvTopLeft = new Vector2(uv.x * x, 1f - uv.y * y);

                    characterInfo[i].index = (i < font.characterInfo.Length) ? font.characterInfo[i].index : 0;
                    characterInfo[i].uvTopLeft = uvTopLeft;
                    characterInfo[i].uvTopRight = uvTopLeft + new Vector2(uv.x, 0f);
                    characterInfo[i].uvBottomLeft = uvTopLeft + new Vector2(0f, -uv.y);
                    characterInfo[i].uvBottomRight = uvTopLeft + new Vector2(uv.x, -uv.y);
                    characterInfo[i].minX = 0;
                    characterInfo[i].maxX = size.x;
                    characterInfo[i].minY = -size.y;
                    characterInfo[i].maxY = 0;
                    characterInfo[i].advance = size.x;
                }
            }

            //マテリアルと文字情報を保存
            font.material = this.fontMaterial;
            font.characterInfo = characterInfo;
            AssetDatabase.SaveAssets();

            //フォントサイズ保存
            var fontSO = new SerializedObject(font);
            fontSO.FindProperty("m_LineSpacing").floatValue = size.y;
            fontSO.FindProperty("m_FontSize").floatValue = size.x;
            fontSO.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
        }
    }
}
#endif