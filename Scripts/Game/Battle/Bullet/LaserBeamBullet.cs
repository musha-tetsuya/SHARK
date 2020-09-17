using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Battle {

/// <summary>
/// 波動砲弾
/// </summary>
public class LaserBeamBullet : Bullet
{
    /// <summary>
    /// ヒット時
    /// </summary>
    protected override void OnHit(FishCollider2D fishCollider2D)
    {
        //着弾エフェクト生成
        this.CreateLandingEffect(fishCollider2D.rectTransform.position);

        //魚にダメージ
        fishCollider2D.fish.OnDamaged(this);
    }
}

}