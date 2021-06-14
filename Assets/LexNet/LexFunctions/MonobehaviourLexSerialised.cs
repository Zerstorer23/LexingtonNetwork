﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MonobehaviourLexSerialised : MonoBehaviourLex
{
    protected bool isWriting = false;
    public void UpdateOwnership()
    {
        isWriting = lexView.IsMine;
        Debug.LogWarning("Update ownership " + isWriting);
    }
    //실행순서를 어떻게 약속시켜야하는지. start에 밀어도 safe인지
    //event로 관리할수도 있을거같음
    //todo lv init이 끝나야됨.
    public abstract void OnSyncView(params object[] parameters);
    protected void WriteSync() {
        if (isWriting)
        {
            OnSyncView(null);
        }
    }

    public void PushSync( params object[] parameters) {
        LexNetwork.instance.SyncVar_Send(lexView, parameters);
    }
/*    private void Update()
    {
        Debug.LogWarning("Update sync " + isWriting);//TODO 왜 false?
        if (isWriting) {
            OnSyncView(null);
        }
    }*/

}