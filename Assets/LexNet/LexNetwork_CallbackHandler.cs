using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LexNetwork_CallbackHandler
{
    public LexNetworkConnection networkConnector;
    bool timeSynched = false;
    bool rpcSynched = false;
    public void ParseCallback(int sentActorNumber, LexNetworkMessage netMessage)
    {
        //actorID , MessageInfo , callbackType, params
        string cbt = netMessage.GetNext();
        char[] arr = cbt.ToCharArray();
        string code = "";
        foreach (char c in arr) {
            code += " " + (int)c;
        }
        Debug.Log(code);
        Debug.Log("Callback type " + cbt);
        int cbtNum = Int32.Parse(cbt);
        LexCallback callbackType = (LexCallback)cbtNum;
        switch (callbackType)
        {
            case LexCallback.PlayerJoined:
                Handle_Receive_PlayerJoin(netMessage);
                break;
            case LexCallback.PlayerDisconnected:
                Handle_Receive_PlayerDisconnect(netMessage);
                break;
            case LexCallback.RoomInformationReceived:
                Handle_Receive_RoomInformation(sentActorNumber, netMessage);
                break;
            case LexCallback.MasterClientChanged:
                Handle_Receive_SetMasterClient(sentActorNumber,netMessage);
                break;
            case LexCallback.OnLocalPlayerJoined:
                Handle_Receive_LocalJoinFinish(sentActorNumber);
                break;
            case LexCallback.PushServerTime:
                Handle_Receive_ServerTime(netMessage);
                break;
        }

    }

    private void Handle_Receive_SetMasterClient(int sentActorNumber, LexNetworkMessage netMessage)
    {
        int nextMaster = Int32.Parse(netMessage.GetNext());
        LexNetwork.instance.SetMasterClient_Receive(sentActorNumber, nextMaster);
    }

    private void Handle_Receive_PlayerDisconnect(LexNetworkMessage netMessage)
    {
        //remove player dict
        //local destroy all rpc and obj
        int disconnActor = Int32.Parse(netMessage.GetNext());
        LexNetwork.RemovePlayerFromDictionary(disconnActor);
        LexNetwork.DestoryAll(disconnActor);
    }

    private void Handle_Receive_PlayerJoin(LexNetworkMessage netMessage)
    {
        NetPlayer player = new NetPlayer(false, netMessage);
        LexNetwork.AddPlayerToDictionary(player);
        NetworkEventManager.TriggerEvent(LexCallback.PlayerJoined,new NetEventObject(LexCallback.PlayerJoined) {objData = player });
    }

    private void Handle_Receive_ServerTime(LexNetworkMessage netMessage)
    {
        //LEX / 0 =SERVER / PING=MESSAGEINFO / targetPlater /1 OR 0 = INDEX TO REFER / SERVERTIME or EXPECTEDDELAY //Part of local Join
        int targetPlayer = Int32.Parse(netMessage.GetNext());
        bool isModification = Int32.Parse(netMessage.GetNext()) != 0;
        long timeValue = long.Parse(netMessage.GetNext());
        bool requestBufferedRPCs = Int32.Parse(netMessage.GetNext()) != 0;
        Debug.Log("Received servertime " + isModification + " / " + timeValue);
        Debug.Assert(targetPlayer == LexNetwork.LocalPlayer.actorID, "Received wrong message");
        LexNetwork.instance.SetServerTime(isModification, timeValue);
        if (!isModification)
        {
            RequestServerTime(true, requestBufferedRPCs);
        }
    }

    private void Handle_Receive_LocalJoinFinish(int sentActorNumber)
    {
        if (sentActorNumber != LexNetwork.LocalPlayer.actorID)
        {
            Debug.LogWarning("Not supposed to happen");
            return;
        }
        Debug.Log("Received RPCs and finished join");
        Debug.Assert(LexNetwork.IsConnected == false, "Connected but received rpc?");
        LexNetwork.instance.SetConnected(true);
        NetworkEventManager.TriggerEvent(LexCallback.OnLocalPlayerJoined, null);
    }

    private void Handle_Receive_RoomInformation(int sentActorNumber, LexNetworkMessage netMessage)
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
        //.1 소켓접속, 2. 룸정보 받기 , 3. bufferedrpc 받기, 4. 서버시간 받기,
    }

    void SyncNetworkTime() {
        RequestServerTime(false, false);
    }

    void RequestServerTime(bool requestModification, bool requestBufferedRPCs)
    {
        //LEX / 0 =SERVER / PING=MESSAGEINFO / targetPlater /1 OR 0 = INDEX TO REFER / SERVERTIME or EXPECTEDDELAY
        Debug.Log("Request servertime "+requestModification);
        LexNetworkMessage requestMessage = new LexNetworkMessage(
               LexNetwork.LocalPlayer.actorID, (int)MessageInfo.ServerRequest, (int)LexRequest.Receive_modifiedTime,
                LexNetwork.LocalPlayer.actorID, (requestModification)?"1":"0","0"
                , (requestBufferedRPCs) ? "1" : "0"
               );
        networkConnector.EnqueueAMessage(requestMessage);
    }
}

