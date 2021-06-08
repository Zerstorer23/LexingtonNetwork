using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public partial class LexNetwork
{
    
    private static Dictionary<int, Dictionary<string,RPC_Info>> lv_rpc = new Dictionary<int, Dictionary<string,RPC_Info>>();
    public void RPC_Send(LexView lv, string functionName, params object[] parameters)
    {
        LexNetworkMessage netMessage = new LexNetworkMessage(LocalPlayer.actorID, (int)MessageInfo.RPC, lv.ViewID, functionName);
        netMessage.EncodeParameters( parameters);
        Run_RPC(lv, functionName, parameters);
        networkConnector.EnqueueAMessage(netMessage);
    }
/*    public void RPC_Send(LexView lv, string functionName, DataType[] dataTypes = null, params object[] parameters)
    {
        LexNetworkMessage netMessage = new LexNetworkMessage(LocalPlayer.actorID, (int)MessageInfo.RPC, lv.ViewID, functionName);
        netMessage.EncodeParameters(dataTypes, parameters);
        Run_RPC(lv,functionName, parameters);
        networkConnector.EnqueueAMessage(netMessage);
    }*/
    public void RPC_Receive(int viewID, string functionName, params object[] parameters)
    {
        if (debugLexNet)
        {
            foreach (object obj in parameters) Debug.Log(obj);
        }

        LexView lv = LexViewManager.GetViewByID(viewID);
        if (!lv) return;
        Run_RPC(lv, functionName, parameters);
    }
    public void Run_RPC(LexView lv, string functionName,object[] parameters)
    {
        if (!lv_rpc.ContainsKey(lv.ViewID)){
            var lv_functions = new Dictionary<string, RPC_Info>();
            UpdateRPC(lv, lv_functions);
            lv_rpc.Add(lv.ViewID, lv_functions);
        }
        var functionsInLV = lv_rpc[lv.ViewID];
        if (!functionsInLV.ContainsKey(functionName)) {
            UpdateRPC(lv, functionsInLV);
        }
        if (!functionsInLV.ContainsKey(functionName))
        {
            Debug.LogWarning(string.Format("No such function [{0}] found in view{1}", functionName, lv.ViewID));
            return;
        }
        functionsInLV[functionName].Invoke(parameters);

        return;
        foreach (MonoBehaviour mono in lv.RpcMonoBehaviours)
        {
            //캐시 필요
            Type type = mono.GetType();
            Debug.Log(type);
            MethodInfo mInfo = type.GetMethod(functionName);
            if (mInfo != null) {
                Debug.Log(mInfo);
                // object o = mInfo.Invoke((object)monob, new object[] { new PhotonMessageInfo(sender, sendTime, photonNetview) });
                mInfo.Invoke((object)mono, parameters);
                break;
            }
        }

    }
    void UpdateRPC(LexView lv, Dictionary<string, RPC_Info> lv_functions) {
        foreach (MonoBehaviour mono in lv.RpcMonoBehaviours)
        {
            Type type = mono.GetType();
            Debug.Log(type);
            var functions = type.GetMethods();
            foreach (var function in functions)
            {
                if (function.GetCustomAttribute(typeof(LexRPC)) == null) continue;
                RPC_Info rpcInfo = new RPC_Info(mono, function);
                lv_functions.Add(function.Name, rpcInfo);
            }
        }
    }

}
public struct RPC_Info {
    public MonoBehaviour monob;
    public MethodInfo mInfo;
    public RPC_Info(MonoBehaviour mono, MethodInfo mInfo){
        this.monob = mono;
        this.mInfo = mInfo;
    }
   public object Invoke(object[] parameters) {
        return mInfo.Invoke((object)monob, parameters);
    }

}