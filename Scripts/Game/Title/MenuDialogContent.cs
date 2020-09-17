using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MenuDialogContent : MonoBehaviour
{
    /// <summary>
    /// ユーザー引継ぎコンテンツPrefab
    /// </summary>
    [SerializeField]
    private TakeOverDialogContent takeOverPrefab = null;
    /// <summary>
    /// 自身のダイアログ
    /// </summary>
    private SimpleDialog dialog = null;
    /// <summary>
    /// ユーザーデータ引き継ぎ成功時コールバック
    /// </summary>
    private Action onTakeOverSuccess = null;

    /// <summary>
    /// メニューダイアログ
    /// </summary>
    public void Set(SimpleDialog dialog, Action onTakeOverSuccess)
    {
        this.dialog = dialog;
        this.onTakeOverSuccess = onTakeOverSuccess;
    }

    /// <summary>
    /// キャッシュ削除ボタンクリック時
    /// </summary>
    public void OnTapCleanCacheButton()
    {
        //確認ダイアログ表示
        var confirmDialog = SharedUI.Instance.ShowSimpleDialog();
        var confirmDialogContent = confirmDialog.SetAsYesNoMessageDialog(Masters.LocalizeTextDB.Get("CacheClearDescription"));

        //YES
        confirmDialogContent.yesNo.yes.onClick = () =>
        {
            //キャッシュ削除処理
            string path = AssetManager.GetAssetBundleDirectoryPath();
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }

            //終了通知ダイアログ表示
            var finishedDialog = SharedUI.Instance.ShowSimpleDialog();
            var finishedDialogContent = finishedDialog.SetAsMessageDialog(Masters.LocalizeTextDB.Get("CacheClearFinished"));
            finishedDialogContent.buttonGroup.buttons[0].onClick = () =>
            {
                //終了通知ダイアログと確認ダイアログ両方閉じる
                finishedDialog.Close();
                confirmDialog.Close();
            };
        };

        //NO
        confirmDialogContent.yesNo.no.onClick = () =>
        {
            //確認ダイアログ閉じる
            confirmDialog.Close();
        };
    }

    /// <summary>
    /// ユーザーデータ引継ぎボタンクリック時
    /// </summary>
    public void OnTapDataMovementButton()
    {
        //引き継ぎダイアログ表示
        var takeOverDialog = SharedUI.Instance.ShowSimpleDialog();
        var content = takeOverDialog.AddContent(this.takeOverPrefab);
        content.Set(takeOverDialog);

        //引き継ぎ成功時
        content.onCompleted = () =>
        {
            this.dialog.Close();
            this.dialog.onClose = this.onTakeOverSuccess;
        };
    }

    /// <summary>
    /// お知らせボタンクリック時
    /// </summary>
    public void OnTapNoticeButton()
    {
        Debug.Log("お知らせ");
        var url = "http://dev-fish-1.sunchoi.co.jp/notice";
        Application.OpenURL(url);
    }

    /// <summary>
    /// お問い合わせボタンクリック時
    /// </summary>
    public void OnClickContactUsButton()
    {
        //TODO:お問い合わせページ用意する
        Application.OpenURL("http://dev-fish-1.sunchoi.co.jp/notice");
    }
}
