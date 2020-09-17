using UnityEngine;
using System.Linq;

public class ProductTurretContent : MonoBehaviour
{
    /// <summary>
    /// 砲弾パーツ
    /// </summary>
    [SerializeField]
    private ShopCannonParts bulletParts = null;
    /// <summary>
    /// 砲身パーツ
    /// </summary>
    [SerializeField]
    private ShopCannonParts barrelParts = null;
    /// <summary>
    /// 台座パーツ
    /// </summary>
    [SerializeField]
    private ShopBatteryParts batteryParts = null;
    /// <summary>
    /// セットスキル
    /// </summary>
    [SerializeField]
    private ShopTurretInfoContent seriesSkillContent = null;
    /// <summary>
    /// FVA
    /// </summary>
    [SerializeField]
    private ShopTurretInfoContent fvaContent = null;

    /// <summary>
    /// 砲台ビューワー
    /// </summary>
    private TurretViewer turretViewer = null;

    /// <summary>
    /// OnDestroy
    /// </summary>
    private void OnDestroy()
    {
        if (this.turretViewer != null)
        {
            Destroy(this.turretViewer.gameObject);
            this.turretViewer = null;
        }
    }

    /// <summary>
    /// 表示情報の設定
    /// </summary>
    public void SetInfo(ProductCannon product, TurretViewer turretViewerPrefab)
    {
        var battery = Masters.BatteryDB.FindById(product.cannonSetData.batteryId);
        var barrel = Masters.BarrelDB.FindById(product.cannonSetData.barrelId);
        var bullet = Masters.BulletDB.FindById(product.cannonSetData.bulletId);
        var config = Masters.ConfigDB.FindById(1);

        //台座設定
        float gaugeFvPoint = (battery.fvPoint < config.maxBatteryFvPoint / 5) ? 0.2f : (float)battery.fvPoint / config.maxBatteryFvPoint;
        this.batteryParts.SetInfo((uint)ItemType.Battery, battery.id, gaugeFvPoint, battery.fvAttackId);

        //砲身設定
        float gaugeSpeed = (barrel.speed < config.maxBarrelSpeed / 5) ? 0.2f : gaugeSpeed = (float)barrel.speed / config.maxBarrelSpeed;
        this.barrelParts.SetInfo((uint)ItemType.Barrel, barrel.id, gaugeSpeed);

        //砲弾設定
        float gaugePower = (bullet.power < config.maxBulletPower / 5) ? 0.2f : (float)bullet.power / config.maxBulletPower;
        this.bulletParts.SetInfo((uint)ItemType.Bullet, bullet.id, gaugePower);

        //セットスキルの設定
        if (battery.seriesId == barrel.seriesId && battery.seriesId == bullet.seriesId)
        {
            this.seriesSkillContent.SetSerieseSkillInfo(battery.seriesId);
        }
        
        //FVA情報設定
        this.fvaContent.SetFVAInfo(battery.fvAttackId);

        //砲台ビューワー表示
        this.turretViewer = Instantiate(turretViewerPrefab, null, false);
        this.turretViewer.BatteryKey = battery.key;
        this.turretViewer.BarrelKey = barrel.key;
        this.turretViewer.BulletKey = bullet.key;
        this.turretViewer.Reflesh();
        this.turretViewer.StartShot();
    }
}
