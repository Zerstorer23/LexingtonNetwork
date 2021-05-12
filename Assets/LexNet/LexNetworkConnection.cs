using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using UnityEngine;

public class LexNetworkConnection
{
    string ipAddress = "127.0.0.1";
    int portNumber = 9000;
    static int BUFFER = 1024;
    public readonly static string NET_DELIM = "#";

    Queue<string> receivedQueue = new Queue<string>();
     Queue<string> sendQueue = new Queue<string>();
    Thread listenThread;
    Thread sendThread;
    Socket mySocket;

    bool stayConnected = true;
    // Start is called before the first frame update

    public bool Connect()
    {
        stayConnected = true;
        mySocket = new Socket(
              AddressFamily.InterNetwork,
              SocketType.Stream,
              ProtocolType.Tcp
              );//소켓 생성
                //인터페이스 결합(옵션)
                //연결
        IPAddress addr = IPAddress.Parse(ipAddress);
        IPEndPoint iep = new IPEndPoint(addr, portNumber);
        try
        {
            Debug.Log("Connecting...");
            mySocket.Connect(iep);
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            Debug.Log(e.StackTrace);
            return false;
        }
        Debug.Log("Connection made!");

        listenThread = new Thread(new ThreadStart(ListenMessage));
        listenThread.IsBackground = true;
        listenThread.Start();
        Debug.Log("Listening...");

        sendThread = new Thread(new ThreadStart(SendMessage));
        sendThread.IsBackground = true;
        sendThread.Start();
        Debug.Log("Writing...");
        return true;
    }
    public void SendMessage()
    {
        while (stayConnected)
        {
            while (sendQueue.Count > 0)
            {
                //MUTEX
                SendAMessage(sendQueue.Dequeue());
                //MUTEX
            }
        }
    }
    public void EnqueueAMessage(string str)
    {
        //MUTEX
        Debug.Log("Enqueue : " + str);
        sendQueue.Enqueue(str);
        //MUTEX
    }

    private void SendAMessage(string str)
    {
        try
        {
            byte[] packet = new byte[BUFFER];
            MemoryStream ms = new MemoryStream(packet);
            BinaryWriter bw = new BinaryWriter(ms);
            bw.Write(str);
            bw.Close();
            ms.Close();
            mySocket.Send(packet);
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            Debug.Log(e.StackTrace);
        }
    }

    private void ListenMessage()
    {

        string str;
        byte[] packet = new byte[BUFFER];
        while (stayConnected)
        {

            mySocket.Receive(packet);
            MemoryStream ms2 = new MemoryStream(packet);
            BinaryReader br = new BinaryReader(ms2);
            str = br.ReadString();
            receivedQueue.Enqueue(str);
            Debug.Log("수신한 메시지:" + str);
            br.Close();
            ms2.Close();

        }

    }
    //MainThread에서만 GameObject조작가능
    //그래서queue에 저장후  따로 Update에서 호출하도록함
    public void DequeueReceivedBuffer() {
        while (receivedQueue.Count > 0) {
            HandleMessage(receivedQueue.Dequeue());
        }
    }

     void HandleMessage(string str)
    {
        string[] tokens = str.Split('#');
        //int i = 0;
       // foreach (string t in tokens) Debug.Log(t +" "+(i++) +" / "+tokens.Length);
        
        Debug.Assert(tokens.Length >= 2," Token information wrong");
        int sentActorNumber =  Int32.Parse(tokens[0]);
        MessageInfo messageInfo = (MessageInfo)Int32.Parse(tokens[1]);
        Debug.Log(sentActorNumber+ " message " + messageInfo);
        switch (messageInfo)
        {
            case MessageInfo.RPC:
                ParseRPC(sentActorNumber, tokens);
                break;
            case MessageInfo.SyncVar:
                ParseSyncVar(sentActorNumber, tokens);
                break;
            case MessageInfo.Chat:
                ParseChat(sentActorNumber, tokens);
                break;
            case MessageInfo.Instantiate:
                ParseInstantiate(sentActorNumber, tokens);
                break;
            case MessageInfo.Destroy:
                ParseDestroy(sentActorNumber, tokens);
                break;
            case MessageInfo.SetHash:
                ParseSetHash(sentActorNumber, tokens);
                break;
            case MessageInfo.ServerRequest:
                break;
            case MessageInfo.ServerCallbacks:
                break;
        }
    }

    private void ParseSetHash(int sentActorNumber, string[] tokens)
    {
        //actorNum, SetHash [int]roomOrPlayer [string]Key [int] DataType [object]value
        if (sentActorNumber == LexNetwork.LocalPlayer.actorID) return;
        int targetHashID = Int32.Parse(tokens[2]); //0 = Room,
        string key = tokens[3];
        object value = ParseParameters(1, tokens, 4)[0];
        
        if (targetHashID == 0)
        {
            LexNetwork.instance.RoomProperty_Receive(key, value);
        }
        else {
            LexNetwork.SetPlayerCustomProperties(targetHashID, key, value);
        }
    }

    private void ParseDestroy(int sentActorNumber, string[] tokens)
    {
        if (sentActorNumber == LexNetwork.LocalPlayer.actorID) return;
        int targetViewID = Int32.Parse(tokens[2]);
        LexNetwork.instance.Destroy_Receive(targetViewID);

    }

