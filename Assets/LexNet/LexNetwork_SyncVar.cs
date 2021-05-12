using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class LexNetwork_SyncVar : MonoBehaviour
{
    protected bool isWriting = false;
    LexView lv;
    private void Awake()
    {
        lv = GetComponent<LexView>();
        isWriting = lv.IsMine;
    }
    public abstract void OnSyncView(params object[] parameters);


    public void PushSync(DataType[] dataTypes, params object[] parameters) {
        LexNetwork.SyncVar_Send(lv, dataTypes, parameters);
    }
    private void Update()
    {
        if (isWriting) {
            OnSyncView(null);
        }
    }

}
