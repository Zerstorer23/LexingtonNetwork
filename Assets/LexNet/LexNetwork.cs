using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System;
using static LexNetworkConnection;
[ExecuteAlways]
public class LexNetwork : MonoBehaviour
{
    private static Dictionary<int, LexView> viewDictionary = new Dictionary<int, LexView>();
    private static Dictionary<int, NetPlayer> playerDictionary  = new Dictionary<int, NetPlayer>();
    
    static readonly int MAX_VIEW_IDS = 1000;

    public static string ServerAddress;
    public static bool connected;
    public static NetPlayer LocalPlayer { get; private set; }
    public static NetPlayer MasterClient { get; private set; }
    public static string NickName { get; private set; }
//    public static NetPlayer[] playerList;
    public static double Time { get; private set; }
    public static bool IsMasterClient { get; private set; }
    public static int countOfPlayersInRoom;

    static LexNetworkConnection networkConnector = new LexNetworkConnection();
    private static LexNetwork prNetwork;

    [SerializeField] private int privateViewID = 0;
    [SerializeField] private int roomViewID = 0;
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

                }
            }
            return prNetwork;
        }
    }
    public static bool ConnectUsingSettings() {
        bool success = networkConnector.Connect();
        //TODO
        //1 소켓 연결
        //2 연결 성공시 Request(플레이어 정보, 해시정보 로드
        //3.해시로드callback받기
        //4. Request Buffered RPC
        Debug.Log("Connection..."+success);
        return success;
    }
  
    public static bool Reconnect() {
        return true;
    }

    public static void Disconnect() {
        networkConnector.Disconnect();
    }

  

    public static int AllocateViewID() {
        return 0;

    }
    public static int AllocateSceneViewID() {

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
        lv.SetInformation(instance.RequestPrivateViewID(), LocalPlayer.actorID, LocalPlayer.actorID, false);
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
        lv.SetInformation(instance.RequestPrivateViewID(), LocalPlayer.actorID, LocalPlayer.actorID, false);
        AddViewtoDictionary(lv);
        instance.Instantiate_Send(lv.ViewID, LocalPlayer.actorID, prefabName, position, quaternion, null, dataTypes,parameters);
        return go;
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
        lv.SetInformation(instance.RequestRoomViewID(), MasterClient.actorID, LocalPlayer.actorID, true);
        Debug.Log("Instnatiate view id " + lv.ViewID);
        AddViewtoDictionary(lv);
        instance.Instantiate_Send(lv.ViewID, LocalPlayer.actorID, prefabName, position, quaternion, parentView, null);
        return go;
    }
    #endregion
    static int GetPing() {
        return 0;
    }

    public static bool CloseConnection(int kickPlayer) {
        //클라이언트에게 접속 해제를 요청 합니다.(KICK). 마스터 클라이언트만 이것을 수행 할 수 있습니다
        return true;
    }

    public static bool SetMasterClient(int masterPlayer) {
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
        RemoveRPCs(lv); //서버 버퍼에서 Instantiate와 모든 RPC제거
        Destroy(destroyTarget);//Replace with Object Pool
        viewDictionary.Remove(viewID);
        //Mutex
        string message = BuildFormat(LocalPlayer.actorID, (int)MessageInfo.Destroy, viewID);
        networkConnector.EnqueueAMessage(message);
    }

    internal void Destroy_Receive(int viewID) {
        if (!viewDictionary.ContainsKey(viewID))
        {
            Debug.LogWarning(viewID + " does not exist!!");
            return;
        }

        //Mutex
        Destroy(viewDictionary[viewID].gameObject);
        viewDictionary.Remove(viewID);
        //Mutex
    }

    public static void DestroyPlayerObjects(int actorID) {
        //플레이어나가면
        //1. [서버] 해당플레이어 모든 RPC제거
        //2. [서버] PlayerDisconnect 콜백
        //3. [클라이언트] 해당유저 Destroy 콜
        //4. [클라이언트] playerlist업데이트
        //Mutex
        var viewList = new List<LexView> (viewDictionary.Values);
        foreach (var lv in viewList) {
            if (lv.ownerActorNr == actorID) {
                viewDictionary.Remove(lv.ViewID);
                Destroy(lv.gameObject);
            }
        }
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


    }
    public static void RemoveRPCs(LexView lv)
    {
        /*
        Remove all buffered RPCs from server that were sent via targetPhotonView.The Master Client and the owner of the targetPhotonView may call this.

       This method requires either:

        The targetPhotonView is owned by this client(Instantiated by it).
This client is the Master Client(can remove any PhotonView's RPCs).
Parameters
targetPhotonView    RPCs buffered for this PhotonView get removed from server buffer.

         */
        //1. 내 송수신버퍼에 actorNumber관련 모든 RPC제거    
        //2. 서버 request 버퍼에 모든 플레이어로부터 rpc제거  <- 이거만 수행
        //3. 서버 callback 수신 rpc 제거


    }

    [SerializeField] LexView testView;
    private void Start()
    {

     //   RPC_Send(testView, "DoTest", 1, 1f, 0.22d, "hi");
    }
    /*

actorNum, RPC [int]viewID [string]FunctionName [object[...]]params

actorNum, SyncVar [int]viewID  [object[,,,]] params

actorNum, Chat [string]chat message (needs cleansing)

actorNum, Instantiate [int]viewID [string]prefabName [flaot,float,float] position [float,float,float]quarternion [object[...]] params

actorNum, Destroy [int]viewID

actorNum, SetHash [int]roomOrPlayer [string]Key [object]value


 */

    private void Awake()
    {
        LocalPlayer = new NetPlayer(true, 1, "LOCAL");
        viewDictionary.Add(testView.ViewID, testView);
    }
    private static string EncodeParameters(DataType[] dataTypes, params object[] parameters)
    {
        string message = string.Empty;
        if (dataTypes == null)
        {
            message += NET_DELIM + 0;
            return message;
        }
        
        message += NET_DELIM + dataTypes.Length;
        Debug.Assert(dataTypes.Length == parameters.Length);
        for (int i = 0; i < parameters.Length; i++)
        {
            message += NET_DELIM + (int)dataTypes[i];
            message += NET_DELIM + parameters[i];
        }
        return message;
    }

    public static void RPC_Send(LexView lv, string functionName, DataType[] dataTypes = null, params object[] parameters ) {
        string message = BuildFormat(LocalPlayer.actorID,(int)MessageInfo.RPC,lv.ViewID,functionName);
        message += EncodeParameters(dataTypes, parameters);
        lv.gameObject.SendMessage(functionName, parameters);
        networkConnector.EnqueueAMessage(message);
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

    static string BuildFormat(params object[] parameters) {
        string outt = ""+parameters[0];
        for (int i = 1; i < parameters.Length; i++)
        {
            outt += NET_DELIM + parameters[i];
        }
        return outt;
    }


    public static void SyncVar_Send(LexView lv, DataType[] dataTypes, params object[] parameters) {
        string message = BuildFormat(LocalPlayer.actorID, (int)MessageInfo.SyncVar, lv.ViewID);
        message += EncodeParameters(dataTypes, parameters);
        networkConnector.EnqueueAMessage(message);
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
        chatMessage = chatMessage.Replace(" ", "_");
        string message = BuildFormat(LocalPlayer.actorID, (int)MessageInfo.Chat, chatMessage);
        LexChatManager.AddChat(message);
        networkConnector.EnqueueAMessage(message);
    }

    private void Instantiate_Send(int viewID, int ownerID, string prefabName,  Vector3 position, Quaternion quaternion, LexView parentView,DataType[] dataTypes, params object[] parameters)
    {
        string message = BuildFormat(LocalPlayer.actorID, (int)MessageInfo.Instantiate, viewID, ownerID, prefabName, position,quaternion);
        if (parentView == null)
        {
            message += NET_DELIM + "-1";
        }
        else
        {
            message += NET_DELIM + parentView.ViewID;
        }
        message += EncodeParameters(dataTypes, parameters);
        networkConnector.EnqueueAMessage(message);
    }



    internal static LexView GetViewByID(int ID)
    {
        //MUTEX
        if (viewDictionary.ContainsKey(ID))
        {
            return viewDictionary[ID];
        }
        else {
            return null;
        }
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
        viewDictionary.Add(lv.ViewID, lv);
        //MUTEX
    }
    internal static void RemoveViewFromDictionary(int viewID)
    {
        //MUTEX
        if (viewDictionary.ContainsKey(viewID)) {
            viewDictionary.Remove(viewID);
        }
        //MUTEX
    }

    internal static void AddPlayerToDictionary(NetPlayer player)
    {
        //MUTEX
        playerDictionary.Add(player.actorID, player);
        //MUTEX
    }
    internal static void RemovePlayerFromDictionary(int actorID)
    {
        //MUTEX
        if (playerDictionary.ContainsKey(actorID))
        {
            playerDictionary.Remove(actorID);
        }
        //MUTEX
    }
    public int RequestPrivateViewID() {
        //MUTEX
        int id = privateViewID++;
        //MUTEX
        return id;
    }
    public int RequestRoomViewID()
    {
        //MUTEX
        int id = roomViewID++;
        //MUTEX
        return id;
    }

    public static void SetRoomCustomProperties(string key, object value)
    {
        CustomProperty.SetRoomSetting(key, value);
        //Needs to be synced with server
        //server needs to keep all hash settings
        instance.SetPlayerCustomProperty_Send(0, key, value);
    }
    internal void SetPlayerCustomProperty_Send(int actorID, string key, object value) { 
         string message = BuildFormat(LocalPlayer.actorID, MessageInfo.SetHash, actorID, key, value);
        networkConnector.EnqueueAMessage(message);
    }

    internal void RoomProperty_Receive(string key, object value) {
        CustomProperty.SetRoomSetting(key, value);
        //TODO : need to send event [callback]
    }
    public static void SetPlayerCustomProperties(int actorNr, string key, object value)
    {
        if (!playerDictionary.ContainsKey(actorNr)) {
            Debug.LogWarning("Missing player!" + actorNr);
            return;
        }
        playerDictionary[actorNr].SetCustomProperty(key,value);
    }
    private void Update()
    {
        networkConnector.DequeueReceivedBuffer();
    }
}
