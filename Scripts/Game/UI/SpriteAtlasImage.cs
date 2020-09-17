using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.U2D;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// SpriteAtlasImage
/// </summary>
public class SpriteAtlasImage : MonoBehaviour
{
    /// <summary>
    /// 対象となるImage
    /// </summary>
    [SerializeField]
    public Image targetImage = null;
    /// <summary>
    /// アトラス名
    /// </summary>
    [SerializeField]
    public string atlasName = null;
    /// <summary>
    /// スプライト名
    /// </summary>
    [SerializeField]
    public string spriteName = null;
    /// <summary>
    /// ローカライズスプライトかどうか
    /// </summary>
    [SerializeField]
    public bool isLocalization = false;

    /// <summary>
    /// Start
    /// </summary>
    private void Start()
    {
        this.LoadSprite();
    }

    /// <summary>
    /// スプライト読み込み
    /// </summary>
    public void LoadSprite()
    {
        if (this.isLocalization)
        {
            var language = UserData.GetLanguage();

            AssetManager.Load<SpriteAtlas>(string.Format(this.atlasName, language), (atlas) =>
            {
                this.targetImage.sprite = atlas.GetSprite(this.spriteName);
            });
        }
        else
        {
            AssetManager.Load<SpriteAtlas>(this.atlasName, (atlas) =>
            {
                this.targetImage.sprite = atlas.GetSprite(this.spriteName);
            });
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// Reset
    /// </summary>
    private void Reset()
    {
        if (this.targetImage == null)
        {
            this.targetImage = GetComponent<Image>();
        }
    }

    /// <summary>
    /// カスタムインスペクター
    /// </summary>
    [CustomEditor(typeof(SpriteAtlasImage))]
    private class CustomInspector : Editor
    {
        /// <summary>
        /// target
        /// </summary>
        private new SpriteAtlasImage target { get { return (SpriteAtlasImage)base.target; } }
        /// <summary>
        /// スプライト前回値
        /// </summary>
        private Sprite beforeSprite = null;
        /// <summary>
        /// アトラス
        /// </summary>
        private SpriteAtlas atlas = null;
        /// <summary>
        /// 表示確認用言語
        /// </summary>
        private Language language = Language.Ja;

        /// <summary>
        /// OnEnable
        /// </summary>
        private void OnEnable()
        {
            if (this.target.targetImage != null)
            {
                this.OnChangeSprite(this.target.targetImage.sprite);
            }
        }

        /// <summary>
        /// OnInspectorGUI
        /// </summary>
        public override void OnInspectorGUI()
        {
            //内部キャッシュから値をロードする
            this.serializedObject.Update();

            //変更前の表示確認用言語
            var beforeLanguage = this.language;

            EditorGUILayout.PropertyField(this.serializedObject.FindProperty("targetImage"));
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(this.serializedObject.FindProperty("atlasName"));
            EditorGUILayout.PropertyField(this.serializedObject.FindProperty("spriteName"));
            EditorGUILayout.PropertyField(this.serializedObject.FindProperty("isLocalization"));
            EditorGUILayout.ObjectField("SpriteAtlas", this.atlas, typeof(SpriteAtlas), false);
            EditorGUI.EndDisabledGroup();
            if (this.target.isLocalization)
            {
                this.language = (Language)EditorGUILayout.EnumPopup("表示確認用言語切り替え", this.language);
            }

            //内部キャッシュに値を保存する
            this.serializedObject.ApplyModifiedProperties();

            //Spriteが変更された
            Sprite afterSprite = (this.target.targetImage == null) ? null : this.target.targetImage.sprite;
            if (afterSprite != this.beforeSprite)
            {
                //アトラス名、スプライト名などを設定し直す
                this.OnChangeSprite(afterSprite);
            }
            //Spriteはそのままに、表示確認用言語が変更された
            else if (this.language != beforeLanguage)
            {
                //変更先言語のアトラスからスプライトを設定し直す
                this.OnChangeLanguage(beforeLanguage, this.language);
            }
        }

        /// <summary>
        /// スプライト変更時
        /// </summary>
        private void OnChangeSprite(Sprite afterSprite)
        {
            this.target.atlasName = null;
            this.target.spriteName = null;
            this.target.isLocalization = false;
            this.beforeSprite = afterSprite;
            this.atlas = null;
            this.language = Language.Ja;

            if (afterSprite != null)
            {
                //スプライト名決定
                this.target.spriteName = afterSprite.name;

                //アトラス決定
                this.atlas = FindSpriteAtlasFromSprite(afterSprite);
                if (this.atlas != null)
                {
                    //アトラス名決定
                    this.target.atlasName = AssetDatabase
                        .GetAssetPath(this.atlas)
                        .Replace("Assets/Sunchoi/AssetbundleResources/Resources/", null)
                        .Replace("Assets/Sunchoi/BuiltinResources/Resources/", null)
                        .Replace(".spriteatlas", null);

                    //ローカライズアトラスだったら
                    if (this.target.atlasName.Contains("Localization/"))
                    {
                        this.target.isLocalization = true;

                        //言語部分文字列の抽出
                        int languageStartIndex = this.target.atlasName.IndexOf("Localization/") + "Localization/".Length;
                        int languageEndIndex = this.target.atlasName.IndexOf("/", languageStartIndex);
                        string lang = this.target.atlasName.Substring(languageStartIndex, languageEndIndex - languageStartIndex);

                        //アトラス言語決定
                        this.language = (Language)Enum.Parse(typeof(Language), lang);

                        //アトラス名の言語部分を置換用文字列に変換
                        this.target.atlasName = this.target.atlasName.Remove(languageStartIndex, lang.Length).Insert(languageStartIndex, "{0}");
                    }
                }
            }
        }

        /// <summary>
        /// 表示確認用言語変更時
        /// </summary>
        private void OnChangeLanguage(Language beforeLanguage, Language afterLanguage)
        {
            string assetPath = AssetDatabase
                .GetAssetPath(this.atlas)
                .Replace("Localization/" + beforeLanguage, "Localization/" + afterLanguage);
            this.atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(assetPath);
            this.target.targetImage.sprite = this.atlas.GetReferenceSprite(this.target.targetImage.sprite.name);
        }

        /// <summary>
        /// 指定スプライトを内包しているアトラスを検索
        /// </summary>
        private static SpriteAtlas FindSpriteAtlasFromSprite(Sprite sprite)
        {
            string[] guids = AssetDatabase.FindAssets("t:SpriteAtlas");
            for (int i = 0; i < guids.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                var spriteAtlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(assetPath);
                var packedSprites = new SerializedObject(spriteAtlas).FindProperty("m_PackedSprites");

                for (int j = 0; j < packedSprites.arraySize; j++)
                {
                    if (packedSprites.GetArrayElementAtIndex(j).objectReferenceValue == sprite)
                    {
                        return spriteAtlas;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// アセットパスからローカライズ言語部分を取得する
        /// </summary>
        private static bool GetLanguageFromAssetPath(string assetPath, out string language)
        {
            language = null;
            if (!assetPath.Contains("Resources/Localization"))
            {
                return false;
            }

            int idx = assetPath.IndexOf("Localization");
            language = assetPath.Remove(0, idx).Split('/').Skip(1).First();
            return true;
        }

    }//class CustomInspector
#endif
}//class SpriteAtlasImage
