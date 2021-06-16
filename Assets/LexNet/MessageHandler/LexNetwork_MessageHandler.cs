using System;
using UnityEngine;

public class LexNetwork_MessageHandler
{
    public readonly static string NET_DELIM = "#";
    public readonly static string NET_SIG = "LEX";
    LexNetwork_CallbackHandler callbackHandler = new LexNetwork_CallbackHandler();

    public void HandleMessage(string str)
    {
        Debug.Log("Received message " + str);
        LexNetworkMessage netMessage = new LexNetworkMessage();
        netMessage.Split(str);
        while (netMessage.HasNext())
        {

            string signature = netMessage.GetNext();
            bool isMyPacket = (signature == NET_SIG);
            if (!isMyPacket) continue;
            try
            {
                Debug.LogWarning("처리중: " + netMessage.Peek());
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
            catch (Exception e) {
                Debug.LogWarning("Handle message fatal error");
                Debug.LogWarning(e.Message);
                Debug.LogWarning(e.StackTrace);
            }
          

        }
        

    }



    private void ParseSetHash(int sentActorNumber, LexNetworkMessage netMessage)
    {
        //actorNum, SetHash [int]roomOrPlayer [int]entryCount [int]Key [string] value
        if (sentActorNumber == LexNetwork.LocalPlayer.actorID) return;
        int targetHashID = Int32.Parse(netMessage.GetNext()); //0 = Room,
        int numHash = Int32.Parse(netMessage.GetNext());
        LexHashTable hash = new LexHashTable();
        for (int i = 0; i < numHash; i++)
        {
            int key = Int32.Parse(netMessage.GetNext());
            string value = netMessage.GetNext();
            hash.Add(key, value);
        }

        if (targetHashID == 0)
        {
            LexNetwork.CustomProperties.UpdateProperties(hash);
        }
        else
        {
            LexNetwork.GetPlayerByID(targetHashID).CustomProperties.UpdateProperties(hash);
        }
        NetworkEventManager.TriggerEvent(LexCallback.HashChanged, new NetEventObject() { intObj = targetHashID, objData = hash });
    }

    private void ParseDestroy(int sentActorNumber, LexNetworkMessage netMessage)
    {
        if (sentActorNumber == LexNetwork.LocalPlayer.actorID) return;
        int targetViewID = Int32.Parse(netMessage.GetNext());
        LexView lv = LexViewManager.GetViewByID(targetViewID);
        LexViewManager.ReleaseViewID(lv);

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
        //TODO
        /*
         INSTANTIATE =COWBOY AWAKE CALL
         SET INFO -> LEXVIEW AWAKE CALL
         
         */
        GameObject go = GameObject.Instantiate((GameObject)Resources.Load(prefabName), position, quaternion);
        LexView lv = go.GetComponent<LexView>();
        lv.SetInformation(targetViewID, prefabName, ownerID, sentActorNumber, false);
        //Params
        int numParams = Int32.Parse(netMessage.GetNext());
        if (numParams > 0)
        {
            var param = ParseParametersByString(numParams, netMessage);
            lv.SetInstantiateData(param);
        }
        Debug.Log("Instantiate finished " + netMessage.GetReceivedSize());
    }

    private void ParseChat(int sentActorNumber, LexNetworkMessage netMessage)
    {
        if (sentActorNumber == LexNetwork.LocalPlayer.actorID) return;
        //actorNum, Chat [string]chat message (needs cleansing)
        string message = netMessage.GetNext();
        LexChatManager.AddChat(message);
    }

    private void ParseSyncVar(int sentActorNumber, LexNetworkMessage netMessage)
    {
        //actorNum, SyncVar [int]viewID  [int]numparam ,[object[,,,]] params
        int targetViewID = Int32.Parse(netMessage.GetNext());
        int numParams = Int32.Parse(netMessage.GetNext());
        Debug.Assert(numParams != 0, "Syncing what?");
        var param = ParseParametersByString(numParams, netMessage);
        if (sentActorNumber == LexNetwork.LocalPlayer.actorID) return;
        LexNetwork.instance.SyncVar_Receive(targetViewID, param);
    }

    //  actorNum, RPC[int] viewID[string] FunctionName[object[...]]params
    private void ParseRPC(int sentActorNumber, LexNetworkMessage netMessage)
    {
        int targetViewID = Int32.Parse(netMessage.GetNext());
        string functionName = netMessage.GetNext();
        int numParams = Int32.Parse(netMessage.GetNext());
        if (numParams == 0)
        {
            LexNetwork.instance.RPC_Receive(targetViewID, functionName);
        }
        else
        {
            // var param = ParseParameters(numParams, netMessage);
            var param = ParseParametersByString(numParams, netMessage);
            LexNetwork.instance.RPC_Receive(targetViewID, functionName, param);
        }
    }
    object[] ParseParametersByString(int numParams, LexNetworkMessage netMessage)
    {

        object[] param = new object[numParams];
        for (int i = 0; i < numParams; i++)
        {
            string typeName = netMessage.GetNext();
            Debug.Log(typeName);
            string dataInfo = netMessage.GetNext();
            switch (typeName)
            {
                case nameof(Int32):
                    param[i] = int.Parse(dataInfo);
                    break;
                case nameof(String):
                    param[i] = dataInfo;
                    break;
                case nameof(Double):
                    param[i] = double.Parse(dataInfo);
                    break;
                case nameof(Vector3):
                    param[i] = StringToVector3(dataInfo);
                    break;
                case nameof(Quaternion):
                    param[i] = StringToQuarternion(dataInfo);
                    break;
                case nameof(Single):
                    param[i] = float.Parse(dataInfo);
                    break;
            }
        }
        return param;
    }
    /*    object[] ParseParameters(int numParams, LexNetworkMessage netMessage)
        {
            object[] param = new object[numParams];
            for (int i = 0; i < numParams; i++)
            {
                DataType dType = (DataType)Int32.Parse(netMessage.GetNext());
                string dataInfo = netMessage.GetNext();
                switch (dType)
                {
                    case DataType.STRING:
                        param[i] = dataInfo;
                        break;
                    case DataType.INT:
                        param[i] = Int32.Parse(dataInfo);
                        break;
                    case DataType.DOUBLE:
                        param[i] = Double.Parse(dataInfo);
                        break;
                    case DataType.FLOAT:
                        param[i] = float.Parse(dataInfo);
                        break;
                    case DataType.VECTOR3:
                        param[i] = StringToVector3(dataInfo);
                        break;
                }
            }
            return param;
        }*/



    private Vector3 StringToVector3(string sVector)
    {

        int start = sVector.IndexOf('(') + 1;
        int end = sVector.IndexOf(')');
        sVector = sVector.Substring(start, end - start);

        // split the items
        string[] sArray = sVector.Split(',');
        Vector3 result = new Vector3(
            float.Parse(sArray[0]),
            float.Parse(sArray[1]),
            float.Parse(sArray[2]));

        return result;
    }
    private Quaternion StringToQuarternion(string sVector)
    {
        int start = sVector.IndexOf('(') + 1;
        int end = sVector.IndexOf(')');
        sVector = sVector.Substring(start, end - start);
        Debug.Log("Parenthe removes " + sVector);

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
