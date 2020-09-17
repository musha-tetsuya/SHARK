using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssetLoadErrorTest : MonoBehaviour
{
    /// <summary>
    /// エラーしたハンドル
    /// </summary>
    private AssetLoadHandle errorHandle = null;

    /// <summary>
    /// Start
    /// </summary>
    private void Start()
    {
        //アセットロードエラー時コールバックを登録
        AssetManager.onError = this.OnAssetLoadError;

        //適当に存在しないリソースをロードしてみる
        AssetManager.Load<GameObject>("hoge", (asset) =>
        {
            Debug.Log("エラーするのでここには来ない");
        });

        //こっちは存在するリソースをロード
        AssetManager.Load<GameObject>("test", (asset) =>
        {
            Debug.Log("リトライするとロードは完了するが、↑でエラーするのでここも来ない");
        });
    }

    /// <summary>
    /// アセットロードエラー時
    /// </summary>
    private void OnAssetLoadError(AssetLoadHandle errorHandle)
    {
        this.errorHandle = errorHandle;
    }

    /// <summary>
    /// OnGUI
    /// </summary>
    private void OnGUI()
    {
        if (this.errorHandle != null)
        {
            //エラーしたハンドルのパス
            GUILayout.Label(this.errorHandle.path);
            //エラー内容
            GUILayout.Label(this.errorHandle.errorStatus.ToString());

            //エラー解消出来たならリトライさせることも出来る（エラーの種類にもよる）
            if (GUILayout.Button("Retry"))
            {
                this.errorHandle = null;
                AssetManager.Retry();
            }

            //エラー解消出来なかったり、何度リトライしてもダメだったらタイトルに戻らせる
            if (GUILayout.Button("BackToTitle"))
            {
                this.errorHandle = null;
                AssetManager.UnloadAll(true);
                AssetManager.Clear();
            }
        }
    }
}
