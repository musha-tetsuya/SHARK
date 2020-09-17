using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Battle {

/// <summary>
/// バトル用弾丸基底クラス
/// </summary>
public abstract class Bullet : TimePauseBehaviour, BulletCollider.IReceiver
{
    /// <summary>
    /// 弾丸基礎
    /// </summary>
    public BulletBase bulletBase { get; private set; }
    /// <summary>
    /// 複数回ヒット判定するかどうか
    /// </summary>
    protected virtual bool isMultipleHit => false;
    /// <summary>
    /// ヒットした魚のリスト（複数回ヒットさせないため１回ヒットしたらリストに詰めて管理する）
    /// </summary>
    private List<GameObject> hittedFishList = new List<GameObject>();
    /// <summary>
    /// BET
    /// </summary>
    public uint bet { get; private set; }
    /// <summary>
    /// 威力
    /// </summary>
    public uint power { get; private set; }
    /// <summary>
    /// FV上昇率
    /// </summary>
    public uint fvRate { get; private set; }
    /// <summary>
    /// 弾丸データ（着弾音のための）
    /// </summary>
    public Master.BulletData bulletData { get; private set; }
    /// <summary>
    /// スキル効果
    /// </summary>
    public SkillGroupManager skill { get; private set; }
    /// <summary>
    /// FVアタック弾かどうか
    /// </summary>
    public bool isFvAttack { get; private set; }

    /// <summary>
    /// Awake
    /// </summary>
    protected override void Awake()
    {
        base.Awake();

        this.bulletBase = this.GetComponent<BulletBase>();
        
        if (BattleGlobal.instance.userData != null)
        {
            this.bet = BattleGlobal.instance.userData.currentBetData.maxBet;
        }

        if (this.bulletBase.bulletCollider != null)
        {
            this.bulletBase.bulletCollider.receiver = this;
        }
    }

    /// <summary>
    /// Start
    /// </summary>
    protected virtual void Start()
    {
        this.bulletBase.SetParticleLayer(BattleGlobal.instance.bulletCanvas);
    }

    /// <summary>
    /// セットアップ
    /// </summary>
    public void Setup(
        bool isFvAttack,
        uint power,
        uint fvRate,
        Master.BulletData bulletData,
        SkillGroupManager skill)
    {
        this.power = power;
        this.fvRate = fvRate;
        this.isFvAttack = isFvAttack;
        this.bulletData = bulletData;
        this.skill = skill;
    }

    /// <summary>
    /// 着弾エフェクト生成
    /// </summary>
    protected LandingEffect CreateLandingEffect(Vector3 position)
    {
        SoundManager.Instance.PlaySe(this.bulletData.seName);
        var effect = Instantiate(this.bulletBase.landingEffectPrefab, BattleGlobal.instance.landingEffectArea, false);
        effect.transform.position = position;
        (SceneChanger.currentScene as MultiBattleScene)?.OnCreateLandingEffect(effect);
        return effect;
    }

    /// <summary>
    /// ヒット時
    /// </summary>
    public virtual void OnHit(Collider2D collider2D)
    {
        //既にヒット済みの魚には再度ヒットしない
        if (!this.isMultipleHit && this.hittedFishList.Exists(x => x != null && x == collider2D.gameObject))
        {
            return;
        }

        //当たったコライダが魚じゃないならreturn
        var fishCollider2D = collider2D.gameObject.GetComponent<FishCollider2D>();
        if (fishCollider2D == null)
        {
            return;
        }

        //画面外の魚にはヒットしない
        if (!fishCollider2D.IsInScreen())
        {
            return;
        }

        //再度ヒットさせないよう、ヒット済み魚としてリストに追加
        this.hittedFishList.Add(collider2D.gameObject);

        //既に死亡しているなら何もしない
        if (fishCollider2D.fish.isDead)
        {
            return;
        }

        //ヒット通知
        this.OnHit(fishCollider2D);
    }

    /// <summary>
    /// ヒット通知
    /// </summary>
    protected abstract void OnHit(FishCollider2D fishCollider2D);

    /// <summary>
    /// タイムスタンプセット
    /// </summary>
    public virtual void SetTimeStamp(int timeStamp){}

    /// <summary>
    /// 破棄時コールバックセット
    /// </summary>
    public virtual void SetOnDestroyCallback(Action<Bullet> onDestroy){}

}//class Bullet

}//namespace Battle