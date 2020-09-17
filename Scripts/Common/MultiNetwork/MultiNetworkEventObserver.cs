using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

/// <summary>
/// マルチネットワークイベント監視
/// </summary>
public class MultiNetworkEventObserver : MonoBehaviourPunCallbacks, IOnEventCallback
{
    /// <summary>
    /// インターフェース
    /// </summary>
    public interface IReceiver
    {
        void OnDisconnected(DisconnectCause cause);
        void OnConnected();
        void OnLobbyStatisticsUpdate(List<TypedLobbyInfo> lobbyStatistics);
        void OnJoinRandomFailed(short returnCode, string message);
        void OnCreateRoomFailed(short returnCode, string message);
        void OnJoinedRoom();
        void OnPlayerEnteredRoom(Player newPlayer);
        void OnPlayerLeftRoom(Player otherPlayer);
        void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps);
        void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged);
        void OnEvent(EventData photonEvent);
        void OnConnectedToMaster();
    }

    /// <summary>
    /// レシーバー
    /// </summary>
    public IReceiver receiver = null;

    /// <summary>
    /// 接続切断時
    /// </summary>
    public override void OnDisconnected(DisconnectCause cause)
    {
        this.receiver?.OnDisconnected(cause);
    }

    /// <summary>
    /// 接続成功時
    /// </summary>
    public override void OnConnected()
    {
        this.receiver?.OnConnected();
    }

    /// <summary>
    /// マスターサーバーへの接続成功時
    /// </summary>
    public override void OnConnectedToMaster()
    {
        this.receiver?.OnConnectedToMaster();
    }

    /// <summary>
    /// ロビー情報更新時
    /// </summary>
    public override void OnLobbyStatisticsUpdate(List<TypedLobbyInfo> lobbyStatistics)
    {
        this.receiver?.OnLobbyStatisticsUpdate(lobbyStatistics);
    }

    /// <summary>
    /// ランダムルームへの参加失敗時
    /// </summary>
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        this.receiver?.OnJoinRandomFailed(returnCode, message);
    }

    /// <summary>
    /// ルーム作成失敗時
    /// </summary>
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        this.receiver?.OnCreateRoomFailed(returnCode, message);
    }

    /// <summary>
    /// ルーム参加時
    /// </summary>
    public override void OnJoinedRoom()
    {
        this.receiver?.OnJoinedRoom();
    }

    /// <summary>
    /// 新規プレイヤー入室時
    /// </summary>
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        this.receiver?.OnPlayerEnteredRoom(newPlayer);
    }

    /// <summary>
    /// プレイヤー退出時
    /// </summary>
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        this.receiver?.OnPlayerLeftRoom(otherPlayer);
    }

    /// <summary>
    /// プレイヤー情報更新時
    /// </summary>
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        this.receiver?.OnPlayerPropertiesUpdate(targetPlayer, changedProps);
    }

    /// <summary>
    /// ルーム情報更新時
    /// </summary>
    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        this.receiver?.OnRoomPropertiesUpdate(propertiesThatChanged);
    }

    /// <summary>
    /// イベント受信時
    /// </summary>
    public void OnEvent(EventData photonEvent)
    {
        this.receiver?.OnEvent(photonEvent);
    }
}
