using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// プレゼントBOXスクロールビュー要素
/// </summary>
public class PresentBoxItem : MonoBehaviour
{
    /// <summary>
    /// CommonIcon
    /// </summary>
    [SerializeField]
    private CommonIcon icon = null;
    /// <summary>
    /// 名前テキスト
    /// </summary>
    [SerializeField]
    private Text nameText = null;
    /// <summary>
    /// メッセージテキスト
    /// </summary>
    [SerializeField]
    private Text messageText = null;
    /// <summary>
    /// 有期限時にONにするオブジェクト
    /// </summary>
    [SerializeField]
    private GameObject limitedDateContent = null;
    /// <summary>
    /// 受け取り期限テキスト
    /// </summary>
    [SerializeField]
    private Text limitedDateText = null;
    /// <summary>
    /// 生成日付テキスト
    /// </summary>
    [SerializeField]
    private Text createDateText = null;
    /// <summary>
    /// 受け取りボタン
    /// </summary>
    [SerializeField]
    private Button receiveButton = null;

    /// <summary>
    /// サーバーデータ
    /// </summary>
    public TPresentBox server { get; private set; }
    /// <summary>
    /// 受け取りボタン押下時コールバック
    /// </summary>
    private Action<PresentBoxItem> onClickReceiveButton = null;

    /// <summary>
    /// 表示構築
    /// </summary>
    public void BuildView(TPresentBox server, Action<PresentBoxItem> onClickReceiveButton)
    {
        this.server = server;
        this.onClickReceiveButton = onClickReceiveButton;
        var limitedData = this.server as TPresentBoxLimited;
        var itemInfo = CommonIconUtility.GetItemInfo(this.server.itemType, this.server.itemId);

        //CommonIcon表示構築
        this.icon.Set(itemInfo, false);

        //アイテム数表示
        if (this.server.itemNum > 1)
        {
            this.icon.SetCountText(this.server.itemNum);
        }
        else
        {
            this.icon.countText.text = null;
        }

        //名前テキスト設定
        this.nameText.text = itemInfo.GetName();

        //メッセージテキスト設定
        this.messageText.text = Masters.MessageDB.FindById(this.server.messageId).messageText;

        //有期限プレゼントなら
        if (limitedData != null && limitedData.limitDate.HasValue)
        {
            //有期限オブジェクト表示ON
            this.limitedDateContent.SetActive(true);

            //残り時間表示
            var span = limitedData.limitDate.Value - DateTime.Now;
            this.limitedDateText.text = (span.Days > 0)    ? Masters.LocalizeTextDB.GetFormat("ReceiveLimitedToDay", span.Days)
                                      : (span.Hours > 0)   ? Masters.LocalizeTextDB.GetFormat("ReceiveLimitedToHour", span.Hours)
                                      : (span.Minutes > 0) ? Masters.LocalizeTextDB.GetFormat("ReceiveLimitedToMinites", span.Minutes)
                                      :                      Masters.LocalizeTextDB.GetFormat("ReceiveLimitedToMinites", 0);
        }
        //無期限プレゼントなら
        else
        {
            this.limitedDateContent.SetActive(false);
        }

        if (this.server.created_at == null)
        {
            //生成日付がnullだったらエラー回避のため今の時間を入れておく
            this.server.created_at = DateTime.Now;
        }

        //生成日付表示
        this.createDateText.text = this.server.created_at.Value.ToString();

        //受け取り済みプレゼントじゃなければ受け取りボタンを表示
        this.receiveButton.gameObject.SetActive(!(this.server is TPresentBoxReceived) && this.onClickReceiveButton != null);
    }

    /// <summary>
    /// 受け取りボタン押下時
    /// </summary>
    public void OnClickReceiveButton()
    {
        this.onClickReceiveButton?.Invoke(this);
    }
}