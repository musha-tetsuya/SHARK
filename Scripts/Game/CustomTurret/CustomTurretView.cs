using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 砲台ビュー
/// </summary>
public class CustomTurretView : MonoBehaviour
{
    /// <summary>
    /// 砲台基礎
    /// </summary>
    [SerializeField]
    public TurretBase turretBase = null;
    /// <summary>
    /// 弾丸キャンバス
    /// </summary>
    [SerializeField]
    private Canvas bulletCanvas = null;

    /// <summary>
    /// 砲台データ
    /// </summary>
    public UserTurretData turretData { get; private set; } = new UserTurretData();

    /// <summary>
    /// 表示更新
    /// </summary>
    public void Reflesh()
    {
        this.turretBase.batteryKey = Masters.BatteryDB.FindById(this.turretData.batteryMasterId).key;
        this.turretBase.barrelKey = Masters.BarrelDB.FindById(this.turretData.barrelMasterId).key;
        this.turretBase.bulletKey = Masters.BulletDB.FindById(this.turretData.bulletMasterId).key;
        this.turretBase.Reflesh();
    }

    /// <summary>
    /// 活性化するたびにCoroutine呼び出し
    /// </summary>
    private void OnEnable()
    {
        StartCoroutine("ShootAnimationCoroutine");
    }

    /// <summary>
    /// 無効化するたびに弾丸を削除
    /// </summary>
    private void OnDisable()
    {
        // Coroutine実行時、bulletCanvasの子を全部削除
            foreach (Transform child in this.bulletCanvas.transform)
            {
                Destroy(child.gameObject);
            }
    }

    /// <summary>
    /// 1秒ごとに繰り返して弾丸を発砲するアニメーションコルティン
    /// </summary>
    private IEnumerator ShootAnimationCoroutine()
    {
        while (true)
        {
            if (this.turretBase.bulletPrefab != null)
            {
                var bullet = this.turretBase.CreateBullet(this.bulletCanvas.transform);
                bullet.bulletCollider.receiver = new BulletHitReceiver { bullet = bullet };
                bullet.movement.speed = 1000;

                this.turretBase.PlayFiringAnimation();
                yield return new WaitForSeconds(1f);
            }
            else
            {
                yield return null;
            }
        }
    }

    /// <summary>
    /// 弾丸当たり判定レシーバー
    /// </summary>
    private class BulletHitReceiver : BulletCollider.IReceiver
    {
        /// <summary>
        /// 弾丸
        /// </summary>
        public BulletBase bullet = null;

        /// <summary>
        /// ヒット時
        /// </summary>
        void BulletCollider.IReceiver.OnHit(Collider2D collider2D)
        {
            //着弾エフェクト生成
            var effect = Instantiate(this.bullet.landingEffectPrefab, this.bullet.transform.parent, false);
            effect.transform.position = bullet.transform.position;

            //弾丸破棄
            Destroy(this.bullet.gameObject);
        }
    }
}