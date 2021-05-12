using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LexMessage
{

}

public enum MessageInfo {
    ServerCallbacks, RPC,SyncVar,Chat,Instantiate,Destroy,SetHash, ServerRequest
}
public enum LexCallback
{
    None, PlayerJoined,PlayerDisconnected,LocalPlayerJoined,Receive_RoomHash,Receive_PlayerHash,MasterClientChanged
}
public enum LexRequest
{
    None, RemoveRPC_ViewID, RemoveRPC_Player,Receive_RoomHash,Receive_PlayerHash
}
/*

actorNum, RPC [int]viewID [string]FunctionName [object[...]]params

actorNum, SyncVar [int]viewID  [object[,,,]] params

actorNum, Chat [string]chat message (needs cleansing)

actorNum, Instantiate [int]viewID [string]prefabName [flaot,float,float] position [float,float,float]quarternion [object[...]] params

actorNum, Destroy [int]viewID

actorNum, SetHash [int]roomOrPlayer [string]Key [object]value


 */