using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class LexViewManager : MonoBehaviour
{
    static LexViewManager instance;
    private static Dictionary<int, LexView> viewDictionary = new Dictionary<int, LexView>();
    [SerializeField] Text lvText;/*
    private static int privateViewID = 0;
    private static int roomViewID = 0;*/
    private static Queue<int> privateViewID_queue;
    private static Queue<int> roomViewID_queue;

    private static Mutex viewMutex = new Mutex();
    static bool init = false;

    [SerializeField] int nextPrivate = 0;
    [SerializeField] int nextRoom = 0;

    private static int exceededID = 0;
    private static int sceneViewNumbers = 0;
    private void Awake()
    {
        instance = this;
        Init();
    }
    private static void Init()
    {
        if (init) return;
        init = true;
        if (Application.isPlaying)
        {
            sceneViewNumbers = FindObjectsOfType<LexView>().Length;
            privateViewID_queue = new Queue<int>();
            roomViewID_queue = new Queue<int>();
            for (int i = sceneViewNumbers; i < LexNetwork.MAX_VIEW_IDS; i++)
            {
                privateViewID_queue.Enqueue(i);
                roomViewID_queue.Enqueue(i);
            }
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
            return userIDoffset + 100000 + (exceededID++);
        }
        int id = privateViewID_queue.Dequeue();
        instance.nextPrivate = privateViewID_queue.Peek();

        //MUTEX
        return id;
    }

    internal static bool CheckKey(int viewID)
    {
        if (viewDictionary.ContainsKey(viewID)) {
            return true;
        }
        else
        {
            Debug.LogWarning("No view id with " + viewID + " found");
            return false;
        } 
    }

    internal static List<LexView> GetViewList()
    {
        return new List<LexView>(viewDictionary.Values);
    }

    public static void AddViewtoDictionary(LexView lv)
    {
        viewDictionary.Add(lv.ViewID, lv);
    }
    public static void ReleaseViewID(bool isRoom, int id)
    {
        LexView lv = GetViewByID(id);
        if (!lv)
        {
            return;
        }
        viewDictionary.Remove(id);
        if (isRoom)
        {
            privateViewID_queue.Enqueue(id);
        }
        else
        {
            roomViewID_queue.Enqueue(id);
        }
        Destroy(lv.gameObject);
    }
    public static void ReleaseViewID(LexView lv)
    {
        if (lv == null){ return;}
        viewDictionary.Remove(lv.ViewID);
        if (lv.IsRoomView)
        {
            privateViewID_queue.Enqueue(lv.ViewID);
        }
        else
        {
            roomViewID_queue.Enqueue(lv.ViewID);
        }
        Destroy(lv.gameObject);
    }
    /*    private void Update()
        {
            lvText.text = privateViewID.ToString();
        }*/
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
        instance.nextRoom = roomViewID_queue.Peek();
        //  Debug.Log("Poll id " + id);
        //MUTEX
        return id;
    }
    public static int RequestSceneViewID()
    {
        Init();
        //MUTEX
        int id = sceneViewNumbers++;
        return id;
    }
    public static LexView GetViewByID(int ID)
    {
        if (CheckKey(ID))
        {
            return viewDictionary[ID];
        }
        else
        {
            return null;
        }
    }
    internal static void WaitMutex()
    {

        viewMutex.WaitOne();
    }
    internal static void ReleaseMutex()
    {
        viewMutex.ReleaseMutex();

    }

}
