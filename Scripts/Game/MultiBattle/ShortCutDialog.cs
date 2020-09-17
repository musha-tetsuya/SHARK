using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// マルチバトルスライドメニューダイアログ
/// </summary>
public class ShortCutDialog : DialogBase
{
    [SerializeField]
    private MissionDialog missionDialogPreafab = null;

    [SerializeField]
    private MultiBattleShop shopDialogPreafab = null;

    [SerializeField]
    private UserOptionDialogContent optionDialogContentPrefab = null;

    public Action onClickBackHomeButton = null;

    public Action<MissionDialog> onMissionChallenge = null;

    public void OnClickMissionButton()
    {
        MissionDialog.Open(this.missionDialogPreafab, (content) =>
        {
            if (!string.IsNullOrEmpty(content.nextSceneName))
            {
                this.onMissionChallenge?.Invoke(content);
                this.Close();
            }
        });
    }

    public void OnClickShopButton()
    {
        MultiBattleShop.Open(this.shopDialogPreafab);
    }

    public void OnClickSettingButton()
    {
        var dialog = SharedUI.Instance.ShowSimpleDialog();
        var content = dialog.AddContent(this.optionDialogContentPrefab);
        content.Setup(dialog);
    }

    public void OnClickBackHomeButton()
    {
        this.onClickBackHomeButton?.Invoke();
    }
}
