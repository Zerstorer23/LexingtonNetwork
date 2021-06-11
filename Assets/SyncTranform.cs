using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SyncTranform : MonobehaviourLexSerialised
{
    Vector3 oldPos = Vector3.zero;
    
    [LexRPC]
    public void Move(Vector3 position, string message) {
        transform.position = position;
        Debug.Log(message);
    }
    private void Start()
    {
        oldPos = transform.position;
    }

    public override void OnSyncView(params object[] parameters)
    {
        if (isWriting)
        {
            if (transform.position == oldPos) return;
            oldPos = transform.position;
            PushSync(transform.position);
        }
        else {
            Vector3 pos = (Vector3)parameters[0];
            transform.position = pos;
        }
    }
}
