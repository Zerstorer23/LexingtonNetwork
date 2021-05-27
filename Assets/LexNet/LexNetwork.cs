using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System;
using static LexNetworkConnection;
using System.Linq;

public class LexNetwork : MonobehaviourLexCallbacks
{
    private static Dictionary<int, LexView> viewDictionary = new Dictionary<int, LexView>();
    private static Dictionary<int, NetPlayer> playerDictionary  = new Dictionary<int, NetPlayer>();
    private static Mutex mutex = new Mutex();


    static readonly int MAX_VIEW_IDS = 1000;

    public static string ServerAddress;
    public static bool connected;
    public static NetPlayer LocalPlayer { get; private set; }
    public static NetPlayer MasterClient { get; private set; }
    public static string NickName { get; private set; }
//    public static NetPlayer[] playerList;
    public static double NetTime { get; private set; }
    public static bool IsConnected { get; private set; }
    public static bool IsMasterClient { get; private set; }
    public static int countOfPlayersInRoom;

    static LexNetworkConnection networkConnector = new LexNetworkConnection();
    public static LexNetworkWorker networkWorker;
    private static LexNetwork prNetwork;

    public static LexNetwork_HashSettings CustomProperty { get; private set; } = new LexNetwork_HashSettings();

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

    internal void SetMasterClient_Receive(int sentActorNumber, int nextMaster)
    {
        //지금마스터 해제
        //새 마스터 등록
        //view아이디 owner정보 변경
        playerDictionary[sentActorNumber].IsMasterClient = false;
        playerDictionary[nextMaster].IsMasterClient = true;
        foreach (var entry in viewDictionary) {
            entry.Value.UpdateOwnership();
        }
    }

    internal static void DestoryAll(int playerID)
    {
        var viewList = viewDictionary.Values.ToList();
        foreach (var view in viewList) {
            if (view.IsRoomView || view.IsSceneView) continue;
            if (view.creatorActorNr == playerID) {
                viewDictionary.Remove(view.ViewID);
            }        
        }
    }

    private void Init()
    {
        networkWorker = new LexNetworkWorker(this);
    }

    internal void SetServerTime(bool isModification, long timeValue)
    {
        if (isModification)
        {
            NetTime+= (double)timeValue / 1000;
        }
        else {
            NetTime = (double)timeValue / 1000; //long is in mills
        }
        Debug.Log("Modified time : " + NetTime);
    }

    public static bool ConnectUsingSettings() {
        //TODO
        //1 소켓 연결
        if (IsConnected) return false;
        bool success = networkConnector.Connect();
        if (!success) return false;
        //2 연결 성공시 Request(플레이어 정보, 해시정보 로드
      //  instance.RequestConnectedPlayerInformation();
        //3.해시로드callback받기
        //4. Request Buffered RPC
        Debug.Log("Connection..."+success);
        return success;
    }

   /* private void RequestConnectedPlayerInformation()
    {
        LexNetworkMessage netMessage = new LexNetworkMessage((int)MessageInfo.ServerRequest, (int)LexRequest.Receive_Initialise);
        networkConnector.EnqueueAMessage(netMessage);
    }*/
    public static bool Reconnect()
    {
        //TODO
        return true;
    }

    public static void Disconnect() {
        networkConnector.Disconnect();
    }

  

    public static int AllocateViewID()
    {
        //TODO
        return 0;

    }

    internal void SetConnected(bool v)
    {
        Debug.Log("Connected : " + v);
        IsConnected = v;
    }

    public static int AllocateSceneViewID()
    {
        //TODO

        return 0;
    }

    #region instantiation
    public static GameObject Instantiate(string prefabName, Vector3 position, Quaternion quaternion, LexView parentView = null)
    {
        GameObject go = null;
        if (parentView != null && viewDictionary.ContainsKey(parentView.ViewID))
        {
          go = GameObject.Instantiate((GameObject)Resources.Load(prefabName), position, quaternion,viewDictionary[parentView.ViewID].transform);
        }
        else {
            go = GameObject.Instantiate((GameObject)Resources.Load(prefabName), position, quaternion);
        }
        LexView lv = go.GetComponent<LexView>();
        lv.SetInformation(LexNetwork_ViewID_Manager.RequestPrivateViewID(), LocalPlayer.actorID, LocalPlayer.actorID, false);
        Debug.Log("Instnatiate view id " + lv.ViewID);
        AddViewtoDictionary(lv);
        instance.Instantiate_Send(lv.ViewID, LocalPlayer.actorID, prefabName, position, quaternion, parentView, null);

        return go;
    }
    public static GameObject Instantiate(string prefabName, Vector3 position, Quaternion quaternion, DataType[] dataTypes, params object[] parameters)
    {
        GameObject go = GameObject.Instantiate((GameObject)Resources.Load(prefabName), position, quaternion);
        LexView lv = go.GetComponent<LexView>();
        lv.SetInstantiateData(parameters);
        lv.SetInformation(LexNetwork_ViewID_Manager.RequestPrivateViewID(), LocalPlayer.actorID, LocalPlayer.actorID, false);
        AddViewtoDictionary(lv);
        instance.Instantiate_Send(lv.ViewID, LocalPlayer.actorID, prefabName, position, quaternion, null, dataTypes,parameters);
        return go;
    }

