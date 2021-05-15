using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using Random = UnityEngine.Random;

public class LobbyManager : MonobehaviourLexCallbacks
{
    private void Awake()
    {
        LexNetwork.ConnectUsingSettings();
        object ttt = (int)32;
        Debug.Log(ttt.GetType().Name);
        var converter = TypeDescriptor.GetConverter(ttt);
       //
        Debug.Log(converter);
        //
    }
    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        LexNetwork.SetRoomCustomProperties(RoomProperty.GameMode, Random.Range(0, 100) + "");
        LexNetwork.SetRoomCustomProperties(RoomProperty.GameMode, Random.Range(0, 100) + "");
        LexNetwork.SetRoomCustomProperties(RoomProperty.GameMode, Random.Range(0, 100) + "");
        LexNetwork.SetRoomCustomProperties(RoomProperty.GameMode, Random.Range(0, 100) + "");
        LexNetwork.SetRoomCustomProperties(RoomProperty.GameMode, Random.Range(0, 100) + "");
        LexNetwork.SetRoomCustomProperties(RoomProperty.GameMode, Random.Range(0, 100) + "");

        Debug.Log("Join successful");

        //...
        //0.1

        // 1초 -> 0.3초 -> 1초 (1.3초)
        // (최대 ping) 평균 50% + 1.15 -> 1.15 (1.3)
        //handshake <- 서버타임동기화/
    }
}
