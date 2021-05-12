using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonobehaviourLexCallbacks : MonoBehaviour
{
    private void OnEnable()
    {
        NetworkEventManager.StartListening(LexCallback.LocalPlayerJoined, OnJoinedRoom);
        NetworkEventManager.StartListening(LexCallback.PlayerDisconnected, OnPlayerDisconnected);
        NetworkEventManager.StartListening(LexCallback.PlayerJoined, OnPlayerConnected);
        NetworkEventManager.StartListening(LexCallback.Receive_RoomHash, OnRoomHashPulled);
        NetworkEventManager.StartListening(LexCallback.MasterClientChanged, OnMasterChanged);
    }
    private void OnDisable() {
        NetworkEventManager.StopListening(LexCallback.LocalPlayerJoined, OnJoinedRoom);
        NetworkEventManager.StopListening(LexCallback.PlayerDisconnected, OnPlayerDisconnected);
        NetworkEventManager.StopListening(LexCallback.PlayerJoined, OnPlayerConnected);
        NetworkEventManager.StopListening(LexCallback.Receive_RoomHash, OnRoomHashPulled);
        NetworkEventManager.StopListening(LexCallback.MasterClientChanged, OnMasterChanged);
    }

    private void OnMasterChanged(NetEventObject arg0)
    {
        OnMasterChanged(arg0.intObj);
    }
    public virtual void OnMasterChanged(int newMasterActorNr)
    {
    }
    private void OnRoomHashPulled(NetEventObject arg0)
    {
        OnRoomHashPulled(arg0.stringObj);
    }
    public virtual void OnRoomHashPulled(string changedKey)
    {

    }
    private void OnPlayerConnected(NetEventObject arg0)
    {
        OnPlayerConnected(arg0.intObj);
    }
    public virtual void OnPlayerConnected(int connectedPlayerNr)
    {
    }
    private void OnPlayerDisconnected(NetEventObject arg0)
    {
        OnPlayerDisconnected(arg0.intObj);
    }
    public virtual void OnPlayerDisconnected(int disconnectedActorNr) { 
        
    }

    private void OnJoinedRoom(NetEventObject arg0)
    {
        OnJoinedRoom();
    }
    public virtual void OnJoinedRoom()
    {

    }
}
