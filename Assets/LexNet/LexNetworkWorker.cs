using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public partial class LexNetwork
{

    private static Dictionary<int, LexPlayer> playerDictionary = new Dictionary<int, LexPlayer>();
    private Mutex playerDictionaryMutex = new Mutex();
    private static LexPlayer[] GetPlayerList()
    {
        return playerDictionary.Values.ToArray();
    }
    private static LexPlayer[] GetPlayerListOthers()
    {
        return playerDictionary.Values.OrderBy((x) => x.actorID).Where(x => !x.IsLocal).ToArray();
    }
    private static int GetPlayerCount() {

        return (useLexNet)?playerDictionary.Count : PhotonNetwork.CurrentRoom.PlayerCount;
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

    internal void SetLocalPlayer(LexPlayer player)
    {
        LocalPlayer = player;
        Debug.Log("Local player set : " + player.actorID);
        //    playerDictionary.Add(player.actorID, player);
    }
    internal static LexPlayer GetPlayerByID(int actorID)
    {
        if (playerDictionary.ContainsKey(actorID))
        {
            return playerDictionary[actorID];
        }
        else
        {
            return null;
        }
    }
    public void AddPlayerToDictionary(LexPlayer player)
    {
        playerDictionaryMutex.WaitOne();
        playerDictionary.Add(player.actorID, player);
        if (player.IsMasterClient)
        {
            LexNetwork.instance.SetMasterClient_Receive(player.actorID, player.actorID);
            
           // MasterClient = player;
        }
        playerDictionaryMutex.ReleaseMutex();
    }
    public void RemovePlayerFromDictionary(int actorID)
    {
        playerDictionaryMutex.WaitOne();
        if (playerDictionary.ContainsKey(actorID))
        {
            playerDictionary.Remove(actorID);
        }
        playerDictionaryMutex.ReleaseMutex();
    }


    internal void CustomProperty_Send(int actorID, LexHashTable hash)
    {
        LexNetworkMessage netMessage = new LexNetworkMessage(LocalPlayer.actorID, (int)MessageInfo.SetHash, actorID,hash.lexHash.Count);
        foreach (var entry in hash.lexHash) {
            netMessage.Add(entry.Key);
            netMessage.Add(entry.Value);
        }
        networkConnector.EnqueueAMessage(netMessage);
    }

    internal void CustomProperty_Receive(int actorID, LexHashTable hash)
    {
        if (actorID == 0)
        {
            CustomProperties.UpdateProperties(hash);
        }
        else {
            GetPlayerByID(actorID).CustomProperties.UpdateProperties(hash);
        }
    }
    internal void Destroy_Receive(int viewID)
    {

    }
    public void Instantiate_Send(int viewID, int ownerID, string prefabName, Vector3 position, Quaternion quaternion,object[] parameters)
    {
        LexNetworkMessage netMessage = new LexNetworkMessage(LocalPlayer.actorID, (int)MessageInfo.Instantiate, viewID, ownerID, prefabName, position, quaternion);
        netMessage.EncodeParameters( parameters);
        networkConnector.EnqueueAMessage(netMessage);
    }


    public void SyncVar_Send(LexView lv, params object[] parameters)
    {
        LexNetworkMessage netMessage = new LexNetworkMessage(LocalPlayer.actorID, (int)MessageInfo.SyncVar, lv.ViewID);
        netMessage.EncodeParameters(parameters);
        networkConnector.EnqueueAMessage(netMessage);
    }
    public void SyncVar_Receive(int viewID, params object[] parameters)
    {
        LexView lv = LexViewManager.GetViewByID(viewID);
        if (!lv) return;

        if (debugLexNet)
        {
            foreach (object obj in parameters) Debug.Log(obj);
        }
        lv.ReceiveSerializedVariable(parameters);
    }
    /*

actorNum, RPC [int]viewID [string]FunctionName [object[...]]params

actorNum, SyncVar [int]viewID  [object[,,,]] params

actorNum, Chat [string]chat message (needs cleansing)

actorNum, Instantiate [int]viewID [string]prefabName [flaot,float,float] position [float,float,float]quarternion [object[...]] params

actorNum, Destroy [int]viewID

actorNum, SetHash [int]roomOrPlayer [string]Key [object]value


*/


    public void DestroyPlayerObjects(int actorID)
    {
        //플레이어나가면
        //1. [서버] 해당플레이어 모든 RPC제거
        //2. [서버] PlayerDisconnect 콜백
        //3. [클라이언트] 해당유저 Destroy 콜
        //4. [클라이언트] playerlist업데이트
        //Mutex
        var viewList = LexViewManager.GetViewList();
        foreach (var lv in viewList)
        {
            if (lv.ownerActorNr == actorID)
            {
                LexViewManager.ReleaseViewID(lv);
            }
        }
        //Mutex
    }
    public void SetMasterClient_Receive(int sentActorNumber, int nextMaster)
    {
        //지금마스터 해제
        //새 마스터 등록
        //view아이디 owner정보 변경
        playerDictionary[sentActorNumber].IsMasterClient = false;
        playerDictionary[nextMaster].IsMasterClient = true;
        MasterClient = playerDictionary[nextMaster];
        if (nextMaster == LocalPlayer.actorID)
        {
            IsMasterClient = true;
        }
        else {
            IsMasterClient = false;
        }
        var viewList = LexViewManager.GetViewList();
        foreach (var entry in viewList)
        {
            entry.UpdateOwnership();
        }
    }
    internal void SetServerTime(bool isModification, long timeValue)
    {
        if (isModification)
        {
            NetTime += (double)timeValue / 1000;
        }
        else
        {
            NetTime = (double)timeValue / 1000; //long is in mills
        }
        Debug.Log("Modified time : " + NetTime);
    }

    double lastSentPing = 0;
    double lastReceivedPing = 0;
    double pingPeriodInSec = 5;
    public void SendPing()
    {
        lastSentPing = NetTime;
        LexNetworkMessage netMessage = new LexNetworkMessage();
        netMessage.Add(LexNetwork.LocalPlayer.actorID);
        netMessage.Add(MessageInfo.ServerRequest);
        netMessage.Add(LexRequest.Ping);
        networkConnector.EnqueueAMessage(netMessage);
    }
    public void ReceivePing()
    {
        lastReceivedPing = NetTime;
    }
    internal void SetConnected(bool v)
    {
        Debug.Log("Connected : " + v);
        IsConnected = v;
    }
}
