using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyManager : MonobehaviourLexCallbacks
{
    private void Awake()
    {
        LexNetwork.ConnectUsingSettings();
    }
    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();

    }
}
