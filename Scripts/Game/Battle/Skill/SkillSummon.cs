using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;
using Battle;

/// <summary>
/// バトルアイテムスキル：召喚
/// </summary>
public class SkillSummon : SkillBattleItemBase
{
    /// <summary>
    /// 召喚可能な魚
    /// </summary>
    [JsonProperty("key")]
    public string key = null;
    /// <summary>
    /// SE名
    /// </summary>
    public override string seName => SeName.ITEM_SUMMON;

    /// <summary>
    /// バトルアイテム使用時
    /// </summary>
    public override void OnUseBattleItem()
    {
        (SceneChanger.currentScene as MultiBattleScene).OnSummon(this.key);
    }
}
