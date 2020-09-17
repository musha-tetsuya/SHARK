using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 砲台基礎
/// </summary>
public class TurretBase : TimePauseBehaviour
{
    /// <summary>
    /// 台座生成位置
    /// </summary>
    [SerializeField]
    public RectTransform batteryPosition = null;
    /// <summary>
    /// 砲身生成位置
    /// </summary>
    [SerializeField]
    public RectTransform barrelPosition = null;
    /// <summary>
    /// 回転部
    /// </summary>
    [SerializeField]
    public RectTransform rotationParts = null;
    /// <summary>
    /// 銃口
    /// </summary>
    [SerializeField]
    public RectTransform muzzle = null;

    /// <summary>
    /// 台座
    /// </summary>
    [NonSerialized]
    public GameObject battery = null;
    /// <summary>
    /// 台座アニメーター
    /// </summary>
    [NonSerialized]
    public Animator batteryAnimator = null;
    /// <summary>
    /// 砲身
    /// </summary>
    [NonSerialized]
    public GameObject barrel = null;
    /// <summary>
    /// 砲身アニメーター
    /// </summary>
    [NonSerialized]
    public Animator barrelAnimator = null;

    /// <summary>
    /// 台座スプライトKey
    /// </summary>
    [NonSerialized]
    public string batteryKey = "battery_test";
    /// <summary>
    /// 砲身スプライトKey
    /// </summary>
    [NonSerialized]
    public string barrelKey = "barrel_test";
    /// <summary>
    /// 弾丸プレハブKey
    /// </summary>
    [NonSerialized]
    public string bulletKey = "bullet_test";
    /// <summary>
    /// アクセサリプレハブKey
    /// </summary>
    [NonSerialized]
    public string accessoriesKey = "accessories_test";
    /// <summary>
    /// 弾丸プレハブ
    /// </summary>
    public BulletBase bulletPrefab { get; private set; }

    /// <summary>
    /// 停止
    /// </summary>
    public override void Pause(BinaryWriter writer)
    {
        base.Pause(writer);
        if (this.batteryAnimator != null)
        {
            writer.Write(this.batteryAnimator.enabled);
            this.batteryAnimator.enabled = false;
        }
        if (this.barrelAnimator != null)
        {
            writer.Write(this.barrelAnimator.enabled);
            this.barrelAnimator.enabled = false;
        }
    }

    /// <summary>
    /// 再開
    /// </summary>
    public override void Play(BinaryReader reader)
    {
        base.Play(reader);
        if (this.batteryAnimator != null)
        {
            this.batteryAnimator.enabled = reader.ReadBoolean();
        }
        if (this.barrelAnimator != null)
        {
            this.barrelAnimator.enabled = reader.ReadBoolean();
        }
    }

    /// <summary>
    /// 見た目更新
    /// </summary>
    public void Reflesh()
    {
        this.RefleshBattery();
        this.RefleshBarrel();
        this.RefleshBulletPrefab();
    }

    /// <summary>
    /// 台座見た目更新
    /// </summary>
    public void RefleshBattery(GameObject batteryPrefab)
    {
        this.ClearBattery();
        this.battery = Instantiate(batteryPrefab, this.batteryPosition, false);
        this.batteryAnimator = this.battery.GetComponent<Animator>();
    }

    /// <summary>
    /// 台座見た目更新
    /// </summary>
    public void RefleshBattery()
    {
        var prefab = AssetManager.FindHandle<GameObject>(SharkDefine.GetBatteryPrefabPath(this.batteryKey)).asset as GameObject;
        this.RefleshBattery(prefab);
    }

    /// <summary>
    /// 台座破棄
    /// </summary>
    public void ClearBattery()
    {
        if (this.battery != null)
        {
            Destroy(this.battery);
            this.battery = null;
            this.batteryAnimator = null;
        }
    }

    /// <summary>
    /// 砲身見た目更新
    /// </summary>
    public void RefleshBarrel(GameObject barrelPrefab)
    {
        this.ClearBarrel();
        this.barrel = Instantiate(barrelPrefab, this.barrelPosition, false);
        this.barrelAnimator = this.barrel.GetComponent<Animator>();
    }

    /// <summary>
    /// 砲身見た目更新
    /// </summary>
    public void RefleshBarrel()
    {
        var prefab = AssetManager.FindHandle<GameObject>(SharkDefine.GetBarrelPrefabPath(this.barrelKey)).asset as GameObject;
        this.RefleshBarrel(prefab);
    }

    /// <summary>
    /// 砲身破棄
    /// </summary>
    public void ClearBarrel()
    {
        if (this.barrel != null)
        {
            Destroy(this.barrel);
            this.barrel = null;
            this.barrelAnimator = null;
        }
    }

    /// <summary>
    /// 弾丸プレハブ更新
    /// </summary>
    public void RefleshBulletPrefab()
    {
        this.bulletPrefab = AssetManager.FindHandle<BulletBase>(SharkDefine.GetBulletPrefabPath(this.bulletKey)).asset as BulletBase;
    }

    /// <summary>
    /// 弾丸生成
    /// </summary>
    public BulletBase CreateBullet(BulletBase prefab, Transform parent)
    {
        var bullet = Instantiate(prefab, parent, false);
        bullet.transform.position = this.muzzle.position;
        bullet.transform.up = this.muzzle.up;
        return bullet;
    }

    /// <summary>
    /// 弾丸生成
    /// </summary>
    public BulletBase CreateBullet(Transform parent)
    {
        return this.CreateBullet(this.bulletPrefab, parent);
    }

    /// <summary>
    /// 弾丸発射アニメーション再生
    /// </summary>
    public void PlayFiringAnimation()
    {
        this.barrelAnimator.Play("Normal", 0, 0f);
    }

    /// <summary>
    /// 波動砲発射アニメーション再生
    /// </summary>
    public void PlayLaserBeamAnimation()
    {
        this.barrelAnimator.ResetTrigger("HadouhouFinish");
        this.barrelAnimator.Play("Hadouhou", 0, 0f);
    }

    /// <summary>
    /// 波動砲発射アニメーション終了
    /// </summary>
    public void EndLaserBeamAnimation()
    {
        this.barrelAnimator.SetTrigger("HadouhouFinish");
    }
}
