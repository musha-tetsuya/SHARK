using System;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

public static class SceneChanger
{
    /// <summary>
    /// 現在のシーン
    /// </summary>
    private static string currentSceneName = null;
    /// <summary>
    /// 現在のシーン
    /// </summary>
    public static SceneBase currentScene { get; private set; }
    /// <summary>
    /// ロード中かどうか
    /// </summary>
    private static bool IsLoading = false;
    /// <summary>
    /// 自動でロード中表示を消すかどうか
    /// </summary>
    public static bool IsAutoHideLoading = true;

    /// <summary>
    /// シーン切り替え
    /// </summary>
    public static void ChangeSceneAsync(string nextSceneName, SceneDataPackBase dataPack = null)
    {
        if (currentSceneName == nextSceneName) return;
        if (IsLoading) return;

        //ロード中フラグON
        IsLoading = true;

        //デフォルトは自動でロード中表示を消す
        IsAutoHideLoading = true;

        if (dataPack == null)
        {
            dataPack = new SceneDataPackBase();
        }
        dataPack.toSceneName = nextSceneName;

        if (string.IsNullOrEmpty(currentSceneName))
        {
            //場面転換アニメなし
            LoadSceneAsync(dataPack);
        }
        else
        {
            dataPack.fromSceneName = currentSceneName;

            //場面転換アニメあり
            SharedUI.Instance.ShowSceneChangeAnimation(() =>
            {
                //空シーンに遷移することで現在のシーンをアンロード
                SceneManager.LoadSceneAsync("Empty").completed += (op1) =>
                {
#if DEBUG && UNITY_EDITOR
                    //シーン消えたけどUnloadされてないアセット一覧をログ表示
                    foreach (var handle in AssetManager.handles)
                    {
                        Debug.LogFormat("未アンロード：{0}, referenceCount={1}", handle.path, handle.referenceCount);
                    }
#endif
                    //リソースアンロード
                    Resources.UnloadUnusedAssets().completed += (op2) =>
                    {
                        //GC整理
                        GC.Collect();
                        //次のシーンをロード
                        LoadSceneAsync(dataPack);
                    };
                };
            });
        }
    }

    /// <summary>
    /// シーンロード
    /// </summary>
    private static void LoadSceneAsync(SceneDataPackBase dataPack)
    {
        //Header非表示 ※次のシーンでも表示が続くなら非表示にしない方が良いか？
        SharedUI.Instance.HideHeader();

        //ロード開始
        SceneManager.LoadSceneAsync(dataPack.toSceneName).completed += (op) =>
        {
            //シーン取得
            currentSceneName = dataPack.toSceneName;
            currentScene = SceneManager
                .GetSceneByName(currentSceneName)
                .GetRootGameObjects()
                .Select(g => g.GetComponent<SceneBase>())
                .First(s => s != null);

            if (IsAutoHideLoading)
            {
                //シーン移動アニメーション終了
                SharedUI.Instance.HideSceneChangeAnimation();
            }

            //ロード完了通知
            IsLoading = false;
            currentScene.OnSceneLoaded(dataPack);
        };
    }
}
