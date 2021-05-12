using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetPlayer 
{
    Dictionary<string, object> playerProperties = new Dictionary<string, object>();
    bool isLocal;
    object tagObject;

    public int actorID;
    public string NickName;
    public bool IsMasterClient;
    private Dictionary<string, object> CustomProperties;


    public NetPlayer(bool isLocal, int actorID, string name) {
        this.isLocal = isLocal;
        this.actorID = actorID;
        this.NickName = name;
        CustomProperties = new Dictionary<string, object>();
    }

    public void SetCustomProperty(string key, object value) {
        if (!CustomProperties.ContainsKey(key))
        {
            CustomProperties.Add(key, value);
        }
        else
        {
            CustomProperties[key] = value;
        }
        LexNetwork.instance.SetPlayerCustomProperty_Send(actorID, key, value);
    }
    public object GetCustomProperty(string key, object defaultValue)
    {
        if (!CustomProperties.ContainsKey(key))
        {
            CustomProperties.Add(key, defaultValue);
        }
        return CustomProperties[key];
    }

    public override bool Equals(object obj)
    {

        return true;
    }

    NetPlayer Get(int id) {
        return null;
    }
    NetPlayer GetNext() {
        return null;
    }
    NetPlayer GetNextFor(NetPlayer player) {
        return null;
    }

    static NetPlayer Find(int ID) {
        return null;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public override string ToString()
    {
        return base.ToString();
    }
}
