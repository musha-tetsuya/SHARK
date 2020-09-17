using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoginBonusIcon : MonoBehaviour
{
    /// <summary>
    /// commonIcon
    /// </summary>
    [SerializeField]
    CommonIcon commonIcon = null;

    /// <summary>
    /// 日付 テキスト
    /// </summary>
    [SerializeField]
    Text topText = null;

    /// <summary>
    /// 受領可否チェック
    /// </summary>
    [SerializeField]
    Image checkImage = null;

    /// <summary>
    /// アイコン情報セット
    /// </summary>
    public void Set(Master.LoginBonusData master, bool check)
    {
        //CommonIcon表示構築
        this.commonIcon.Set(master.itemType, master.itemId, true);

        this.commonIcon.SetCountText(master.itemNum);

        this.topText.text = Masters.LocalizeTextDB.GetFormat("Day", master.id);

        // チェックイメージOn/Off
        this.checkImage.gameObject.SetActive(check);
    }

    /// <summary>
    /// アイコン情報セット
    /// </summary>
    public void SetSpecialLoginBonus(Master.LoginBonusSpecialData master, bool check)
    {
        //CommonIcon表示構築
        this.commonIcon.Set(master.itemType, master.itemId, true);

        this.commonIcon.SetCountText(master.itemNum);

        this.topText.text = Masters.LocalizeTextDB.GetFormat("Day", master.id);

        // チェックイメージOn/Off
        this.checkImage.gameObject.SetActive(check);
    }
}