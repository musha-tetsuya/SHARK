using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

/// <summary>
/// SharedScene
/// </summary>
public class SharedScene : MonoBehaviour
{
#if UNITY_EDITOR
    /// <summary>
    /// SharedScene経由後に開くシーン名のEditorPrefsキー
    /// </summary>
    private const string NEXT_SCENENAME_KEY = "SharedScene.NextSceneName";

    /// <summary>
    /// 再生時開始シーンをセットする
    /// </summary>
    /// <para>
    /// エディタ起動時やスクリプトコンパイル時に呼ばれる
    /// </para>
    [InitializeOnLoadMethod]
    private static void SetPlayModeStartScene()
    {
        //エディタ上でシーン開いたときのイベントをセット
        EditorSceneManager.activeSceneChangedInEditMode += (closedScene, openedScene) =>
        {
            EditorSceneManager.playModeStartScene = null;

            //指定ディレクトリ外のシーンの場合は再生時にSharedSceneを経由したくないのでスルーする
            if (!openedScene.path.Contains("Assets/Sunchoi/Scenes/Game")) return;

            //再生時開始シーンをSharedSceneに設定
            EditorSceneManager.playModeStartScene = AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/Sunchoi/Scenes/Game/Shared.unity");

            //SharedScene経由後に開くシーンとして、今開いたシーンの名前を保存
            EditorPrefs.SetString(NEXT_SCENENAME_KEY, openedScene.name);

            Debug.LogFormat("SetPlayModeStartScene: {0}", openedScene.name);
        };
    }

    /// <summary>
    /// Editor時、Titleを経由しない場合のユーザーデータ
    /// </summary>
    [SerializeField]
    private OfflineResponseData offlineFirstUserResponceData = null;
#endif

    /// <summary>
    /// Start
    /// </summary>
    private void Start()
    {
        //SharedSceneは破棄されないようにする
        DontDestroyOnLoad(this.gameObject);

#if SHARK_PRODUCTION
        //製品版はログ出ないように
        Debug.unityLogger.logEnabled = false;
#endif

#if UNITY_EDITOR
        if (EditorPrefs.HasKey(NEXT_SCENENAME_KEY))
        {
            //SharedSceneに来る前のシーンを開く（前のシーンもSharedSceneだったらスルー）
            string nextSceneName = EditorPrefs.GetString(NEXT_SCENENAME_KEY);
            if (nextSceneName != EditorSceneManager.GetActiveScene().name)
            {
                //遷移先がタイトル以外の場合、オフラインユーザーを作っておく
                if (nextSceneName != "Title")
                {
                    var userData = new UserData();
                    var firstUserResponseData = JsonConvert.DeserializeObject<FirstApi.FirstUserResponseData>(this.offlineFirstUserResponceData.data);
                    userData.Set(firstUserResponseData);
                    UserData.Set(userData);
                }

                SceneChanger.ChangeSceneAsync(nextSceneName);
                return;
            }
        }
#endif

        //SharedSceneの次のシーンはTitle？に遷移
        SceneChanger.ChangeSceneAsync("Title");
    }
}
