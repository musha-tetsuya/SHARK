using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 魚コライダデータ
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObject/FishColliderData")]
public class FishColliderData : ScriptableObject
{
    [SerializeField]
    public Vector3 center = Vector3.zero;

    [SerializeField]
    public Vector3 size = Vector3.one;

    [SerializeField]
    public string placementName = null;
}
