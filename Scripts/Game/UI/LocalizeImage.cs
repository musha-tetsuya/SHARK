using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.U2D;

/// <summary>
/// ローカライズイメージ
/// </summary>
[RequireComponent(typeof(Image))]
public class LocalizeImage : MonoBehaviour
{
    /// <summary>
    /// イメージ
    /// </summary>
    [SerializeField]
    private Image image = null;

    /// <summary>
    /// スプライト名
    /// </summary>
    [SerializeField]
    private string spriteName = null;

    /// <summary>
    /// Reset
    /// </summary>
    private void Reset()
    {
        this.image = GetComponent<Image>();

        if (this.image != null && this.image.sprite != null)
        {
            this.spriteName = this.image.sprite.name;
        }
    }

    /// <summary>
    /// Start
    /// </summary>
    private void Start()
    {
        var atlas = GlobalSpriteAtlas.GetAtlas(GlobalSpriteAtlas.AtlasType.Localization);
#if UNITY_EDITOR
        if (atlas == null)
        {
            atlas = new AtlasSpriteCache(Resources.Load<SpriteAtlas>(GetLocalizationAtlasPath()));
            GlobalSpriteAtlas.SetAtlas(GlobalSpriteAtlas.AtlasType.Localization, atlas);
        }
#endif
        this.image.sprite = atlas.GetSprite(this.spriteName);
    }

    /// <summary>
    /// ローカライズアトラスのパス取得
    /// </summary>
    public static string GetLocalizationAtlasPath()
    {
        return string.Format("SpriteAtlas/Localization/{0}/LocalizationAtlas", UserData.GetLanguage());
    }
}
