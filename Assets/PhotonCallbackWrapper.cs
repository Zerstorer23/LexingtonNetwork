
namespace Lex
{
    using Photon.Pun;
    using Photon.Realtime;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class PhotonCallbackWrapper : MonoBehaviourPunCallbacks
    {
#if !USE_LEX
        public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
        {
            NetworkEventManager.TriggerEvent(LexCallback.HashChanged, new NetEventObject() { stringObj = "0", objData = new LexHashTable(propertiesThatChanged) });
        }
        public override void OnDisconnected(DisconnectCause cause)
        {
            NetworkEventManager.TriggerEvent(LexCallback.Disconnected, null);
        }
        public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
        {
            var pp = LexNetwork.GetPlayerByID(targetPlayer.UserId);
            LexHashTable hashChanged = new LexHashTable(changedProps);
            NetworkEventManager.TriggerEvent(LexCallback.HashChanged, new NetEventObject() { stringObj = pp.actorID.ToString(), objData = hashChanged });
        }
        public override void OnMasterClientSwitched(Player newMasterClient)
        {
            NetworkEventManager.TriggerEvent(LexCallback.MasterClientChanged, new NetEventObject() {intObj = LexNetwork.GetPlayerByID(newMasterClient.UserId).actorID });
        }
        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            if (!LexNetwork.ContainsPlayer(newPlayer.UserId))
            {
                LexPlayer player = new LexPlayer(newPlayer);// 
                LexNetwork.AddPlayerToDictionary(player);
                NetworkEventManager.TriggerEvent(LexCallback.PlayerJoined, new NetEventObject() { objData = player });
            }
        }
        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            LexPlayer player = LexNetwork.GetPlayerByID(otherPlayer.UserId);
            if (player != null)
            {
                LexNetwork.RemovePlayerFromDictionary(player.uid);
                NetworkEventManager.TriggerEvent(LexCallback.PlayerDisconnected, new NetEventObject() { objData = player });
            }
        }
        public override void OnJoinedRoom()
        {
            Player[] players = PhotonNetwork.PlayerList;
            LexNetwork.playerDictionary.Clear();
            foreach (Player p in players)
            {
                LexPlayer uPlayer = new LexPlayer(p);
                Debug.LogWarning(p.IsLocal +" "+ p);
                LexNetwork.AddPlayerToDictionary(uPlayer);
            }
            Debug.Log("<color=#00ff00>Conn man : current size</color> " + LexNetwork.playerDictionary.Count);
            NetworkEventManager.TriggerEvent(LexCallback.OnLocalPlayerJoined, null);
        }

        public static ExitGames.Client.Photon.Hashtable GetInitOptions()
        {
            var hash = new ExitGames.Client.Photon.Hashtable();
            hash.Add(RoomProperty.GameMode, "dd");
            hash.Add(RoomProperty.Seed, Random.Range(0, 133));
            return hash;
        }
        public static void JoinRoom()
        {

            var hash = GetInitOptions();
            RoomOptions roomOpts = new RoomOptions()
            {
                IsVisible = true,
                IsOpen = true,
                MaxPlayers = (byte)10,
                PublishUserId = true,
                CustomRoomProperties = hash
            };
            PhotonNetwork.JoinOrCreateRoom("Primary", roomOpts, TypedLobby.Default);

        }
        public override void OnConnectedToMaster()
        {
            Debug.Log("Connected to master");
            JoinRoom();
        }
#endif
    }
}