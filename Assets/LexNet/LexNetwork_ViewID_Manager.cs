using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class LexNetwork_ViewID_Manager : MonoBehaviour
{
    [SerializeField] Text lvText;
    [SerializeField] private static int privateViewID = 0;
    [SerializeField] private static int roomViewID = 0;
    public static int RequestPrivateViewID()
    {
        //MUTEX
        int id = privateViewID++;
        Debug.Log("Poll id " + id);
        
        //MUTEX
        return LexNetwork.LocalPlayer.actorID * 10000 + id;
    }
    private void Update()
    {
        lvText.text = privateViewID.ToString();
    }
    public static int RequestRoomViewID()
    {
        //MUTEX
        int id = roomViewID++;
        Debug.Log("Poll id " + id);
        //MUTEX
        return id;
    }
}
