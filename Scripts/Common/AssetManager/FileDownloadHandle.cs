using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ファイルダウンロードハンドル
/// </summary>
public class FileDownloadHandle
{
    /// <summary>
    /// ステータス
    /// </summary>
    public enum Status
    {
        None,
        Downloading,
        Success,
        Error,
    }

    /// <summary>
    /// ステータス
    /// </summary>
    public Status status { get; private set; }
    /// <summary>
    /// 自動保存フラグ
    /// </summary>
    public bool isAutoSave = true;
    /// <summary>
    /// バージョン
    /// </summary>
    private string version = null;
    /// <summary>
    /// 名前
    /// </summary>
    public string name { get; private set; }
    /// <summary>
    /// ハッシュ
    /// </summary>
    public string hash { get; private set; }
    /// <summary>
    /// ダウンロードしたデータ
    /// </summary>
    public byte[] bytes { get; private set; }
    /// <summary>
    /// ダウンロード完了時コールバック
    /// </summary>
    public event Action<FileDownloadHandle> onCompleted = null;

    /// <summary>
    /// construct
    /// </summary>
    public FileDownloadHandle(string version, string name)
    {
        this.version = version;
        this.name = name;
        this.hash = name.GetHashString();
    }

    /// <summary>
    /// URL取得
    /// </summary>
    private static string GetURL(string version, string path)
    {
#if UNITY_IOS
        string os = "iOS";
#else
        string os = "Android";
#endif
        //TODO:環境によって切り替えられるようにする
        return string.Format("https://dev-fish-asset-1.sunchoi.co.jp/{0}/{1}/{2}", os, version, path);
    }

    /// <summary>
    /// 送信
    /// </summary>
    public void Send()
    {
        if (this.status == Status.Success)
        {
            //既にDL成功済みなので、1フレ後に完了処理を呼ぶ
            this.status = Status.Downloading;
            CoroutineUpdator.Create(null, () =>
            {
                this.status = Status.Success;
                this.onCompleted?.Invoke(this);
            });
            return;
        }

        this.status = Status.Downloading;

        TaskUpdator.Run(async () =>
        {
            try
            {
                //タイムアウト設定
                SharkHttpClient.instance.Timeout = TimeSpan.FromSeconds(60);
                //DL開始
                this.bytes = await SharkHttpClient.instance.GetByteArrayAsync(GetURL(this.version, this.hash));
                //DL完了
                this.status = Status.Success;
            }
            catch (Exception e)
            {
                //エラー
                this.status = Status.Error;
                Debug.LogErrorFormat("FileDownloadHandle Error : name={0}, error={1}", this.name, e.Message);
            }

            if (Application.isPlaying)
            {
                //完了を通知
                CoroutineUpdator.Create(null, () => this.onCompleted?.Invoke(this));
            }
        });
    }
}