using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonobehaviourLexCallbacks : MonoBehaviourLex
{
    private void OnEnable()
    {
        NetworkEventManager.StartListening(LexCallback.PlayerDisconnected, OnPlayerDisconnected);
        NetworkEventManager.StartListening(LexCallback.PlayerJoined, OnPlayerConnected);
        NetworkEventManager.StartListening(LexCallback.OnLocalPlayerJoined, OnLocalPlayerJoined);
        NetworkEventManager.StartListening(LexCallback.MasterClientChanged, OnMasterChanged);
        NetworkEventManager.StartListening(LexCallback.HashChanged, OnHashChanged);
        NetworkEventManager.StartListening(LexCallback.Disconnected, OnDisconnected);
    }
    private void OnDisable()
    {
        NetworkEventManager.StopListening(LexCallback.PlayerDisconnected, OnPlayerDisconnected);
        NetworkEventManager.StopListening(LexCallback.PlayerJoined, OnPlayerConnected);
        NetworkEventManager.StopListening(LexCallback.OnLocalPlayerJoined, OnLocalPlayerJoined);
        NetworkEventManager.StopListening(LexCallback.MasterClientChanged, OnMasterChanged);
        NetworkEventManager.StopListening(LexCallback.MasterClientChanged, OnMasterChanged);
        NetworkEventManager.StopListening(LexCallback.HashChanged, OnHashChanged);
    }

    private void OnDisconnected(NetEventObject arg0)
    {
        OnDisconnected();
    }
    public virtual void OnDisconnected() { 
    
    }

    private void OnHashChanged(NetEventObject arg0)
    {
        //        NetworkEventManager.TriggerEvent(LexCallback.HashChanged, new NetEventObject(LexCallback.HashChanged) { intObj = targetHashID, hashKey = key, hashValue = value });
        int target = arg0.intObj;
        LexHashTable hashChanged =(LexHashTable) arg0.objData;
        if (target == 0)
        {
            OnRoomSettingsChanged(hashChanged);
        }
        else
        {
            OnPlayerSettingsChanged(LexNetwork.GetPlayerByID(target), hashChanged);
        }
    }
    public virtual void OnRoomSettingsChanged(LexHashTable hashChanged)
    {

    }
    public virtual void OnPlayerSettingsChanged(LexPlayer player, LexHashTable hashChanged)
    {

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
        OnPlayerEnteredRoom((LexPlayer)arg0.objData);
    }
    public virtual void OnPlayerEnteredRoom(LexPlayer newPlayer)
    {
    }
    private void OnPlayerDisconnected(NetEventObject arg0)
    {
        OnPlayerLeftRoom((LexPlayer)arg0.objData);
    }
    public virtual void OnPlayerLeftRoom(LexPlayer newPlayer) { 
        
    }
    private void OnLocalPlayerJoined(NetEventObject arg0)
    {
        OnJoinedRoom();
    }

    public virtual void OnJoinedRoom()
    {

    }
}
