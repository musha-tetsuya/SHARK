using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// シングルバトルリザルトの報酬アイコン
/// </summary>
public class SingleBattleResultRewardIcon : MonoBehaviour
{
    /// <summary>
    /// CommonIcon
    /// </summary>
    [SerializeField]
    public CommonIcon icon = null;
    /// <summary>
    /// 初回報酬マーク
    /// </summary>
    [SerializeField]
    private GameObject firstRewardMark = null;

    /// <summary>
    /// セットアップ
    /// </summary>
    public void Setup(SinglePlayApi.RewardData rewardData, bool isFirstReward)
    {
        //CommonIcon表示構築
        this.icon.Set(rewardData.itemType, rewardData.itemId, true);

        //報酬数表示
        if (rewardData.itemNum > 1)
        {
            this.icon.SetCountText(rewardData.itemNum);
        }
        else
        {
            this.icon.countText.text = null;
        }

        //初回報酬マークの表示切り替え
        this.firstRewardMark.SetActive(isFirstReward);
    }
}
