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
}
