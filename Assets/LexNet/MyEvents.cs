﻿using UnityEngine.Events;
using UnityEngine;

public class NetEvent: UnityEvent<NetEventObject>
{


}
public class NetEventObject
{
    public LexCallback callbackID;
    public object objData;
    public bool boolObj;
    public int intObj;
    public float floatObj;
    public string stringObj;
    public NetEventObject(LexCallback id) {
        this.callbackID = id;
    }
    

}