    private void ParseInstantiate(int sentActorNumber, string[] tokens)
    {
       if (sentActorNumber == LexNetwork.LocalPlayer.actorID) return;
        //actorNum, Instantiate [int]viewID [int]ownerID [string]prefabName [flaot,float,float] position [float,float,float]quarternion parentviewID [object[...]] params
        int targetViewID = Int32.Parse(tokens[2]);
        int ownerID = Int32.Parse(tokens[3]);
        string prefabName = tokens[4];
        Vector3 position = StringToVector3(tokens[5]);
        Quaternion quaternion = StringToQuarternion(tokens[6]);
        int parentID = Int32.Parse(tokens[7]);
        LexView plv = LexNetwork.GetViewByID(parentID);
        GameObject go = null;
        if (plv != null)
        {
            go= GameObject.Instantiate((GameObject)Resources.Load(prefabName), position, quaternion, plv.transform);
        }
        else {
            go = GameObject.Instantiate((GameObject)Resources.Load(prefabName), position, quaternion);
        }
        LexView childView = go.GetComponent<LexView>();
        childView.SetInformation(targetViewID,ownerID,sentActorNumber,false);
        //Params
        int numParams = Int32.Parse(tokens[8]);
        if (numParams > 0) {
            var param = ParseParameters(numParams, tokens, 9);
            childView.SetInstantiateData(param);
        }
        LexNetwork.AddViewtoDictionary(childView);
    }

    private void ParseChat(int sentActorNumber, string[] tokens)
    {
        if (sentActorNumber == LexNetwork.LocalPlayer.actorID) return;
        //actorNum, Chat [string]chat message (needs cleansing)
        string message = tokens[2].Replace(NET_DELIM," ");
        LexChatManager.AddChat(message);
    }

    private void ParseSyncVar(int sentActorNumber, string[] tokens)
    {
        //actorNum, SyncVar [int]viewID  [int]numparam ,[object[,,,]] params
        int targetViewID = Int32.Parse(tokens[2]);
        int numParams = Int32.Parse(tokens[3]);
        Debug.Assert(numParams != 0,"Syncing what?");
        var param = ParseParameters(numParams, tokens, 4);
        if (sentActorNumber == LexNetwork.LocalPlayer.actorID) return;
        LexNetwork.SyncVar_Receive(targetViewID, param);
    }

    //  actorNum, RPC[int] viewID[string] FunctionName[object[...]]params
    private void ParseRPC(int sentActorNumber, string[] tokens)
    {
        Debug.Assert(tokens.Length >= 3, " Token information wrong");
        int targetViewID = Int32.Parse(tokens[2]);
        string functionName = tokens[3];
        int numParams = Int32.Parse(tokens[4]);
        if (numParams == 0)
        {
            LexNetwork.RPC_Receive(targetViewID, functionName);
        }
        else
        {
            var param = ParseParameters(numParams, tokens, 5);
            LexNetwork.RPC_Receive(targetViewID, functionName, param);
        }
    }

    object[] ParseParameters(int numParams, string[] tokens, int beginIndex) {
        object[] param = new object[numParams];
        int index = 0;
        for (int i = beginIndex; i < tokens.Length; i += 2)
        {
            DataType dType = (DataType)Int32.Parse(tokens[i]);
            string dataInfo = tokens[i + 1];
            switch (dType)
            {
                case DataType.STRING:
                    param[index] = dataInfo;
                    break;
                case DataType.INT:
                    param[index] = Int32.Parse(dataInfo);
                    break;
                case DataType.DOUBLE:
                    param[index] = Double.Parse(dataInfo);
                    break;
                case DataType.FLOAT:
                    param[index] = float.Parse(dataInfo);
                    break;
                case DataType.VECTOR3:
                    param[index] = StringToVector3(dataInfo);
                    break;
            }
        }
        return param;
    }

    public void Disconnect()
    {
        stayConnected = false;
        mySocket.Close();//소켓 닫기
        sendThread.Join();
        listenThread.Join();
    }


    private  Vector3 StringToVector3(string sVector)
    {
        // Remove the parentheses
        if (sVector.StartsWith("(") && sVector.EndsWith(")"))
        {
            sVector = sVector.Substring(1, sVector.Length - 2);
        }

        // split the items
        string[] sArray = sVector.Split(',');

        // store as a Vector3
        Vector3 result = new Vector3(
            float.Parse(sArray[0]),
            float.Parse(sArray[1]),
            float.Parse(sArray[2]));

        return result;
    }
    private Quaternion StringToQuarternion(string sVector)
    {
        // Remove the parentheses
        if (sVector.StartsWith("(") && sVector.EndsWith(")"))
        {
            sVector = sVector.Substring(1, sVector.Length - 2);
        }

        // split the items
        string[] sArray = sVector.Split(',');

        // store as a Vector3
        Quaternion result = new Quaternion(
            float.Parse(sArray[0]),
            float.Parse(sArray[1]),
            float.Parse(sArray[2]),
            float.Parse(sArray[3])
            );

        return result;
    }
}


public enum DataType { 
    STRING,INT,DOUBLE,FLOAT,VECTOR3,QUARTERNION
}
