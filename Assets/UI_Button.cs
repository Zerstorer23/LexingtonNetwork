using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Button : MonoBehaviour
{
    // Start is called before the first frame update
    public void OnClick_Connect() {
        LexNetwork.ConnectUsingSettings();
    
    }
    public void OnClick_Disconnect()
    {
        LexNetwork.Disconnect();
    }

    [SerializeField] LexView testView;
    public Vector3 targetPos;
    public void OnClick_MoveRPC() {
        testView.RPC("Move", Photon.Pun.RpcTarget.All, targetPos, "hi");
    }
    public void OnClick_Instantiate()
    {
        LexNetwork.InstantiateRoomObject("netObj", transform.position, Quaternion.identity);
    }
    public void OnClick_InstantiatePrivate()
    {
        LexNetwork.Instantiate("netObj", transform.position, Quaternion.identity);
    }
    public void OnClick_RoomHash()
    {
        LexHashTable hash = new LexHashTable();
        hash.Add(RoomProperty.GameMode, Random.Range(0, 100) + "");
        LexNetwork.SetRoomCustomProperties(hash);
    }
    public void OnClick_PlayerHash()
    {
        LexHashTable hash = new LexHashTable();
        hash.Add(PlayerProperty.NickName, Random.Range(0, 100) + "");
        LexNetwork.SetPlayerCustomProperties(hash);
    }
}
