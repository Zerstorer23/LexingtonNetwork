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
            case LexCallback.RoomInformationReceived:
                Handle_Receive_RoomInformation(sentActorNumber, netMessage);
                break;
            case LexCallback.MasterClientChanged:
                break;
            case LexCallback.BufferedRPCsLoaded:
                Handle_Receive_BufferedRPCs(sentActorNumber);
                break;
        }

    }

    private void Handle_Receive_BufferedRPCs(int sentActorNumber)
    {
        if (sentActorNumber != LexNetwork.LocalPlayer.actorID) {
            Debug.LogWarning("Not suppoed to happen");
            return;
        }
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
        LexNetwork.instance.RequestBufferedRPCs();
    }
}