    internal static void PrintStringToCode(string str)
    {
        char[] arr = str.ToCharArray();
        string code = "";
        foreach (char c in arr)
        {
            code += " " + (int)c;
        }
        Debug.Log(code);
        Debug.Log(str);
    }

    public static GameObject Instantiate(string prefabName, Vector3 position, Quaternion quaternion, LexView parentView, DataType[] dataTypes, params object[] parameters)
    {
        GameObject go = null;
        if (parentView != null && viewDictionary.ContainsKey(parentView.ViewID))
        {
            go = Instantiate((GameObject)Resources.Load(prefabName), position, quaternion, viewDictionary[parentView.ViewID].transform);
        }
        else
        {
            go = Instantiate((GameObject)Resources.Load(prefabName), position, quaternion);
        }
        LexView lv = go.GetComponent<LexView>();
        lv.SetInstantiateData(parameters);
        lv.SetInformation(instance.GetInstanceID(), LocalPlayer.actorID, LocalPlayer.actorID, false);
        AddViewtoDictionary(lv);
        instance.Instantiate_Send(lv.ViewID, LocalPlayer.actorID, prefabName, position, quaternion, parentView, dataTypes, parameters);
        return go;
    }

    public static GameObject InstantiateRoomObject(string prefabName, Vector3 position, Quaternion quaternion, LexView parentView = null)
    {
        GameObject go = null;
        if (parentView != null && viewDictionary.ContainsKey(parentView.ViewID))
        {
            go = GameObject.Instantiate((GameObject)Resources.Load(prefabName), position, quaternion, viewDictionary[parentView.ViewID].transform);
        }
        else
        {
            go = GameObject.Instantiate((GameObject)Resources.Load(prefabName), position, quaternion);
        }
        LexView lv = go.GetComponent<LexView>();
        lv.SetInformation(LexNetwork_ViewID_Manager.RequestRoomViewID(), MasterClient.actorID, LocalPlayer.actorID, true);
        Debug.Log("Instnatiate view id " + lv.ViewID);
        AddViewtoDictionary(lv);
        instance.Instantiate_Send(lv.ViewID, LocalPlayer.actorID, prefabName, position, quaternion, parentView, null);
        return go;
    }
    #endregion
    static int GetPing() {
        //TODO
        return 0;
    }

    public static bool CloseConnection(int kickPlayer) {
        //클라이언트에게 접속 해제를 요청 합니다.(KICK). 마스터 클라이언트만 이것을 수행 할 수 있습니다
        return true;
    }

