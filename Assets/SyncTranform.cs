using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SyncTranform : LexNetwork_SyncVar
{
    Vector3 oldPos = Vector3.zero;

    public void Move(object[] position) {
        transform.position = (Vector3)position[0];
    }


    public override void OnSyncView(params object[] parameters)
    {
        if (isWriting)
        {
            if (transform.position == oldPos) return;
            oldPos = transform.position;
            PushSync(new DataType[] { DataType.VECTOR3 }, transform.position);
        }
        else {
            Vector3 pos = (Vector3)parameters[0];
            transform.position = pos;
        }
    }
}
