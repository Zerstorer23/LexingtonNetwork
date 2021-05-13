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
        LexNetwork.RPC_Send(testView, "Move",
            new DataType[] {DataType.VECTOR3},
            targetPos
            );
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
        LexNetwork.SetRoomCustomProperties(RoomProperty.GameMode, Random.Range(0, 100)+"");
    }
    public void OnClick_PlayerHash()
    {
        LexNetwork.SetPlayerCustomProperties(LexNetwork.LocalPlayer.actorID, PlayerProperty.NickName, Random.Range(0, 100) + "");
    }
}
