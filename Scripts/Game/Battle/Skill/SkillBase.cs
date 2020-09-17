using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// スキル基底
/// </summary>
public abstract class SkillBase
{
    /// <summary>
    /// スキルグループデータ
    /// </summary>
    public Master.SkillGroupData skillGroupData = null;
    /// <summary>
    /// スキルデータ
    /// </summary>
    public Master.SkillData skillData = null;

    /// <summary>
    /// バトルアイテム使用時
    /// </summary>
    public virtual void OnUseBattleItem(){}
    /// <summary>
    /// 増加ダメージ量計算
    /// </summary>
    public virtual float DamageUp(uint power, Battle.Fish fish){ return 0; }
    /// <summary>
    /// 魚被ダメ時
    /// </summary>
    public virtual void OnFishDamaged(Battle.Fish fish){}
    /// <summary>
    /// 被ダメージカット量計算
    /// </summary>
    public virtual float DamageCut(int baseDamage){ return 0; }
    /// <summary>
    /// アイテム再使用間隔短縮
    /// </summary>
    public virtual float ShortenCoolTime(){ return 0; }
    /// <summary>
    /// 開戦時FVゲージ上昇％
    /// </summary>
    public virtual float InitFvGaugeUp(){ return 0; }
    /// <summary>
    /// コイン獲得量上昇％
    /// </summary>
    public virtual float CoinGetUp(){ return 0; }
    /// <summary>
    /// 経験値獲得量上昇％
    /// </summary>
    public virtual float ExpGetUp(){ return 0; }
    /// <summary>
    /// FVゲージ獲得量上昇％
    /// </summary>
    public virtual float FvGetUp(){ return 0; }
}