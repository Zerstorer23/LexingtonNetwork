using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonobehaviourLexCallbacks : MonoBehaviour
{
    private void OnEnable()
    {
        NetworkEventManager.StartListening(LexCallback.PlayerDisconnected, OnPlayerDisconnected);
        NetworkEventManager.StartListening(LexCallback.PlayerJoined, OnPlayerConnected);
        NetworkEventManager.StartListening(LexCallback.OnLocalPlayerJoined, OnLocalPlayerJoined);
        NetworkEventManager.StartListening(LexCallback.MasterClientChanged, OnMasterChanged);
        NetworkEventManager.StartListening(LexCallback.HashChanged, OnHashChanged);
    }

    private void OnHashChanged(NetEventObject arg0)
    {
        //        NetworkEventManager.TriggerEvent(LexCallback.HashChanged, new NetEventObject(LexCallback.HashChanged) { intObj = targetHashID, hashKey = key, hashValue = value });
        int target = arg0.intObj;
        int key = arg0.hashKey;
        string value = arg0.hashValue;
        if (target == 0)
        {
            OnRoomSettingsChanged((RoomProperty)key, value);
        }
        else
        {
            OnPlayerSettingsChanged(LexNetwork.GetPlayerByID(key),(PlayerProperty)key, value);
        }
    }
    public virtual void OnRoomSettingsChanged(RoomProperty key, string value) {

    }
    public virtual void OnPlayerSettingsChanged(NetPlayer player, PlayerProperty key, string value)
    {

    }

    private void OnDisable() {
        NetworkEventManager.StopListening(LexCallback.PlayerDisconnected, OnPlayerDisconnected);
        NetworkEventManager.StopListening(LexCallback.PlayerJoined, OnPlayerConnected);
        NetworkEventManager.StopListening(LexCallback.OnLocalPlayerJoined, OnLocalPlayerJoined);
        NetworkEventManager.StopListening(LexCallback.MasterClientChanged, OnMasterChanged);
        NetworkEventManager.StopListening(LexCallback.MasterClientChanged, OnMasterChanged);
        NetworkEventManager.StopListening(LexCallback.HashChanged, OnHashChanged);
    }

    private void OnMasterChanged(NetEventObject arg0)
    {
        OnMasterChanged(arg0.intObj);
    }
    public virtual void OnMasterChanged(int newMasterActorNr)
    {
    }

    private void OnPlayerConnected(NetEventObject arg0)
    {
        OnPlayerConnected((NetPlayer)arg0.objData);
    }
    public virtual void OnPlayerConnected(NetPlayer newPlayer)
    {
    }
    private void OnPlayerDisconnected(NetEventObject arg0)
    {
        OnPlayerDisconnected(arg0.intObj);
    }
    public virtual void OnPlayerDisconnected(int disconnectedActorNr) { 
        
    }
    private void OnLocalPlayerJoined(NetEventObject arg0)
    {
        OnJoinedRoom();
    }

    public virtual void OnJoinedRoom()
    {

    }
}
