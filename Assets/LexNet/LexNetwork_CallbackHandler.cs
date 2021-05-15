using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LexNetwork_CallbackHandler
{
    public void ParseCallback(int sentActorNumber, LexNetworkMessage netMessage)
    {
        //actorID , MessageInfo , callbackType, params
        LexCallback callbackType = (LexCallback)Int32.Parse(netMessage.GetNext());
        switch (callbackType)
        {
            case LexCallback.None:
                break;
            case LexCallback.PlayerJoined:
                break;
            case LexCallback.PlayerDisconnected:
                break;
            case LexCallback.LocalPlayerJoined:
                Handle_LocalPlayerJoin(sentActorNumber, netMessage);
                break;
            case LexCallback.Receive_RoomHash:
                break;
            case LexCallback.Receive_PlayerHash:
                break;
            case LexCallback.MasterClientChanged:
                break;
        }

    }

    private void Handle_LocalPlayerJoin(int sentActorNumber, LexNetworkMessage netMessage)
    {
        /*
         0 Sent Actor Num = -1
         1 MessageInfo = Callback
         2 RoomInfo Begin==
            2 - NumRoomHash
                1 - key (int)
                2 - value (string)
         3. Player Begin===
            4. NumPlayer
            5. LocalPlayer 
               6. id, ismaster, numParam , key..value...
         
         */
        //params = [int]numPlayers(local Included) , LocalPlayerInfo , players[...
        //Player Info = actorID, isMaster, customprop[num prop]
        //Load Room
        int numRoomHash = Int32.Parse(netMessage.GetNext());
        int count = 0;
        Debug.Log("Number of room hash : " + numRoomHash);
        while (count < numRoomHash) {
            RoomProperty key = (RoomProperty)Int32.Parse(netMessage.GetNext());
            string value = netMessage.GetNext();
            Debug.Log("room hash : " + key+" / "+value);
            LexNetwork.instance.RoomProperty_Receive(key, value);
            count++;
        }

        //Load Players
        int numPlayers = Int32.Parse(netMessage.GetNext());
        Debug.Log("Number of Players: " + numRoomHash);
        count = 1;
        NetPlayer localPlayer = new NetPlayer(true, netMessage);
        LexNetwork.SetLocalPlayer(localPlayer);
        while (count < numPlayers)
        {
            NetPlayer player = new NetPlayer(false, netMessage);
            LexNetwork.AddPlayerToDictionary(player);
            count++;
        }

        LexNetwork.instance.SetConnected(true);
        NetworkEventManager.TriggerEvent(LexCallback.LocalPlayerJoined, null);
    }
}

public enum MessageInfo {
    ServerRequest, RPC,SyncVar,Chat,Instantiate,Destroy,SetHash, ServerCallbacks
}
public enum LexCallback
{
    None, PlayerJoined,PlayerDisconnected,LocalPlayerJoined,Receive_RoomHash,Receive_PlayerHash,MasterClientChanged
}
public enum LexRequest
{
    None, RemoveRPC_ViewID, RemoveRPC_Player,Receive_Initialise,Receive_RPCbuffer
}
/*

actorNum, RPC [int]viewID [string]FunctionName [object[...]]params

actorNum, SyncVar [int]viewID  [object[,,,]] params

actorNum, Chat [string]chat message (needs cleansing)

actorNum, Instantiate [int]viewID [string]prefabName [flaot,float,float] position [float,float,float]quarternion [object[...]] params

actorNum, Destroy [int]viewID

actorNum, SetHash [int]roomOrPlayer [string]Key [object]value


 */