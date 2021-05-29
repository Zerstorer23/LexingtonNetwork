using System;
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
    private LexNetwork_ViewID_Manager instance;
    private static Queue<int> privateViewID_queue;
    private static Queue<int> roomViewID_queue;
    static bool init = false;
    private void Awake()
    {

        instance = this;
        Init();
    }
    private static void Init() {
        if (init) return;
        init = true;
        privateViewID_queue = new Queue<int>();
        roomViewID_queue = new Queue<int>();
        for (int i = 0; i < LexNetwork.MAX_VIEW_IDS; i++)
        {
            privateViewID_queue.Enqueue(i);
            roomViewID_queue.Enqueue(i);
        }
    }
    public static int RequestPrivateViewID()
    {
        //TODO View iD 빠진거 순서대로 채우기
        //MUTEX
        int userIDoffset = LexNetwork.LocalPlayer.actorID * 10000;
        if (privateViewID_queue.Count <= 0)
        {
            Debug.LogWarning("Max view id exceeded / " + LexNetwork.MAX_VIEW_IDS);
            return userIDoffset+100000+ UnityEngine.Random.Range(0, LexNetwork.MAX_VIEW_IDS);
        }
        int id = privateViewID_queue.Dequeue();
        Debug.Log("Poll id " + id);
     
        //MUTEX
        return  + id;
    }
    public static void ReleaseViewID(bool isRoom, int id) {
        if (isRoom)
        {
            privateViewID_queue.Enqueue(id);
        }
        else {
            roomViewID_queue.Enqueue(id);
        }
    }
    private void Update()
    {
        lvText.text = privateViewID.ToString();
    }
    public static int RequestRoomViewID()
    {
        Init();
        //MUTEX
        if (roomViewID_queue.Count <= 0)
        {
            Debug.LogWarning("Max view id exceeded / " + LexNetwork.MAX_VIEW_IDS);
            return 100000 + UnityEngine.Random.Range(0, LexNetwork.MAX_VIEW_IDS);
        }
        int id = roomViewID_queue.Dequeue();
        Debug.Log("Poll id " + id);
        //MUTEX
        return id;
    }

}
