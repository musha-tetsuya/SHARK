using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ファイルダウンロードダイアログ内容
/// </summary>
public class FileDownloadDialogContent : MonoBehaviour
{
    /// <summary>
    /// テキスト
    /// </summary>
    [SerializeField]
    private Text text = null;

    /// <summary>
    /// 自身のダイアログ
    /// </summary>
    private SimpleDialog dialog = null;
    /// <summary>
    /// 旧リソースリスト
    /// </summary>
    private List<AssetBundleInfo> oldInfoList = null;
    /// <summary>
    /// 新リソースリストのハンドル
    /// </summary>
    private FileDownloadHandle newInfoListHandle = null;
    /// <summary>
    /// ダウンロード対象リスト
    /// </summary>
    private List<AssetBundleInfo> targetInfoList = null;
    /// <summary>
    /// ダウンロードマネージャ
    /// </summary>
    private FileDownloadManager downloadManager = new FileDownloadManager();

    /// <summary>
    /// セットアップ
    /// </summary>
    public void Setup(
        SimpleDialog dialog,
        string resourceVersion,
        List<AssetBundleInfo> oldInfoList,
        FileDownloadHandle newInfoListHandle,
        List<AssetBundleInfo> targetInfoList)
    {
        this.dialog = dialog;
        this.oldInfoList = oldInfoList;
        this.newInfoListHandle = newInfoListHandle;
        this.targetInfoList = targetInfoList;
        this.downloadManager.SetDirectory(AssetManager.GetAssetBundleDirectoryPath());
        this.downloadManager.onCompleted = this.OnDownloaded;

        //進捗率0%表示
        this.text.text = "0%";

        for (int i = 0; i < this.targetInfoList.Count; i++)
        {
            //ダウンロード対象追加
            var handle = new FileDownloadHandle(resourceVersion, this.targetInfoList[i].assetBundleName);
            handle.onCompleted += this.OnDownloadedHandle;
            this.downloadManager.Add(handle);
        }

        //ダイアログが開ききったらダウンロード開始
        this.dialog.onCompleteShow = () => StartCoroutine(this.Download());
    } 

    /// <summary>
    /// ダウンロード処理
    /// </summary>
    private IEnumerator Download()
    {
        //ダウンロード開始
        this.downloadManager.DownloadStart();

        while (true)
        {
            //進捗率表示更新
            var progress = this.downloadManager.GetProgress();
            this.text.text = string.Format("{0}%", (int)(progress * 100));
            yield return new WaitForSeconds(1f);
        }
    }

    /// <summary>
    /// 各ファイルダウンロード完了時
    /// </summary>
    private void OnDownloadedHandle(FileDownloadHandle handle)
    {
        if (handle.status == FileDownloadHandle.Status.Success)
        {
            //古い情報消す
            this.oldInfoList.RemoveAll(x => x.assetBundleName == handle.name);

            //新しい情報保存
            var newInfo = this.targetInfoList.Find(x => x.assetBundleName == handle.name);
            if (newInfo != null)
            {
                this.oldInfoList.Add(newInfo);
            }

            //情報バイナリ保存
            this.downloadManager.SaveFile(this.newInfoListHandle.hash, this.oldInfoList.ToBinary().Cryption());
        }
    }

    /// <summary>
    /// ダウンロード完了時
    /// </summary>
    private void OnDownloaded()
    {
        StopAllCoroutines();

        //進捗率100%表示
        this.text.text = "100%";

        //リソースリスト保存
        this.downloadManager.SaveFile(this.newInfoListHandle);

        //ダイアログ閉じる
        this.dialog.Close();
    }
}