    public static bool SetMasterClient(int masterPlayer) {
        //TODO
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

    public static void Destroy(LexView lv =null, GameObject destroyTarget = null, int viewID = -1) {
        /*
         Network-Destroy the GameObject, unless it is static or not under this client's control.

    Destroying a networked GameObject includes:

   1.  Removal of the Instantiate call from the server's room buffer.
   2. Removing RPCs buffered for PhotonViews that got created indirectly with the PhotonNetwork.Instantiate call.
   3. Sending a message to other clients to remove the GameObject also (affected by network lag).
   4. Usually, when you leave a room, the GOs get destroyed automatically. If you have to destroy a GO while not in a room, the Destroy is only done locally.

    Destroying networked objects works only if they got created with PhotonNetwork.Instantiate(). Objects loaded with a scene are ignored, no matter if they have PhotonView components.

    The GameObject must be under this client's control:

    Instantiated and owned by this client.
    Instantiated objects of players who left the room are controlled by the Master Client.
    Room-owned game objects are controlled by the Master Client.
    GameObject can be destroyed while client is not in a room.
    Returns
    Nothing. Check error debug log for any issues.
         */
        if (lv != null)
        {
            viewID = lv.ViewID;
            destroyTarget = lv.gameObject;
        }
        else if (destroyTarget != null)
        {
            lv = destroyTarget.GetComponent<LexView>();
            viewID = lv.ViewID;
        }
        else if (viewID != -1)
        {
            lv = viewDictionary[viewID];
            destroyTarget = lv.gameObject;
        }
        else {
            Debug.LogWarning("Wrong parameter");
            return;
        }

        if (!lv.IsMine) {
            Debug.LogWarning(viewID + " is not mine! ");
            return;
        }
        //Mutex
        mutex.WaitOne();
        RemoveRPCs(lv); //서버 버퍼에서 Instantiate와 모든 RPC제거
        Destroy(destroyTarget);//Replace with Object Pool
        viewDictionary.Remove(viewID);
        mutex.ReleaseMutex();
        //Mutex
        LexNetworkMessage netMessage = new LexNetworkMessage(LocalPlayer.actorID, (int)MessageInfo.Destroy, viewID);
        networkConnector.EnqueueAMessage(netMessage);
    }

    internal void Destroy_Receive(int viewID) {
        if (!viewDictionary.ContainsKey(viewID))
        {
            Debug.LogWarning(viewID + " does not exist!!");
            return;
        }

        //Mutex
        mutex.WaitOne();
        Destroy(viewDictionary[viewID].gameObject);
        viewDictionary.Remove(viewID);
        mutex.ReleaseMutex();
        //Mutex
    }

    public static void DestroyPlayerObjects(int actorID) {
        //플레이어나가면
        //1. [서버] 해당플레이어 모든 RPC제거
        //2. [서버] PlayerDisconnect 콜백
        //3. [클라이언트] 해당유저 Destroy 콜
        //4. [클라이언트] playerlist업데이트
        //Mutex
        mutex.WaitOne();
        var viewList = new List<LexView> (viewDictionary.Values);
        foreach (var lv in viewList) {
            if (lv.ownerActorNr == actorID) {
                viewDictionary.Remove(lv.ViewID);
                Destroy(lv.gameObject);
            }
        }
        mutex.ReleaseMutex();
        //Mutex
    }

    public static void RemoveRPCs(int actorID) {
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
        networkMessage.Add(MessageInfo.ServerRequest);
        networkMessage.Add(LexRequest.RemoveRPC);
        networkMessage.Add(actorID);
        networkMessage.Add("-1");
        networkConnector.EnqueueAMessage(networkMessage);

    }
    public static void RemoveRPCs(LexView lv)
    {
        LexNetworkMessage networkMessage = new LexNetworkMessage();
        networkMessage.Add(MessageInfo.ServerRequest);
        networkMessage.Add(LexRequest.RemoveRPC);
        networkMessage.Add("-1");
        networkMessage.Add(lv.ViewID);
        networkConnector.EnqueueAMessage(networkMessage);
    }

    /*

actorNum, RPC [int]viewID [string]FunctionName [object[...]]params

actorNum, SyncVar [int]viewID  [object[,,,]] params

actorNum, Chat [string]chat message (needs cleansing)

actorNum, Instantiate [int]viewID [string]prefabName [flaot,float,float] position [float,float,float]quarternion [object[...]] params

actorNum, Destroy [int]viewID

actorNum, SetHash [int]roomOrPlayer [string]Key [object]value


 */
   // RPC-send(NewsStyleUriParser Data[]{int, Double},sd,sd,sd)
    public static void RPC_Send(LexView lv, string functionName, DataType[] dataTypes = null, params object[] parameters ) {
        LexNetworkMessage netMessage = new LexNetworkMessage(LocalPlayer.actorID,(int)MessageInfo.RPC,lv.ViewID,functionName);
    
        netMessage.EncodeParameters(dataTypes, parameters);
        lv.gameObject.SendMessage(functionName, parameters);
        networkConnector.EnqueueAMessage(netMessage);
    }
    internal static void RPC_Receive(int viewID, string functionName, params object[] parameters)
    {
        if (viewDictionary.ContainsKey(viewID))
        {
            foreach (object obj in parameters) Debug.Log(obj);
            viewDictionary[viewID].gameObject.SendMessage(functionName, parameters);
        }
        else
        {
            Debug.LogWarning("No view id " + viewID);
        }
    }

    internal static string BuildFormatt(int expectedTokenLength , params object[] parameters) {
        string outt = NET_SIG+NET_DELIM+expectedTokenLength+NET_DELIM+parameters[0];
        for (int i = 1; i < parameters.Length; i++)
        {
            outt += NET_DELIM + parameters[i];
        }
        return outt;
    }


    public static void SyncVar_Send(LexView lv, DataType[] dataTypes, params object[] parameters) {
        LexNetworkMessage netMessage = new LexNetworkMessage(LocalPlayer.actorID, (int)MessageInfo.SyncVar, lv.ViewID);
        netMessage.EncodeParameters(dataTypes, parameters);
        networkConnector.EnqueueAMessage(netMessage);
    }
    internal static void SyncVar_Receive(int viewID,  params object[] parameters)
    {
        if (viewDictionary.ContainsKey(viewID))
        {
            foreach (object obj in parameters) Debug.Log(obj);

            if (viewDictionary[viewID].serializedView == null) {
                Debug.LogError("No sync view");
                return;
            }
            viewDictionary[viewID].ReceiveSerializedVariable(parameters);
        }
        else
        {
            Debug.LogWarning("No view id " + viewID);
        }
    }
    public static void Chat_Send(string chatMessage)
    {
        chatMessage = chatMessage.Replace(NET_DELIM, " ");
        LexNetworkMessage netMessage = new LexNetworkMessage(LocalPlayer.actorID, (int)MessageInfo.Chat, chatMessage);
        LexChatManager.AddChat(chatMessage);
        networkConnector.EnqueueAMessage(netMessage);
    }

    private void Instantiate_Send(int viewID, int ownerID, string prefabName,  Vector3 position, Quaternion quaternion, LexView parentView,DataType[] dataTypes, params object[] parameters)
    {
        LexNetworkMessage netMessage = new LexNetworkMessage(LocalPlayer.actorID, (int)MessageInfo.Instantiate, viewID, ownerID, prefabName, position,quaternion);
        if (parentView == null)
        {
            netMessage.Add("-1");
        }
        else
        {
            netMessage.Add(parentView.ViewID);
        }
        netMessage.EncodeParameters(dataTypes, parameters);
        networkConnector.EnqueueAMessage(netMessage);
    }



    internal static LexView GetViewByID(int ID)
    {
        //MUTEX

        mutex.WaitOne();
        if (viewDictionary.ContainsKey(ID))
        {
            return viewDictionary[ID];
        }
        else {
            return null;
        }
        mutex.ReleaseMutex();
        //MUTEX
    }
    internal static NetPlayer GetPlayerByID(int actorID)
    {
        //MUTEX
        if (playerDictionary.ContainsKey(actorID))
        {
            return playerDictionary[actorID];
        }
        else {
            return null;
        }
        //MUTEX
    }

    internal static void AddViewtoDictionary(LexView lv)
    {
        //MUTEX
        mutex.WaitOne();
        viewDictionary.Add(lv.ViewID, lv);
        mutex.ReleaseMutex();
        //MUTEX
    }
    internal static void RemoveViewFromDictionary(int viewID)
    {
        //MUTEX
        mutex.WaitOne();
        if (viewDictionary.ContainsKey(viewID)) {
            viewDictionary.Remove(viewID);
        }
        mutex.ReleaseMutex();
        //MUTEX
    }

    internal static void AddPlayerToDictionary(NetPlayer player)
    {
        //MUTEX
        mutex.WaitOne();
        playerDictionary.Add(player.actorID, player);
        if (player.IsMasterClient) {
            MasterClient = player;
        }
        mutex.ReleaseMutex();
        //MUTEX
    }
    internal static void RemovePlayerFromDictionary(int actorID)
    {
        //MUTEX
        mutex.WaitOne();
        if (playerDictionary.ContainsKey(actorID))
        {
            playerDictionary.Remove(actorID);
        }
        mutex.ReleaseMutex();
        //MUTEX
    }


    public static void SetRoomCustomProperties(RoomProperty key, string value)
    {
        CustomProperty.SetRoomSetting(key, value);
        //Needs to be synced with server
        //server needs to keep all hash settings
        instance.SetPlayerCustomProperty_Send(0, (int)key, value);
    }
    internal void SetPlayerCustomProperty_Send(int actorID, int key, string value) {
        LexNetworkMessage netMessage = new LexNetworkMessage(LocalPlayer.actorID,(int) MessageInfo.SetHash, actorID, key, value);
        networkConnector.EnqueueAMessage(netMessage);
    }

    internal void RoomProperty_Receive(RoomProperty key, string value) {
        CustomProperty.SetRoomSetting(key, value);
        //TODO : need to send event [callback]
    }
    public static void SetPlayerCustomProperties(int actorNr, PlayerProperty key, string value)
    {
        if (!playerDictionary.ContainsKey(actorNr)) {
            Debug.LogWarning("Missing player!" + actorNr);
            return;
        }
        playerDictionary[actorNr].SetCustomProperty(key,value);
    }


    internal static void SetLocalPlayer(NetPlayer player) {
        LocalPlayer = player;
        playerDictionary.Add(player.actorID, player);
        if (player.IsMasterClient) {
            IsMasterClient = true;
            MasterClient = player;
        }
    }


    private void Update()
    {
        NetTime += Time.deltaTime;
        networkConnector.DequeueReceivedBuffer();
    }
}
//Raycast
