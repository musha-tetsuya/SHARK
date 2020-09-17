using System.Collections;
using UnityEngine;

public class TurretViewer : MonoBehaviour
{
    /// <summary>
    /// 砲台基礎
    /// </summary>
    [SerializeField]
    private TurretBase turret = null;
    /// <summary>
    /// 弾生成用キャンバス
    /// </summary>
    [SerializeField]
    private Canvas bulletCanvas = null;

    [SerializeField]
    private Camera renderTextureCamera = null;
    
    [SerializeField]
    private RenderTexture renderTexture = null;

    private Coroutine coroutine = null;

    public string BatteryKey
    {
        set { turret.batteryKey = value; }
    }
    public string BarrelKey
    {
        set { turret.barrelKey = value; }
    }
    public string BulletKey
    {
        set { turret.bulletKey = value; }
    }

    private void Awake()
    {
        this.renderTextureCamera.targetTexture = this.renderTexture;
    }

    private void OnDestroy()
    {
        this.renderTextureCamera.targetTexture = null;
    }

    public void Reflesh()
    {
        turret.Reflesh();
    }

    public void StartShot()
    {
        if(coroutine != null)
        {
            StopCoroutine(coroutine);
        }

        coroutine = StartCoroutine(ShotLoop());
    }

    public void StopShot()
    {
        if (coroutine != null) {
            StopCoroutine(coroutine);
            coroutine = null;
        }
    }

    private IEnumerator ShotLoop()
    {
        while (true)
        {
            var bullet = this.turret.CreateBullet(this.bulletCanvas.transform);
            //※パーティクルの描画順設定をしてる
            bullet.SetParticleLayer(this.bulletCanvas);
            bullet.movement.speed = 1000;
            //当たり判定の設定
            bullet.bulletCollider.receiver = new BulletHitReceiver {
                bullet = bullet
            };

            this.turret.PlayFiringAnimation();

            yield return new WaitForSeconds(1f);
        }
    }

    /// <summary>
    /// 当たり判定クラス
    /// </summary>
    private class BulletHitReceiver : BulletCollider.IReceiver
    {
        public BulletBase bullet = null;

        void BulletCollider.IReceiver.OnHit(Collider2D collider2D)
        {
            //着弾エフェクト生成
            var effect = Instantiate(this.bullet.landingEffectPrefab, this.bullet.transform.parent, false);
            effect.transform.position = this.bullet.transform.position;
            Destroy(this.bullet.gameObject);
        }
    }
}
