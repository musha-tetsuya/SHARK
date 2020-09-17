using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// シーン基底
/// </summary>
public class SceneBase : MonoBehaviour, HeaderPanel.IEventListner
{
    /// <summary>
    /// ヘッダーを使うかどうか
    /// </summary>
    [SerializeField]
    public bool useHeader = false;
    /// <summary>
    /// シーンアトラス
    /// </summary>
    [SerializeField]
    public AtlasSpriteCache sceneAtlas = null;

    /// <summary>
    /// Awake
    /// </summary>
    protected virtual void Awake()
    {
        //マルチタッチを許可しない
        Input.multiTouchEnabled = false;

        if (this.useHeader)
        {
            SharedUI.Instance.ShowHeader();
            SharedUI.Instance.header.Set(this);
        }
    }

    /// <summary>
    /// ①Awake ②OnSceneLoaded ③Startの順で呼ばれます
    /// </summary>
    public virtual void OnSceneLoaded(SceneDataPackBase dataPack)
    {
    }

    /// <summary>
    /// ユーザーアイコンクリック時
    /// </summary>
    public virtual void OnClickUserIcon()
    {
    }

    /// <summary>
    /// HOMEボタンクリック時
    /// </summary>
    public virtual void OnClickHomeButton()
    {
        SceneChanger.ChangeSceneAsync("Home");
    }

    /// <summary>
    /// コインボタンクリック時
    /// </summary>
    public virtual void OnClickCoinButton() {
        SceneChanger.ChangeSceneAsync("Shop", new ToShopSceneDataPack() { pageType = ShopScene.PageType.Coin });
    }

    /// <summary>
    /// ジェムボタンクリック時
    /// </summary>
    public virtual void OnClickGemButton()
    {
        SceneChanger.ChangeSceneAsync("Shop", new ToShopSceneDataPack() { pageType = ShopScene.PageType.Gem });
    }

#if DEBUG
    /// <summary>
    /// GUI描画
    /// </summary>
    public virtual void DrawGUI()
    {
    }
#endif
}
