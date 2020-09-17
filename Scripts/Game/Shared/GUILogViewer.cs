using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Logビューワー
/// </summary>
/// <para>
/// 実機でログをGUIで見るやつ
/// </para>
public class GUILogViewer : MonoBehaviour
{
    /// <summary>
    /// Logデータ
    /// </summary>
    private class LogData
    {
        /// <summary>
        /// Logタイプ
        /// </summary>
        public LogType type = LogType.Log;
        /// <summary>
        /// メッセージ
        /// </summary>
        public string message = null;
        /// <summary>
        /// 詳細
        /// </summary>
        public string detail = null;
    }

    /// <summary>
    /// ステートタイプ
    /// </summary>
    private enum StateType
    {
        None,
        ShowLog,
        Length
    }

    /// <summary>
    /// 現在のステートタイプ
    /// </summary>
    private StateType stateType = StateType.None;
    /// <summary>
    /// 最大ログ保持数
    /// </summary>
    private int maxLogSize = 20;
    /// <summary>
    /// ログ保持先
    /// </summary>
    private List<LogData> logList = new List<LogData>();
    /// <summary>
    /// 選択中Logデータ
    /// </summary>
    private LogData selectedLogData = null;
    /// <summary>
    /// スクロール位置
    /// </summary>
    private Vector2 scrollPosition = Vector2.zero;
    /// <summary>
    /// Logボタンのスタイル
    /// </summary>
    private GUIStyle logButtonStyle = null;
    /// <summary>
    /// GUIボタンのスタイル
    /// </summary>
    private GUIStyle[] buttonStyles = null;
    /// <summary>
    /// GUIテキストエリアのスタイル
    /// </summary>
    private GUIStyle[] textAreaStyles = null;
    /// <summary>
    /// ステート別処理
    /// </summary>
    private Action[] guiStateAction = new Action[(int)StateType.Length];

    /// <summary>
    /// Awake
    /// </summary>
    private void Awake()
    {
        //ログ受信時イベント登録
        Application.logMessageReceived += this.OnLogMessageReceived;

        //ステート別処理登録
        this.guiStateAction[(int)StateType.None] = null;
        this.guiStateAction[(int)StateType.ShowLog] = this.ShowLogState;
    }

    /// <summary>
    /// ログ受信時
    /// </summary>
    private void OnLogMessageReceived(string condition, string stackTrace, LogType type)
    {
        var logData = new LogData();
        logData.type = type;
        logData.message = string.Format("[{0}] {1}\n{2}", DateTime.Now.ToString("HH:mm:ss"), condition, type);
        logData.detail = string.Format("{0}\n{1}", condition, stackTrace);

        //保持リストに追加
        this.logList.Add(logData);

        //最大保持数超えたら、一番最初のやつを消す
        if (this.logList.Count > this.maxLogSize)
        {
            this.logList.RemoveAt(0);
        }

        //エラー系の場合自動でログ画面を開く
        if (type == LogType.Error
        ||  type == LogType.Assert
        ||  type == LogType.Exception)
        {
            if (this.stateType == StateType.None)
            {
                this.ShowLog();
            }
        }
    }

    /// <summary>
    /// GUIStyle作成
    /// </summary>
    private GUIStyle CreateGUIStyle(GUIStyle baseStyle, LogType type)
    {
        Color[] textColors =
        {
            Color.red,
            Color.red,
            Color.yellow,
            Color.white,
            Color.red,
        };

        var guiStyle = new GUIStyle(baseStyle);
        guiStyle.normal.textColor = textColors[(int)type];
        guiStyle.hover.textColor = textColors[(int)type];
        return guiStyle;
    }

    /// <summary>
    /// GUI描画
    /// </summary>
    public void DrawGUI()
    {
        if (this.logButtonStyle == null)
        {
            this.logButtonStyle = UIUtility.CreateButtonStyle(150f, 40);
        }

        if (this.buttonStyles == null)
        {
            this.buttonStyles = new GUIStyle[5];
            for (int i = 0; i < 5; i++)
            {
                this.buttonStyles[i] = this.CreateGUIStyle(GUI.skin.button, (LogType)i);
                this.buttonStyles[i].alignment = TextAnchor.UpperLeft;
            }
        }

        if (this.textAreaStyles == null)
        {
            this.textAreaStyles = new GUIStyle[5];
            for (int i = 0; i < 5; i++)
            {
                this.textAreaStyles[i] = this.CreateGUIStyle(GUI.skin.textArea, (LogType)i);
            }
        }

        if (GUILayout.Button("log", this.logButtonStyle))
        {
            if (this.stateType == StateType.None)
            {
                this.ShowLog();
            }
            else
            {
                this.CloseLog();
            }
        }

        this.guiStateAction[(int)this.stateType]?.Invoke();
    }

    /// <summary>
    /// ログ表示
    /// </summary>
    private void ShowLog()
    {
        this.scrollPosition = Vector2.zero;
        this.stateType = StateType.ShowLog;
        this.selectedLogData = this.logList.LastOrDefault();
    }

    /// <summary>
    /// ログ閉じる
    /// </summary>
    private void CloseLog()
    {
        this.scrollPosition = Vector2.zero;
        this.stateType = StateType.None;
        this.selectedLogData = null;
    }

    /// <summary>
    /// ログ表示ステート
    /// </summary>
    private void ShowLogState()
    {
        this.scrollPosition = GUILayout.BeginScrollView(this.scrollPosition);

        for (int i = this.logList.Count - 1; i >= 0; i--)
        {
            int index = (int)this.logList[i].type;
            var style = this.buttonStyles[index];
            if (GUILayout.Button(this.logList[i].message, style))
            {
                this.selectedLogData = this.logList[i];
            }
        }

        GUILayout.EndScrollView();

        if (this.selectedLogData != null)
        {
            int index = (int)selectedLogData.type;
            var style = this.textAreaStyles[index];
            GUILayout.TextArea(this.selectedLogData.detail, style);
        }
    }
}
