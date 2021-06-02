using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class LexNetwork_SyncVar : MonoBehaviourLex
{
    protected bool isWriting = false;
    LexView lv;
    private void Awake()
    {
        lv = GetComponent<LexView>();
        isWriting = lv.IsMine;
    }
    public abstract void OnSyncView(params object[] parameters);


    public void PushSync( params object[] parameters) {
        
        LexNetwork.instance.SyncVar_Send(lv, parameters);
    }
    private void Update()
    {
        if (isWriting) {
            OnSyncView(null);
        }
    }

}
