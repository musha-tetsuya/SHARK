using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 砲台スクロールビュー要素
/// </summary>
public class CustomTurretScrollViewItem : MonoBehaviour
{
    /// <summary>
    /// 砲台基礎
    /// </summary>
    [SerializeField]
    private TurretBase turretBase = null;
    /// <summary>
    /// 装備中マーク
    /// </summary>
    [SerializeField]
    private GameObject equippedMark = null;
    /// <summary>
    /// 選択中マーク
    /// </summary>
    [SerializeField]
    private GameObject selectedMark = null;

    /// <summary>
    /// 砲台データ
    /// </summary>
    public UserTurretData turretData { get; private set; }
    /// <summary>
    /// クリック時コールバック
    /// </summary>
    private Action<CustomTurretScrollViewItem> onClick = null;

    /// <summary>
    /// 表示構築
    /// </summary>
    public void Set(UserTurretData data, bool isEquipped, bool isSelected, Action<CustomTurretScrollViewItem> onClick)
    {
        this.turretData = data;
        this.turretBase.batteryKey = Masters.BatteryDB.FindById(this.turretData.batteryMasterId).key;
        this.turretBase.barrelKey = Masters.BarrelDB.FindById(this.turretData.barrelMasterId).key;
        this.turretBase.bulletKey = Masters.BulletDB.FindById(this.turretData.bulletMasterId).key;
        this.turretBase.Reflesh();

        //装備中マークON/OFF
        this.equippedMark.SetActive(isEquipped);

        //選択中マークON/OFF
        this.selectedMark.SetActive(isSelected);

        //クリック時処理登録
        this.onClick = onClick;
    }

    /// <summary>
    /// クリック時
    /// </summary>
    public void OnClick()
    {
        this.onClick?.Invoke(this);
        SoundManager.Instance.PlaySe(SeName.PARTS_CHANGE);
    }
}
