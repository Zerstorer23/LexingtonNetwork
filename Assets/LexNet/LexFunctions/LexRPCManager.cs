
namespace Lex
{
    using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

    public partial class LexNetwork
    {
        //TODO 캐시 순서, STRUCT의미
        public void RPC_Send(LexView lv, string functionName, params object[] parameters)
        {
            LexNetworkMessage netMessage = new LexNetworkMessage(LocalPlayer.actorID, (int)MessageInfo.RPC, lv.ViewID, functionName);
            netMessage.EncodeParameters(parameters);
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
        public void Run_RPC(LexView lv, string functionName, object[] parameters)
        {

            if (!lv.cachedRPCs.ContainsKey(functionName))
            {
                Debug.LogWarning(string.Format("No such function [{0}] found in view{1}", functionName, lv.ViewID));
                return;
            }
            lv.cachedRPCs[functionName].Invoke(parameters);
            return;
            /*       foreach (MonoBehaviour mono in lv.RpcMonoBehaviours)
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
                   }*/

        }


    }
    public struct RPC_Info
    {
        public MonoBehaviour monob;
        public MethodInfo mInfo;
        public RPC_Info(MonoBehaviour mono, MethodInfo mInfo)
        {
            this.monob = mono;
            this.mInfo = mInfo;
        }
        public object Invoke(object[] parameters)
        {
            return mInfo.Invoke((object)monob, parameters);
        }

    }
}