using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class IconBase : MonoBehaviour, IEventSystemHandler, IPointerDownHandler, IPointerExitHandler
{
    /// <summary>
    /// ボタン
    /// </summary>
    [SerializeField]
    public Button button = null;

    /// <summary>
    /// 押下中コルーチン
    /// </summary>
    private Coroutine pressingCoroutine = null;
    /// <summary>
    /// クリック時コールバック
    /// </summary>
    public Action onClick = null;
    /// <summary>
    /// 長押し時コールバック
    /// </summary>
    public Action onLongPress = null;

    /// <summary>
    /// クリック時
    /// </summary>
    public void OnClick()
    {
        if (this.pressingCoroutine != null)
        {
            this.ClearPressing();
            this.onClick?.Invoke();
        }
    }

    /// <summary>
    /// 長押し時
    /// </summary>
    private void OnLongPress()
    {
        this.ClearPressing();
        this.onLongPress?.Invoke();
    }

    /// <summary>
    /// 押した瞬間
    /// </summary>
    void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
    {
        this.ClearPressing();

        if (this.button.interactable)
        {
            //長押しコルーチン開始
            this.pressingCoroutine = StartCoroutine(this.Pressing(eventData));
        }
    }

    /// <summary>
    /// 離れた瞬間
    /// </summary>
    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        this.ClearPressing();
    }

    /// <summary>
    /// 押下中
    /// </summary>
    private IEnumerator Pressing(PointerEventData eventData)
    {
        float time = 0f;

        //0.5秒押下で長押し成立
        while (time < 0.5f)
        {
            if (eventData.dragging)
            {
                //ドラッグされたら長押し解除
                this.ClearPressing();
                yield break;
            }

            time += Time.deltaTime;
            yield return null;
        }

        this.OnLongPress();
    }

    /// <summary>
    /// 押下中状態の解除
    /// </summary>
    private void ClearPressing()
    {
        if (this.pressingCoroutine != null)
        {
            StopCoroutine(this.pressingCoroutine);
            this.pressingCoroutine = null;
        }
    }
}
