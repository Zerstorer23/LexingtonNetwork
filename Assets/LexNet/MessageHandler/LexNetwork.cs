
#undef USE_LEX


namespace Lex
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using System;
    using static LexNetwork_MessageHandler;
    using System.Linq;
    using Photon.Pun;
    using Photon.Realtime;
    public partial class LexNetwork : MonobehaviourLexCallbacks
    {
#if USE_LEX
        public static bool useLexNet = true;
#else
        public static bool useLexNet = false;
#endif
        public static bool debugLexNet = false;
        public static readonly int MAX_VIEW_IDS = 10000;

        public static string ServerAddress;
        public static bool connected;

        public static LexNetworkConnection networkConnector = new LexNetworkConnection();
        private static LexNetwork prNetwork;
        [SerializeField] [ReadOnly] bool amMaster;
        [SerializeField] [ReadOnly] int myActorID;

        public static LexHashTable CustomProperties { get; private set; } = new LexHashTable();

        public static LexNetwork instance
        {
            get
            {
                if (!prNetwork)
                {
                    prNetwork = FindObjectOfType<LexNetwork>();
                    if (!prNetwork)
                    {
                        Debug.LogWarning("There needs to be one active LexNetwork script on a GameObject in your scene.");
                    }
                    else
                    {
                        prNetwork.Init();
                    }
                }
                return prNetwork;
            }
        }

        public static void DestroyPlayerObjects(string playerID, bool localOnly = false)
        {
            if (!useLexNet)
            {
                PhotonNetwork.DestroyPlayerObjects(GetPlayerByID(playerID).actorID, localOnly);
                return;
            }
            var viewList = LexViewManager.GetViewList();
            foreach (var view in viewList)
            {
                if (view.IsRoomView || view.IsSceneView) continue;
                if (view.Owner.uid == playerID)
                {
                    if (localOnly)
                    {
                        LexViewManager.ReleaseViewID(view);
                        Destroy(view.gameObject);
                    }
                    else
                    {
                        Destroy(view);
                    }
                }
            }

        }
        public static void DestroyAll(bool localOnly = false)
        {
            if (!useLexNet)
            {
                PhotonNetwork.DestroyAll(false);
                return;
            }
            var viewList = LexViewManager.GetViewList();
            foreach (var view in viewList)
            {
                if (view.IsSceneView) continue;
                if (localOnly)
                {
                    LexViewManager.ReleaseViewID(view);
                }
                else
                {
                    Destroy(view);
                }
            }
        }
        bool init = false;
        private void Init()
        {
            if (init) return;
            init = true;
            playerDictionary.Clear();
            Player[] players = PhotonNetwork.PlayerList;
            foreach (Player p in players)
            {
                LexPlayer uPlayer = new LexPlayer(p);
                AddPlayerToDictionary(uPlayer);
            }
            Debug.Log("<color=#00ff00>Conn man : current size</color> " + playerDictionary.Count);
        }


        public static bool ConnectUsingSettings()
        {
            if (!useLexNet)
            {
                Debug.Log("Connect photon");
                return PhotonNetwork.ConnectUsingSettings();
            }

            //1 소켓 연결
            if (IsConnected) return false;
            bool success = networkConnector.Connect();
            if (!success) return false;
            //2 연결 성공시 Request(플레이어 정보, 해시정보 로드
            //  instance.RequestConnectedPlayerInformation();
            //3.해시로드callback받기
            //4. Request Buffered RPC
            Debug.Log("Connection..." + success);
            return success;
        }


        public static bool Reconnect()
        {
            if (!useLexNet)
            {
                return PhotonNetwork.Reconnect();
            }
            Disconnect();
            return ConnectUsingSettings();
        }

        public static void Disconnect()
        {
            if (!useLexNet)
            {
                PhotonNetwork.LeaveRoom();
                return;
            }
            DestroyAll();
            playerDictionary.Clear();
            instance.SetConnected(false);
            networkConnector.Disconnect();
        }



        public static bool AllocateViewID(LexView lv)
        {
            if (!useLexNet)
            {
                return PhotonNetwork.AllocateViewID(lv.Pv);
            }
            return false;

        }
        public static bool AllocateRoomViewID(LexView lv)
        {
            if (!useLexNet)
            {
                return PhotonNetwork.AllocateRoomViewID(lv.Pv);
            }
            return false;
        }

#region instantiation
        public static GameObject Instantiate(string prefabName, Vector3 position, Quaternion quaternion, byte group = 0, object[] parameters = null)
        {

            if (!useLexNet)
            {   //NOTE object not need parse
                return PhotonNetwork.Instantiate(prefabName, position, quaternion, group, parameters);
            }

            GameObject go = NetObjectPool.PollObject(prefabName, position, quaternion);
            LexView lv = go.GetComponent<LexView>();
            lv.SetInstantiateData(parameters);
            lv.SetInformation(LexViewManager.RequestPrivateViewID(), prefabName, LocalPlayer.actorID, LocalPlayer.actorID, false);
            instance.Instantiate_Send(lv.ViewID, LocalPlayer.actorID, prefabName, position, quaternion, parameters);
            return go;
        }


        public static GameObject InstantiateRoomObject(string prefabName, Vector3 position, Quaternion quaternion, byte group = 0, object[] parameters = null)
        {
            if (!useLexNet)
            {   //NOTE object not need parse
                return PhotonNetwork.InstantiateRoomObject(prefabName, position, quaternion, group, parameters);
            }

            GameObject go = NetObjectPool.PollObject(prefabName, position, quaternion);
            LexView lv = go.GetComponent<LexView>();
            lv.SetInstantiateData(parameters);
            lv.SetInformation(LexViewManager.RequestRoomViewID(), prefabName, MasterClient.actorID, LocalPlayer.actorID, true);
            Debug.Log("Instnatiate view id " + lv.ViewID);
            instance.Instantiate_Send(lv.ViewID, LocalPlayer.actorID, prefabName, position, quaternion, parameters);
            return go;
        }
#endregion
        public static int GetPing()
        {
            if (!useLexNet)
            {   //NOTE object not need parse
                return PhotonNetwork.GetPing();
            }
            if (!IsConnected)
            {
                return 0;
            }
            if (NetTime > instance.lastReceivedPing + instance.pingPeriodInSec)
            {
                instance.SendPing();
            }
            double ping = (instance.lastReceivedPing - instance.lastSentPing) * 1000;
            return (int)ping;
        }


        public static bool CloseConnection(LexPlayer player = null, Player pPlayer = null)
        {
            //TODO
            if (!useLexNet)
            {   //NOTE object not need parse
                return PhotonNetwork.CloseConnection(pPlayer);
            }
            //클라이언트에게 접속 해제를 요청 합니다.(KICK). 마스터 클라이언트만 이것을 수행 할 수 있습니다
            return true;
        }

        public static bool SetMasterClient(int masterPlayer)
        {
            //actorID , MessageInfo , callbackType, params
            if (!IsMasterClient) return false;
            LexNetworkMessage netMessage = new LexNetworkMessage();
            netMessage.Add(LocalPlayer.actorID);
            netMessage.Add(MessageInfo.ServerRequest);
            netMessage.Add(LexRequest.ChangeMasterClient);
            netMessage.Add(masterPlayer);
            networkConnector.EnqueueAMessage(netMessage);
            return true;
        }

        public static void Destroy(LexView lv = null, int viewID = -1)
        {
            if (!useLexNet)
            {   //NOTE object not need parse
                PhotonNetwork.Destroy(lv.Pv);
                return;
            }
            if (lv != null)
            {
                viewID = lv.ViewID;
            }
            else if (viewID != -1)
            {
                lv = LexViewManager.GetViewByID(viewID);
                if (lv == null)
                {
                    return;
                }
            }
            else
            {
                Debug.LogWarning("Wrong parameter");
                return;
            }

            if (!lv.IsMine)
            {
                Debug.LogWarning(viewID + " is not mine! ");
                return;
            }
            RemoveBufferedRPCs(lv); //서버 버퍼에서 Instantiate와 모든 RPC제거
            LexViewManager.ReleaseViewID(lv);
            LexNetworkMessage netMessage = new LexNetworkMessage(LocalPlayer.actorID, (int)MessageInfo.Destroy, viewID);
            networkConnector.EnqueueAMessage(netMessage);
        }




        public static void RemoveRPCs(int actorID, Player pPlayer = null)
        {
            if (!useLexNet)
            {   //NOTE object not need parse
                PhotonNetwork.RemoveRPCs(pPlayer);
                return;
            }
            /*
             Remove all buffered RPCs from server that were sent by targetPlayer. Can only be called on local player (for "self") or Master Client (for anyone).

            This method requires either:

            This is the targetPlayer's client.
            This client is the Master Client (can remove any Player's RPCs).
            If the targetPlayer calls RPCs at the same time that this is called, network lag will determine if those get buffered or cleared like the rest.
             */
            //1. 내 송수신버퍼에 actorNumber관련 모든 RPC제거    
            //2. 서버 request 버퍼에 모든 플레이어로부터 rpc제거  <- 이거만 수행
            //3. 서버 callback 수신 rpc 제거
            LexNetworkMessage networkMessage = new LexNetworkMessage();
            networkMessage.Add(LocalPlayer.actorID);
            networkMessage.Add(MessageInfo.ServerRequest);
            networkMessage.Add(LexRequest.RemoveRPC);
            networkMessage.Add(actorID);
            networkMessage.Add("-1");
            networkConnector.EnqueueAMessage(networkMessage);

        }
        public static void RemoveBufferedRPCs(LexView lv)
        {
            if (!useLexNet)
            {   //NOTE object not need parse
                PhotonNetwork.RemoveBufferedRPCs(lv.Pv.ViewID);
                return;
            }
            LexNetworkMessage networkMessage = new LexNetworkMessage();
            networkMessage.Add(LocalPlayer.actorID);
            networkMessage.Add(MessageInfo.ServerRequest);
            networkMessage.Add(LexRequest.RemoveRPC);
            networkMessage.Add("-1");
            networkMessage.Add(lv.ViewID);
            networkConnector.EnqueueAMessage(networkMessage);
        }


        public static void SendChat(string chatMessage)
        {
            chatMessage = chatMessage.Replace(NET_DELIM, " ");
            LexNetworkMessage netMessage = new LexNetworkMessage(LocalPlayer.actorID, (int)MessageInfo.Chat, chatMessage);
            LexChatManager.AddChat(chatMessage);
            networkConnector.EnqueueAMessage(netMessage);
        }


        public static void SetRoomCustomProperties(LexHashTable hash)
        {
            if (!useLexNet)
            {
                PhotonNetwork.CurrentRoom.SetCustomProperties(hash.ToPhotonHash());
                return;
            }
            CustomProperties.UpdateProperties(hash);
            //Needs to be synced with server
            //server needs to keep all hash settings
            instance.CustomProperty_Send(0, hash);
        }

        public static void SetPlayerCustomProperties(ExitGames.Client.Photon.Hashtable hash)
        {
            PhotonNetwork.SetPlayerCustomProperties(hash);
        }
        public static void SetPlayerCustomProperties(LexHashTable hash)
        {
            LocalPlayer.SetCustomProperties(hash);
        }


        [LexRPC]
        public void SetBotProperty(string uid, int key, string typename, object value)
        {
            var player = GetPlayerByID(uid);
            if (player == null) return;
            if (useLexNet)
            {
                object data = ParserAParameter(typename, (string)value);
                player.ReceiveBotProperty(key, value);
            }
            else {
                player.ReceiveBotProperty(key, value);
            }
        }
//TODO Wrap Photon Player on joins

        private void Awake()
        {
            //  dict.Add("Hi", "A");
            // Debug.Log(dict["Hi"]);
        }
        private void Update()
        {
            NetTime += Time.deltaTime;
            networkConnector.DequeueReceivedBuffer();
        }
        private void FixedUpdate()
        {
            amMaster = IsMasterClient;
            if (IsConnected && LocalPlayer != null)
            {

                myActorID = LocalPlayer.actorID;
                players = GetPlayerList();
            }
        }
        public LexPlayer[] players;
        LexRPC
        private void OnApplicationQuit()
        {
            LexNetwork.Disconnect();
        }
    }

}