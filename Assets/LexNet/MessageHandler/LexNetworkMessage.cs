using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static LexNetwork_MessageHandler;

public class LexNetworkMessage 
{
    List<object> paramQueue;
    Queue<string> receivedQueue = new Queue<string>();
    public LexNetworkMessage() {
        paramQueue = new List<object>();
    }
    public LexNetworkMessage(params object[] strings)
    {
        paramQueue = new List<object>(strings);
    }
    public void Add(MessageInfo s)
    {
        paramQueue.Add(((int)s).ToString());
    }
    public void Add(LexRequest s)
    {
        paramQueue.Add(((int)s).ToString());
    }
    public void Add(LexCallback s)
    {
        paramQueue.Add(((int)s).ToString());
    }
    public void Add(int s)
    {
        paramQueue.Add(s.ToString());
    }
    public void Add(string s) {
        paramQueue.Add(s);
    }
    public void Add(params object[] strings) {
        foreach (string s in strings)
        {
            paramQueue.Add(s);
        }
    }
    public string Build() {
        int count = paramQueue.Count + 2;/// signature and size 
        string message = NET_SIG + NET_DELIM + count;
        foreach(object s in paramQueue) { 
            message += NET_DELIM + s.ToString();
        }
        return message+ NET_DELIM;
        //3#hello
        //2#hello

        
        //3#hello#2#hello#
    }

/*    internal void EncodeParameters(DataType[] dataTypes, object[] parameters)
    {
        if (dataTypes == null)
        {
            paramQueue.Add("0");
            return;
        }
        else {
            paramQueue.Add(dataTypes.Length);
        }

        Debug.Assert(dataTypes.Length == parameters.Length);
        for (int i = 0; i < parameters.Length; i++)
        {
            paramQueue.Add((int)dataTypes[i]);
            paramQueue.Add(parameters[i]);
        }
    }*/
    internal void EncodeParameters(object[] parameters)
    {
       // Debug.Log("Parameters " + parameters);
        if ( parameters!=null && parameters.Length>0) {
          //  Debug.Log("Parameters size" + parameters.Length);
           // Debug.Log("Parameters 0" + parameters[0]);
            paramQueue.Add(parameters.Length);
        }
        else 
        {
            paramQueue.Add("0");
            return;
        }

        foreach(object o in parameters)
        {
            Type type = o.GetType();
            paramQueue.Add(type.Name);
            paramQueue.Add(o);
        }
    }


    public void Split(string message) {
        receivedQueue.Clear();
        message.Trim();
        string[] tokens = message.Split('#');
        foreach (string s in tokens) {
           // Debug.Log(receivedQueue.Count+" : "+ s);
            receivedQueue.Enqueue(s);
        }
    }
    public string GetNext() {
        return receivedQueue.Dequeue();
    }
    public int GetReceivedSize() {
        return receivedQueue.Count;
    }
    public bool HasNext() {
        return receivedQueue.Count > 0;
    }
    public string Peek()
    {
        if (receivedQueue.Count == 0) return "0";
        string msg = "";
        foreach (var s in receivedQueue) {
            msg += " " + s;
        }
        return receivedQueue.Count +" / "+ msg;
    }
}
public enum MessageInfo
{
    ServerRequest, RPC, SyncVar, Chat, Instantiate, Destroy, SetHash, ServerCallbacks
}
public enum LexCallback
{
    None, PlayerJoined, PlayerDisconnected, OnLocalPlayerJoined, MasterClientChanged,
    HashChanged,
    Disconnected,
    ModifyServerTime,
    RoomInformationReceived
        ,Ping_Received
}
public enum  LexRequest
{
    None, RemoveRPC, ChangeMasterClient, Receive_modifiedTime, Ping
}
/*

actorNum, RPC [int]viewID [string]FunctionName [object[...]]params

actorNum, SyncVar [int]viewID  [object[,,,]] params

actorNum, Chat [string]chat message (needs cleansing)

actorNum, Instantiate [int]viewID [string]prefabName [flaot,float,float] position [float,float,float]quarternion [object[...]] params

actorNum, Destroy [int]viewID

actorNum, SetHash [int]roomOrPlayer [string]Key [object]value


 */