using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// BoxCollliderの８頂点
/// </summary>
public class BoxColliderPoints : MonoBehaviour
{
    /// <summary>
    /// ８頂点
    /// </summary>
    [SerializeField]
    private Transform[] p = null;

    /// <summary>
    /// Transformのキャッシュ
    /// </summary>
    public Transform cachedTransform { get; private set; }

    /// <summary>
    /// ８頂点
    /// </summary>
    public Transform this[int i] => this.p[i];

    /// <summary>
    /// BoxCollider
    /// </summary>
    private BoxCollider boxCollider = null;

    /// <summary>
    /// Awake
    /// </summary>
    private void Awake()
    {
        this.cachedTransform = this.transform;
        this.boxCollider = this.cachedTransform.parent.GetComponent<BoxCollider>();
        this.Recalc();
    }

    /// <summary>
    /// 再計算
    /// </summary>
    public void Recalc()
    {
        this.cachedTransform.localPosition = this.boxCollider.center;
        this.cachedTransform.localScale = this.boxCollider.size;
    }
}
