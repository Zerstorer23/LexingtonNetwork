namespace Lex
{
    using Photon.Pun;
    using Photon.Realtime;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class MonobehaviourLexCallbacks :
#if USE_LEX
        MonoBehaviourLex
#else
        MonoBehaviourPunCallbacks
#endif
    {
#if USE_LEX
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
         private void OnHashChanged(NetEventObject arg0)
        {
            //        NetworkEventManager.TriggerEvent(LexCallback.HashChanged, new NetEventObject(LexCallback.HashChanged) { intObj = targetHashID, hashKey = key, hashValue = value });
            string target = arg0.stringObj;
            LexHashTable hashChanged = (LexHashTable)arg0.objData;
            if (target == "0")
            {
                OnRoomSettingsChanged(hashChanged);
            }
            else
            {
                OnPlayerSettingsChanged(LexNetwork.GetPlayerByID(target), hashChanged);
            }
        }
           private void OnMasterChanged(NetEventObject arg0)
        {
            OnMasterChanged(arg0.intObj);
        }
        private void OnPlayerConnected(NetEventObject arg0)
        {
            OnPlayerEnteredRoom((LexPlayer)arg0.objData);
        }
        private void OnPlayerDisconnected(NetEventObject arg0)
        {
            OnPlayerLeftRoom((LexPlayer)arg0.objData);
        }
        private void OnLocalPlayerJoined(NetEventObject arg0)
        {
            OnJoinedRoom();
        }

#else
        public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
        {
            OnRoomSettingsChanged(new LexHashTable(propertiesThatChanged));
        }
        public override void OnDisconnected(DisconnectCause cause)
        {
            OnDisconnected();
        }
        public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
        {
            LexPlayer player = LexNetwork.GetPlayerByID(targetPlayer.UserId);
            OnPlayerSettingsChanged(player, new LexHashTable(changedProps));
        }
        public override void OnMasterClientSwitched(Player newMasterClient)
        {
            LexPlayer player = LexNetwork.GetPlayerByID(newMasterClient.UserId);
            OnMasterChanged(player.actorID);
        }
        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            LexPlayer player = new LexPlayer(newPlayer);// LexNetwork.GetPlayerByID(newPlayer.UserId);
            LexNetwork.instance.AddPlayerToDictionary(player);
            OnPlayerEnteredRoom(player);
        }
        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            LexPlayer player = LexNetwork.GetPlayerByID(otherPlayer.UserId);
            LexNetwork.instance.RemovePlayerFromDictionary(player.uid);
            OnPlayerLeftRoom(player);
        }
        public override void OnJoinedRoom()
        {
            OnLocalPlayerJoined();
        }
#endif

        public virtual void OnDisconnected()
        {

        }

       
        public virtual void OnRoomSettingsChanged(LexHashTable hashChanged)
        {

        }
        public virtual void OnPlayerSettingsChanged(LexPlayer player, LexHashTable hashChanged)
        {

        }

        public virtual void OnMasterChanged(int newMasterActorNr)
        {
        }
     
        public virtual void OnPlayerEnteredRoom(LexPlayer newPlayer)
        {
        }
       
        public virtual void OnPlayerLeftRoom(LexPlayer newPlayer)
        {

        }
        public virtual void OnLocalPlayerJoined()
        {

        }


    }

}