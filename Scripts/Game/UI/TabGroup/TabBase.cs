using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

/// <summary>
/// タブ
/// </summary>
public class TabBase : MonoBehaviour
{
    /// <summary>
    /// クリック時コールバック
    /// </summary>
    [SerializeField]
    public UnityEvent onClick = null;
    /// <summary>
    /// ボタン
    /// </summary>
    [SerializeField]
    private Button button = null;
    /// <summary>
    /// イメージ
    /// </summary>
    [SerializeField]
    private Image image = null;
    /// <summary>
    /// アクティブ時スプライト
    /// </summary>
    [SerializeField]
    private Sprite activeSprite = null;
    /// <summary>
    /// 非アクティブ時スプライト
    /// </summary>
    [SerializeField]
    private Sprite inActiveSprite = null;
    /// <summary>
    /// グレーアウト対象
    /// </summary>
    [SerializeField]
    private Graphic[] grayoutTarget = null;

    /// <summary>
    /// アクティブかどうか
    /// </summary>
    public bool isActive { get; private set; }

    /// <summary>
    /// クリック時
    /// </summary>
    public virtual void OnClick()
    {
        //すでにアクティブなのでreturn
        if (this.isActive) return;

        //アクティブになる
        this.SetActive(true);

        //コールバック
        this.onClick?.Invoke();
    }

    /// <summary>
    /// アクティブ切り替え
    /// </summary>
    public void SetActive(bool active)
    {
        if (this.isActive != active)
        {
            this.isActive = active;
            this.OnChangeActive();
        }
    }

    /// <summary>
    /// アクティブ変化時
    /// </summary>
    protected virtual void OnChangeActive()
    {
        //スプライト切り替え
        this.image.sprite = this.isActive ? this.activeSprite : this.inActiveSprite;
    }

    /// <summary>
    /// グレーアウトON/OFF
    /// </summary>
    public virtual void SetGrayout(bool isGrayout)
    {
        this.button.interactable = !isGrayout;

        foreach (var graphic in this.grayoutTarget)
        {
            graphic.material = isGrayout ? SharedUI.Instance.grayScaleMaterial : null;
        }
    }
}
