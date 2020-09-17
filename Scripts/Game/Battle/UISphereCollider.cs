#if UNITY_EDITOR
//#define UI_SPHERE_COLLIDER_DEBUG
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Battle {

/// <summary>
/// UISphereCollider
/// <para>
/// 3DのSphereColliderを2DのUIキャンバス上に落とし込むやつ
/// </para>
/// </summary>
[Serializable]
public class UISphereCollider
{
    /// <summary>
    /// SphereCollider
    /// </summary>
    [SerializeField]
    private SphereCollider sphereCollider = null;

    /// <summary>
    /// Transform
    /// </summary>
    private Transform cachedTransform = null;
    /// <summary>
    /// 3Dカメラ
    /// </summary>
    private Camera camera3d = null;
    /// <summary>
    /// 2Dカメラ
    /// </summary>
    private Camera camera2d = null;
    /// <summary>
    /// キャンバスのRectTransform
    /// </summary>
    private RectTransform canvasRect = null;
    /// <summary>
    /// スケール最大値
    /// </summary>
    private float maxScale = 1f;
    /// <summary>
    /// キャンバス上での半径
    /// </summary>
    public float radius { get; private set; }
    /// <summary>
    /// キャンバス上での座標
    /// </summary>
    [NonSerialized]
    public Vector2 position = Vector2.zero;

#if UI_SPHERE_COLLIDER_DEBUG
    /// <summary>
    /// デバッグ表示用イメージ
    /// </summary>
    private Image debugImage = null;
#endif

    /// <summary>
    /// セットアップ
    /// </summary>
    public void Setup(Camera camera3d, Camera camera2d, RectTransform canvasRect)
    {
        this.cachedTransform = this.sphereCollider.transform;
        this.camera3d = camera3d;
        this.camera2d = camera2d;
        this.canvasRect = canvasRect;
        this.maxScale = Mathf.Max(this.cachedTransform.lossyScale.x, this.cachedTransform.lossyScale.y, this.cachedTransform.lossyScale.z);
        this.radius = this.GetUIRadius();
#if UI_SPHERE_COLLIDER_DEBUG
        this.DebugCreateImage();
#endif
        this.UpdateUIPosition();
    }

    /// <summary>
    /// UI半径の計算
    /// </summary>
    private float GetUIRadius()
    {
        //ワールド座標系でのコライダ半径の位置
        Vector3 worldRadiusPosition = Vector3.forward * this.sphereCollider.radius * this.maxScale;
        //カメラ角度を考慮して補正
        worldRadiusPosition = Quaternion.Euler(-this.camera3d.transform.eulerAngles) * worldRadiusPosition;

        //スクリーン座標系に変換
        Vector3 screenRadiusPosition = this.camera3d.WorldToScreenPoint(worldRadiusPosition);

        //UI座標系に変換
        Vector2 uiRadiusPosition = Vector2.zero;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(this.canvasRect, screenRadiusPosition, this.camera2d, out uiRadiusPosition);

        //長さを返却
        return uiRadiusPosition.magnitude;
    }

    /// <summary>
    /// UI座標の更新
    /// </summary>
    public void UpdateUIPosition()
    {
        //進行方向を考慮したワールド座標系でのコライダの位置
        Vector3 worldRadiusPosition = Quaternion.LookRotation(this.cachedTransform.forward) * (this.sphereCollider.center * this.maxScale) + this.cachedTransform.position;

        //スクリーン座標系に変換
        Vector3 screenRadiusPosition = this.camera3d.WorldToScreenPoint(worldRadiusPosition);

        //UI座標系に変換
        RectTransformUtility.ScreenPointToLocalPointInRectangle(this.canvasRect, screenRadiusPosition, this.camera2d, out this.position);

#if UI_SPHERE_COLLIDER_DEBUG
        //デバッグ用コライダ範囲イメージ座標を更新
        this.DebugUpdateImagePosition();
#endif
    }

#if UI_SPHERE_COLLIDER_DEBUG
    /// <summary>
    /// デバッグ用にコライダ範囲を可視化
    /// </summary>
    private void DebugCreateImage()
    {
        var gobj = new GameObject("UISphereCollider DebugImage");
        gobj.transform.SetParent(this.canvasRect);
        gobj.transform.SetAsFirstSibling();
        gobj.transform.localPosition = Vector3.zero;
        gobj.transform.localEulerAngles = Vector3.zero;
        gobj.transform.localScale = Vector3.one;
        this.debugImage = gobj.AddComponent<Image>();
        this.debugImage.rectTransform.sizeDelta = Vector2.one * this.radius * 2f;
        this.debugImage.raycastTarget = false;
        this.debugImage.sprite = UnityEditor.AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        this.debugImage.color = new Color(1f, 1f, 1f, 0.5f);
    }

    /// <summary>
    /// デバッグ用のコライダ範囲イメージ座標を更新
    /// </summary>
    private void DebugUpdateImagePosition()
    {
        if (this.debugImage != null)
        {
            this.debugImage.rectTransform.anchoredPosition = this.position;
        }
    }
#endif

}//class UISphereCollider

}//namespace Battle