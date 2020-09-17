using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Battle {

/// <summary>
/// 貫通弾
/// </summary>
[RequireComponent(typeof(BulletBouncing))]
public class PenetrationBullet : Bullet
{
    /// <summary>
    /// 寿命
    /// </summary>
    private float duration = 0f;
    /// <summary>
    /// サーバータイムスタンプ
    /// </summary>
    private int timeStamp = 0;
    /// <summary>
    /// 破棄時コールバック
    /// </summary>
    private Action<Bullet> onDestroy = null;

    /// <summary>
    /// OnDestroy
    /// </summary>
    protected override void OnDestroy()
    {
        this.onDestroy?.Invoke(this);
        base.OnDestroy();
    }

    /// <summary>
    /// セットアップ
    /// </summary>
    public void Setup(
        bool isFvAttack,
        uint power,
        uint fvRate,
        Master.BulletData bulletData,
        SkillGroupManager skill,
        uint speed,
        float duration)
    {
        base.Setup(isFvAttack, power, fvRate, bulletData, skill);
        this.bulletBase.movement.speed = speed;
        this.duration = duration;
    }

    /// <summary>
    /// タイムスタンプセット
    /// </summary>
    public override void SetTimeStamp(int timeStamp)
    {
        this.timeStamp = timeStamp;
        this.bulletBase.movement.timeStamp = timeStamp;
    }

    /// <summary>
    /// 破棄時コールバックセット
    /// </summary>
    public override void SetOnDestroyCallback(Action<Bullet> onDestroy)
    {
        this.onDestroy = onDestroy;
    }

    /// <summary>
    /// Update
    /// </summary>
    private void Update()
    {
        if (this.duration <= 0f)
        {
            //寿命が尽きたので終了
            Destroy(this.gameObject);
            return;
        }

        float deltaTime = BattleGlobal.GetDeltaTime(ref this.timeStamp);
        this.duration -= deltaTime;
    }

    /// <summary>
    /// 停止
    /// </summary>
    public override void Pause(BinaryWriter writer)
    {
        base.Pause(writer);
        this.GetComponent<BulletBouncing>().enabled = false;
    }

    /// <summary>
    /// 再開
    /// </summary>
    public override void Play(BinaryReader reader)
    {
        base.Play(reader);
        this.GetComponent<BulletBouncing>().enabled = true;
    }

    /// <summary>
    /// ヒット時
    /// </summary>
    protected override void OnHit(FishCollider2D fishCollider2D)
    {
        //着弾エフェクト再生
        this.CreateLandingEffect(fishCollider2D.rectTransform.position);

        //魚にダメージ
        fishCollider2D.fish.OnDamaged(this);
    }

}//class PenetrationBullet

}//namespace Battle