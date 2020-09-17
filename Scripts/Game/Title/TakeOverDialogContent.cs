using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TakeOverDialogContent : MonoBehaviour
{
    /// <summary>
    /// 引継ぎID入力フィールド
    /// </summary>
    [SerializeField]
    private Text idInputField = null;
    /// <summary>
    /// 引継ぎPASS入力フィールド
    /// </summary>
    [SerializeField]
    private Text passInputField = null;

    /// <summary>
    /// ダイアログ
    /// </summary>
    private SimpleDialog dialog = null;
    /// <summary>
    /// 完了時コールバック
    /// </summary>
    public Action onCompleted = null;

    /// <summary>
    /// コンテンツセット
    /// </summary>
    public void Set(SimpleDialog dialog)
    {
        this.dialog = dialog;

        var yesNo = dialog.AddYesNoButton();

        yesNo.yes.onClick = OnClickTakeOverConfirmYesButton;
        yesNo.no.onClick = OnClickTakeOverConfirmNoButton;
    }

    /// <summary>
    /// Yesボタンクリック時
    /// </summary>
    private void OnClickTakeOverConfirmYesButton()
    {
        if (string.IsNullOrEmpty(this.idInputField.text)) return;
        if (string.IsNullOrEmpty(this.passInputField.text)) return;

        // API実行
        UserApi.CallDeviceChangeCode(
            takeOverId: idInputField.text,
            takeOverPass: passInputField.text,
            onCompleted: (response) =>
            {
                UserData.Get().userId = response.tUsersLogin.userId;
                UserData.Get().password = response.password;

                // 以前の他のIDにログインした機器の場合でも、残っているPlayerPrefsをすべて削除
                PlayerPrefs.DeleteAll();

                this.dialog.Close();
                this.onCompleted?.Invoke();
            },
            onError: (errorCode) =>
            {
                var dialog = SharedUI.Instance.ShowSimpleDialog(true);
                var content = dialog.SetAsMessageDialog(string.Format("ERROR_CODE : {0}", errorCode));
                content.buttonGroup.buttons[0].onClick = dialog.Close;
            });
    }

    /// <summary>
    /// Noボタンクリック時
    /// </summary>
    private void OnClickTakeOverConfirmNoButton()
    {
        this.dialog.Close();
    }
}
