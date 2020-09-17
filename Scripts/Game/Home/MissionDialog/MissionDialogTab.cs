using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ミッションダイアログタブ
/// </summary>
public class MissionDialogTab : TabBase
{
    /// <summary>
    /// テキストアウトライン
    /// </summary>
    [SerializeField]
    private Shadow textShadow = null;
    /// <summary>
    /// アクティブ時テキストアウトライン色
    /// </summary>
    [SerializeField]
    private Color activeTextOutlineColor = Color.white;
    /// <summary>
    /// 非アクティブ時テキストアウトライン色
    /// </summary>
    [SerializeField]
    private Color inactiveTextOutlineColor = Color.white;
    /// <summary>
    /// カテゴリ
    /// </summary>
    [SerializeField]
    public MissionApi.Category category = MissionApi.Category.None;

    /// <summary>
    /// サーバーデータ
    /// </summary>
    public MissionApi.MissionProgressGroup server { get; private set; }
    /// <summary>
    /// ミッション一覧
    /// </summary>
    public MissionApi.MissionProgress[] missionList { get; private set; }

    /// <summary>
    /// アクティブ切り替え
    /// </summary>
    protected override void OnChangeActive()
    {
        base.OnChangeActive();

        //テキストアウトライン色の変化
        if (this.isActive)
        {
            this.textShadow.effectColor = this.activeTextOutlineColor;
        }
        else
        {
            this.textShadow.effectColor = this.inactiveTextOutlineColor;
        }
    }

    /// <summary>
    /// ミッションリストセット
    /// </summary>
    public void Set(MissionApi.MissionProgressGroup server)
    {
        this.server = server;

        if (this.server != null)
        {
            this.missionList = this.server.clearNotReceived
                .Concat(this.server.notClear)
                .Concat(this.server.clearReceived)
                .ToArray();
        }
        else
        {
            this.missionList = new MissionApi.MissionProgress[0];
        }
    }
}
