using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Battle;

/// <summary>
/// バトルアイテムスキル：氷結
/// </summary>
public class SkillFreezing : SkillBattleItemBase
{
    /// <summary>
    /// SE名
    /// </summary>
    public override string seName => SeName.ITEM_ICE;

    /// <summary>
    /// バトルアイテム使用時
    /// </summary>
    public override void OnUseBattleItem()
    {
        for (int i = 0; i < BattleGlobal.instance.fishList.Count; i++)
        {
            var fish = BattleGlobal.instance.fishList[i];

            //画面内にいる魚に対して
            if (fish.fishCollider2D.IsInScreen())
            {
                //氷結実行
                fish.Freezing(this.duration * Masters.MilliSecToSecond);
        
                //状態変化通知
                (SceneChanger.currentScene as MultiBattleScene)?.OnChangeFishCondition(fish);
            }
        }
    }
}
