using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;

/// <summary>
/// スキルグループ管理
/// </summary>
public class SkillGroupManager : List<SkillBase>
{
    /// <summary>
    /// スキルグループ追加
    /// </summary>
    public void AddSkillGroup(uint skillGroupId)
    {
        var skillGroupDataList = Masters.SkillGroupDB.GetList().FindAll(x => x.groupId == skillGroupId);

        for (int i = 0, imax = skillGroupDataList.Count; i < imax; i++)
        {
            var skillId = skillGroupDataList[i].skillId;
            var skillData = Masters.SkillDB.FindById(skillId);
            var skillType = Type.GetType("Skill" + (SkillType)skillData.skillType);
            var skill = JsonConvert.DeserializeObject(skillGroupDataList[i].effectValue, skillType) as SkillBase;
            skill.skillGroupData = skillGroupDataList[i];
            skill.skillData = skillData;
            this.Add(skill);
        }
    }

    /// <summary>
    /// スキルグループ追加
    /// </summary>
    public void AddRangeSkillGroup(IEnumerable<uint> skillGroupIds)
    {
        foreach (uint id in skillGroupIds)
        {
            this.AddSkillGroup(id);
        }
    }

    /// <summary>
    /// バトルアイテム使用時
    /// </summary>
    public void OnUseBattleItem()
    {
        for (int i = 0; i < this.Count; i++)
        {
            this[i].OnUseBattleItem();
        }
    }

    /// <summary>
    /// 魚へのダメージ増加量計算
    /// </summary>
    public float DamageUp(uint power, Battle.Fish fish)
    {
        return this.Select(skill => skill.DamageUp(power, fish)).Sum();
    }

    /// <summary>
    /// 魚被ダメ時
    /// </summary>
    public void OnFishDamaged(Battle.Fish fish)
    {
        for (int i = 0; i < this.Count; i++)
        {
            this[i].OnFishDamaged(fish);
        }
    }

    /// <summary>
    /// 被ダメージカット量計算
    /// </summary>
    public float DamageCut(int baseDamage)
    {
        return this.Select(skill => skill.DamageCut(baseDamage)).Sum();
    }

    /// <summary>
    /// アイテム再使用間隔短縮
    /// </summary>
    public float ShortenCoolTime()
    {
        return this.Select(skill => skill.ShortenCoolTime()).Sum();
    }

    /// <summary>
    /// 開戦時FVゲージ上昇％
    /// </summary>
    public float InitFvGaugeUp()
    {
        return this.Select(skill => skill.InitFvGaugeUp()).Sum();
    }

    /// <summary>
    /// コイン獲得量上昇％
    /// </summary>
    public float CoinGetUp()
    {
        return this.Select(skill => skill.CoinGetUp()).Sum();
    }

    /// <summary>
    /// 経験値獲得量上昇％
    /// </summary>
    public float ExpGetUp()
    {
        return this.Select(skill => skill.ExpGetUp()).Sum();
    }

    /// <summary>
    /// FVゲージ獲得量上昇％
    /// </summary>
    public float FvGetUp()
    {
        return this.Select(skill => skill.FvGetUp()).Sum();
    }
}