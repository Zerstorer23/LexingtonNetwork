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
    static int BUFFER = 32* 1024;
    public readonly static string NET_DELIM = "#";
    public readonly static string NET_SIG = "LEX";

    Queue<string> receivedQueue = new Queue<string>();
    Queue<LexNetworkMessage> sendQueue = new Queue<LexNetworkMessage>();
    Thread listenThread;
    Thread sendThread;
    Socket mySocket;
    LexNetwork_CallbackHandler callbackHandler = new LexNetwork_CallbackHandler();


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
    {//ㅁㄴㅇㄹㄴ ㅎㅇㅀㅇㅀ
        //program1-
        //[20길]3#3#3#123123ㄹ3//20
        while (stayConnected)
        {
            //MUTEX
            while (sendQueue.Count > 0)
            {
                string message = MergeMessages();
                Debug.Log("Send message " + message);
                SendAMessage(message);
     //무한루프에 주의        
            }
            //MUTEX
        }
    }
    public void EnqueueAMessage(LexNetworkMessage netMessage)
    {
        //MUTEX
        sendQueue.Enqueue(netMessage);
        //MUTEX
    }
    string MergeMessages() {
        string message = sendQueue.Dequeue().Build();
        while (sendQueue.Count > 0)
        {
            string nextMessage = sendQueue.Peek().Build();
            //vector adfdas
            //
            if ((message.Length + nextMessage.Length+1) < BUFFER) {
                message += nextMessage;
                sendQueue.Dequeue();
            } 
        }
        return message;
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

            try
            {
                //서버 40
                // 40다
                //TODO 
                // 1 40 / 2 50 
                // c# socket buffer 늘리기
                mySocket.Receive(packet);
                //[----------------]
                //"333" <- 6
                //"22" <-4
            }
            catch (Exception e){
                Debug.LogWarning(e.Message);
                Debug.LogWarning(e.StackTrace);
                Debug.Log("Socket error");
                return;
            }
            MemoryStream ms2 = new MemoryStream(packet);
            BinaryReader br = new BinaryReader(ms2);
            str = br.ReadString();
            receivedQueue.Enqueue(str);
           // Debug.Log("size "+str.Length);
            Debug.Log(receivedQueue.Count+ "/ 수신한 메시지:" + str);
            br.Close();
            ms2.Close();

        }

    }
    //MainThread에서만 GameObject조작가능
    //그래서queue에 저장후  따로 Update에서 호출하도록함
    public void DequeueReceivedBuffer() {
        while (receivedQueue.Count > 0) {
            string message = receivedQueue.Dequeue();
            Debug.Log("Received " + message);
            HandleMessage(message);
        }
    }

     void HandleMessage(string str)
    {
        //  Debug.Log("Received message " + str);

        LexNetworkMessage netMessage = new LexNetworkMessage();
        netMessage.Split(str);
        while (netMessage.HasNext()) {
            Debug.Assert(netMessage.GetReceivedSize() >= 4, " Token information wrong");//TODO:: Queue이용하는 포맷으로 바꾸기
            string signature = netMessage.GetNext();
            bool isMyPacket = (signature == NET_SIG);
            if (!isMyPacket) continue;
            int lengthOfMessages = Int32.Parse(netMessage.GetNext());
            int sentActorNumber = Int32.Parse(netMessage.GetNext());
            MessageInfo messageInfo = (MessageInfo)Int32.Parse(netMessage.GetNext());
            Debug.Log(sentActorNumber + " message " + messageInfo);
            switch (messageInfo)
            {
                case MessageInfo.RPC:
                    ParseRPC(sentActorNumber, netMessage);
                    break;
                case MessageInfo.SyncVar:
                    ParseSyncVar(sentActorNumber, netMessage);
                    break;
                case MessageInfo.Chat:
                    ParseChat(sentActorNumber, netMessage);
                    break;
                case MessageInfo.Instantiate:
                    ParseInstantiate(sentActorNumber, netMessage);
                    break;
                case MessageInfo.Destroy:
                    ParseDestroy(sentActorNumber, netMessage);
                    break;
                case MessageInfo.SetHash:
                    ParseSetHash(sentActorNumber, netMessage);
                    break;
                case MessageInfo.ServerRequest:
                    break;
                case MessageInfo.ServerCallbacks:
                    callbackHandler.ParseCallback(sentActorNumber, netMessage);
                    break;
            }

        }
        
     
    }



    private void ParseSetHash(int sentActorNumber, LexNetworkMessage netMessage)
    {
        //actorNum, SetHash [int]roomOrPlayer [string]Key [int] DataType [object]value
        if (sentActorNumber == LexNetwork.LocalPlayer.actorID) return;
        int targetHashID = Int32.Parse(netMessage.GetNext()); //0 = Room,
        int key = Int32.Parse(netMessage.GetNext());
        string value = (string) ParseParameters(1, netMessage)[0];
        
        if (targetHashID == 0)
        {
            LexNetwork.instance.RoomProperty_Receive((RoomProperty)key, value);
        }
        else {
            LexNetwork.SetPlayerCustomProperties(targetHashID,(PlayerProperty) key, value);
        }
    }

    private void ParseDestroy(int sentActorNumber, LexNetworkMessage netMessage)
    {
        if (sentActorNumber == LexNetwork.LocalPlayer.actorID) return;
        int targetViewID = Int32.Parse(netMessage.GetNext());
        LexNetwork.instance.Destroy_Receive(targetViewID);

    }

    private void ParseInstantiate(int sentActorNumber, LexNetworkMessage netMessage)
    {
       if (sentActorNumber == LexNetwork.LocalPlayer.actorID) return;
        //actorNum, Instantiate [int]viewID [int]ownerID [string]prefabName [flaot,float,float] position [float,float,float]quarternion parentviewID [object[...]] params
        int targetViewID = Int32.Parse(netMessage.GetNext());
        int ownerID = Int32.Parse(netMessage.GetNext());
        string prefabName = netMessage.GetNext();
        Vector3 position = StringToVector3(netMessage.GetNext());
        Quaternion quaternion = StringToQuarternion(netMessage.GetNext());
        int parentID = Int32.Parse(netMessage.GetNext());
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
        int numParams = Int32.Parse(netMessage.GetNext());
        if (numParams > 0) {
            var param = ParseParameters(numParams, netMessage);
            childView.SetInstantiateData(param);
        }
        LexNetwork.AddViewtoDictionary(childView);
    }

    private void ParseChat(int sentActorNumber, LexNetworkMessage netMessage)
    {
        if (sentActorNumber == LexNetwork.LocalPlayer.actorID) return;
        //actorNum, Chat [string]chat message (needs cleansing)
        string message = netMessage.GetNext();//.Replace(NET_DELIM," ");
        LexChatManager.AddChat(message);
    }

    private void ParseSyncVar(int sentActorNumber, LexNetworkMessage netMessage)
    {
        //actorNum, SyncVar [int]viewID  [int]numparam ,[object[,,,]] params
        int targetViewID = Int32.Parse(netMessage.GetNext());
        int numParams = Int32.Parse(netMessage.GetNext());
        Debug.Assert(numParams != 0,"Syncing what?");
        var param = ParseParameters(numParams, netMessage);
        if (sentActorNumber == LexNetwork.LocalPlayer.actorID) return;
        LexNetwork.SyncVar_Receive(targetViewID, param);
    }

    //  actorNum, RPC[int] viewID[string] FunctionName[object[...]]params
    private void ParseRPC(int sentActorNumber, LexNetworkMessage netMessage)
    {
        int targetViewID = Int32.Parse(netMessage.GetNext());
        string functionName = netMessage.GetNext();
        int numParams = Int32.Parse(netMessage.GetNext());
        if (numParams == 0)
        {
            LexNetwork.RPC_Receive(targetViewID, functionName);
        }
        else
        {
            var param = ParseParameters(numParams, netMessage);
            LexNetwork.RPC_Receive(targetViewID, functionName, param);
        }
    }

    object[] ParseParameters(int numParams, LexNetworkMessage netMessage) {
        object[] param = new object[numParams];
        int index = 0;
        for (int i = 0; i < numParams; i++)
        {
            DataType dType = (DataType)Int32.Parse(netMessage.GetNext());
            string dataInfo = netMessage.GetNext();
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
