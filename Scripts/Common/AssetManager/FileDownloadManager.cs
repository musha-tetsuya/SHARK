using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// ファイルダウンロード管理
/// </summary>
public class FileDownloadManager
{
    /// <summary>
    /// 最大スレッド数
    /// </summary>
    private const int MAX_THREAD_COUNT = 1;//マルチスレッド上手く組めなかったので1でorz

    /// <summary>
    /// エラーフラグ
    /// </summary>
    private bool isError = false;
    /// <summary>
    /// 総数
    /// </summary>
    private int totalCount = 0;
    /// <summary>
    /// ディレクトリ
    /// </summary>
    private string directory = null;
    /// <summary>
    /// ダウンロードハンドルリスト
    /// </summary>
    private List<FileDownloadHandle> handles = new List<FileDownloadHandle>();
    /// <summary>
    /// ダウンロード完了時コールバック
    /// </summary>
    public Action onCompleted = null;

    /// <summary>
    /// ディレクトリ設定
    /// </summary>
    public void SetDirectory(string directory)
    {
        this.directory = directory;

        //ディレクトリ作成
        Directory.CreateDirectory(directory);

#if UNITY_IOS
        //iOS時は保存したファイルをiCloudにバックアップしないようフラグを立てる
        UnityEngine.iOS.Device.SetNoBackupFlag(directory);
#endif
    }

    /// <summary>
    /// ダウンロード
    /// </summary>
    public void Add(FileDownloadHandle handle)
    {
        handle.onCompleted += this.OnDownloadCompleted;
        this.handles.Add(handle);
    }

    /// <summary>
    /// ダウンロード
    /// </summary>
    public void Add(string version, string fileName)
    {
        this.Add(new FileDownloadHandle(version, fileName));
    }

    /// <summary>
    /// ダウンロード開始
    /// </summary>
    public void DownloadStart()
    {
        this.totalCount = this.handles.Count;
        this.DownloadIfCan();
    }

    /// <summary>
    /// 可能ならダウンロード処理
    /// </summary>
    private void DownloadIfCan()
    {
        if (this.isError)
        {
            return;
        }

        int busyCount = 0;
        FileDownloadHandle handle = null;

        for (int i = 0; i < this.handles.Count && busyCount < MAX_THREAD_COUNT; i++)
        {
            if (this.handles[i].status == FileDownloadHandle.Status.Downloading)
            {
                //処理中リクエスト数をカウント
                busyCount++;
            }
            else if (handle == null)
            {
                //未処理ハンドルの検索
                handle = this.handles[i];
            }
        }

        if (busyCount < MAX_THREAD_COUNT && handle != null)
        {
            //余裕があるなら未処理リクエストの処理を開始
            handle.Send();
            this.DownloadIfCan();
        }
    }

    /// <summary>
    /// ダウンロード完了時
    /// </summary>
    private void OnDownloadCompleted(FileDownloadHandle handle)
    {
        if (handle.status == FileDownloadHandle.Status.Error)
        {
            //エラー発生
            this.isError = true;
        }

        //エラー発生中の場合
        if (this.isError)
        {
            //処理中のものが無くなったら
            if (!this.handles.Exists(x => x.status == FileDownloadHandle.Status.Downloading))
            {
                //エラーダイアログ表示してリトライさせる
                var dialog = SharedUI.Instance.ShowSimpleDialog(true);
                var content = dialog.SetAsMessageDialog(Masters.LocalizeTextDB.Get("ConnectErrorMessage"));
                content.buttonGroup.buttons[0].onClick = dialog.Close;
                dialog.onClose = () =>
                {
                    this.isError = false;
                    this.DownloadIfCan();
                };
            }
            return;
        }

        if (handle.isAutoSave)
        {
            //ファイル保存
            this.SaveFile(handle);
        }

        //完了したハンドルの除去
        this.handles.Remove(handle);

        if (this.handles.Count > 0)
        {
            //可能なら次のファイルダウンロード開始
            this.DownloadIfCan();
        }
        else
        {
            //完了通知
            this.onCompleted?.Invoke();
        }
    }

    /// <summary>
    /// ファイル保存
    /// </summary>
    public void SaveFile(FileDownloadHandle handle)
    {
        this.SaveFile(handle.hash, handle.bytes);
    }

    /// <summary>
    /// ファイル保存
    /// </summary>
    public void SaveFile(string hash, byte[] bytes)
    {
        string path = Path.Combine(this.directory, hash);
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        File.WriteAllBytes(path, bytes);
    }

    /// <summary>
    /// 進捗率取得
    /// </summary>
    public float GetProgress()
    {
        return (this.totalCount == 0) ? 0f : (float)(this.totalCount - this.handles.Count) / this.totalCount;
    }
}
