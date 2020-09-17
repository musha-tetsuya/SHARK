using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public abstract class MultiNetworkEventReceiverState : StateBase, MultiNetworkEventObserver.IReceiver
{
    public abstract MultiNetworkEventObserver observer { get; }

    public override void Start()
    {
        this.observer.receiver = this;
    }

    public override void End()
    {
        this.observer.receiver = null;
    }

    public virtual void OnDisconnected(DisconnectCause cause){}
    public virtual void OnConnected(){}
    public virtual void OnLobbyStatisticsUpdate(List<TypedLobbyInfo> lobbyStatistics){}
    public virtual void OnJoinRandomFailed(short returnCode, string message){}
    public virtual void OnCreateRoomFailed(short returnCode, string message){}
    public virtual void OnJoinedRoom(){}
    public virtual void OnPlayerEnteredRoom(Player newPlayer){}
    public virtual void OnPlayerLeftRoom(Player otherPlayer){}
    public virtual void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps){}
    public virtual void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged){}
    public virtual void OnEvent(EventData photonEvent){}
    public virtual void OnConnectedToMaster(){}
}
