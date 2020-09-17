using System;
using UnityEngine;
using UnityEngine.UI;

public class SharedUI : SingletonMonoBehaviour<SharedUI>
{
    /// <summary>
    /// ビルドデータ
    /// </summary>
    [SerializeField]
    public BuildData buildData = null;

    /// <summary>
    /// ヘッダー生成先
    /// </summary>
    [SerializeField]
    private RectTransform headerParent = null;

    /// <summary>
    /// ポップアップ生成先
    /// </summary>
    [SerializeField]
    private RectTransform popupRoot = null;

    /// <summary>
    /// シーン遷移時アニメ生成先
    /// </summary>
    [SerializeField]
    private RectTransform sceneChangeAnimationRoot = null;

    /// <summary>
    /// 通信中アニメ生成先
    /// </summary>
    [SerializeField]
    private RectTransform connectingRoot = null;

    /// <summary>
    /// システムポップアップ生成先
    /// </summary>
    [SerializeField]
    private RectTransform systemPopupRoot = null;

    /// <summary>
    /// タッチ制御
    /// </summary>
    [SerializeField]
    private Image touchDisabler = null;

    /// <summary>
    /// ヘッダープレハブ
    /// </summary>
    [SerializeField]
    private HeaderPanel headerPrefab = null;

    /// <summary>
    /// シンプルダイアログプレハブ
    /// </summary>
    [SerializeField]
    private SimpleDialog simpleDialogPrefab = null;

    /// <summary>
    /// シーン遷移時アニメプレハブ
    /// </summary>
    [SerializeField]
    private SceneChangeAnimation sceneChangeAnimationPrefab = null;

    /// <summary>
    /// 通信中アニメプレハブ
    /// </summary>
    [SerializeField]
    private ConnectingIndicator connectingIndicatorPrefab = null;

    /// <summary>
    /// グレースケール用マテリアル
    /// </summary>
    [SerializeField]
    public Material grayScaleMaterial = null;

    /// <summary>
    /// Commonアトラス
    /// </summary>
    [SerializeField]
    public AtlasSpriteCache commonAtlas = null;

    /// <summary>
    /// ヘッダー
    /// </summary>
    public HeaderPanel header { get; private set; }

    /// <summary>
    /// 通信中アニメ
    /// </summary>
    private ConnectingIndicator connectingIndicator = null;

    /// <summary>
    /// シーン遷移時アニメ
    /// </summary>
    private SceneChangeAnimation sceneChangeAnimation;

    /// <summary>
    /// GUIログ表示
    /// </summary>
    private GUILogViewer guiLogViewer = null;

    /// <summary>
    /// Start
    /// </summary>
    private void Start()
    {
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR && DEBUG
        this.guiLogViewer = this.gameObject.AddComponent<GUILogViewer>();
#endif
    }

    public SimpleDialog ShowSimpleDialog(bool isSystem = false)
    {
        return Instantiate(this.simpleDialogPrefab, isSystem ? this.systemPopupRoot : this.popupRoot, false);
    }

    public T ShowPopup<T>(T popupPrefab) where T : DialogBase
    {
        return Instantiate(popupPrefab, this.popupRoot, false);
    }

    /// <summary>
    /// 通信中マークを表示（タッチブロックされる）
    /// </summary>
    public void ShowConnectingIndicator()
    {
        if (this.connectingIndicator == null)
        {
            this.connectingIndicator = Instantiate(this.connectingIndicatorPrefab, this.connectingRoot, false);
        }

        this.connectingIndicator.Play();
    }

    /// <summary>
    /// 通信中マークを非表示
    /// </summary>
    public void HideConnectingIndicator()
    {
        if (this.connectingIndicator != null)
        {
            this.connectingIndicator.Destroy();
        }
    }

    /// <summary>
    /// シーン移動アニメーション開始
    /// </summary>
    public void ShowSceneChangeAnimation(Action onFinishedIn)
    {
        if (this.sceneChangeAnimation == null)
        {
            this.sceneChangeAnimation = Instantiate(this.sceneChangeAnimationPrefab, this.sceneChangeAnimationRoot, false);
            this.sceneChangeAnimation.onFinishedIn = onFinishedIn;
        }
        else
        {
            onFinishedIn?.Invoke();
        }
    }

    /// <summary>
    /// シーン移動アニメーション終了
    /// </summary>
    public void HideSceneChangeAnimation(Action onFinished = null)
    {
        if (this.sceneChangeAnimation != null)
        {
            this.sceneChangeAnimation.SetOut();
            this.sceneChangeAnimation.onFinishedOut = () =>
            {
                Destroy(this.sceneChangeAnimation.gameObject);
                this.sceneChangeAnimation = null;
                onFinished?.Invoke();
            };
        }
    }

    public void DisableTouch()
    {
        touchDisabler.enabled = true;
    }

    public void EnableTouch()
    {
        touchDisabler.enabled = false;
    }

    public void ShowHeader()
    {
        if (this.header == null)
        {
            this.header = Instantiate(this.headerPrefab, this.headerParent, false);
        }

        this.header.gameObject.SetActive(true);
    }

    public void HideHeader()
    {
        if (this.header != null)
        {
            this.header.gameObject.SetActive(false);
        }
    }

#if DEBUG
    /// <summary>
    /// OnGUI
    /// </summary>
    private void OnGUI()
    {
        this.guiLogViewer?.DrawGUI();
        SceneChanger.currentScene?.DrawGUI();
    }
#endif
}
