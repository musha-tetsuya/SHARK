using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

/// <summary>
/// リピートボタン
/// </summary>
public class RepeatButton : MonoBehaviour, IEventSystemHandler, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    /// <summary>
    /// 初回判定後、次の判定までのDelay
    /// </summary>
    [SerializeField]
    private float delay = 0.5f;
    /// <summary>
    /// リピート間隔
    /// </summary>
    [SerializeField]
    private float interval = 0.1f;
    /// <summary>
    /// ボタンイベント
    /// </summary>
    [SerializeField]
    public UnityEvent onPressed = null;

    /// <summary>
    /// コルーチン
    /// </summary>
    private Coroutine repeatCoroutine = null;

    /// <summary>
    /// ボタン押下時
    /// </summary>
    void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
    {
        //既にリピート処理中だったら終了させる
        if (this.repeatCoroutine != null)
        {
            StopCoroutine(this.repeatCoroutine);
            this.repeatCoroutine = null;
        }

        //リピート処理開始
        this.repeatCoroutine = StartCoroutine(this.Repeat());
    }

    /// <summary>
    /// ボタン解放時
    /// </summary>
    void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
    {
        //処理を終了させる
        if (this.repeatCoroutine != null)
        {
            StopCoroutine(this.repeatCoroutine);
            this.repeatCoroutine = null;
        }
    }

    /// <summary>
    /// ボタン範囲外に出た時
    /// </summary>
    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        //処理を終了させる
        if (this.repeatCoroutine != null)
        {
            StopCoroutine(this.repeatCoroutine);
            this.repeatCoroutine = null;
        }
    }

    /// <summary>
    /// リピート処理
    /// </summary>
    private IEnumerator Repeat()
    {
        //まず一発押下イベント呼ぶ
        this.onPressed.Invoke();

        //次の処理までDelay時間待つ
        yield return new WaitForSeconds(this.delay);

        while (true)
        {
            //繰り返し押下イベント発火
            this.onPressed.Invoke();

            //一定間隔待つ
            yield return new WaitForSeconds(this.interval);
        }
    }

}//class RepeatButton
