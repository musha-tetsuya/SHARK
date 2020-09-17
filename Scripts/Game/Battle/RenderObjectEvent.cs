using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Battle {

/// <summary>
/// RenderObjectイベント
/// </summary>
[RequireComponent(typeof(Renderer))]
public class RenderObjectEvent : MonoBehaviour
{
    /// <summary>
    /// Rendererがカメラに描画されているときに呼ばれるコールバック
    /// </summary>
    [SerializeField]
    public UnityEvent onWillRenderObject = new UnityEvent();

    /// <summary>
    /// Rendererがカメラに描画されているときに呼ばれる
    /// </summary>
    private void OnWillRenderObject()
    {
        this.onWillRenderObject.Invoke();
    }
}

}//namespace Battle