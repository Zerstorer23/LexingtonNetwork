using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetPlayer 
{
    bool isLocal;
    object tagObject;
    public int actorID;
    public string NickName;
    public bool IsMasterClient;
    private Dictionary<PlayerProperty, string> CustomProperties;
    public static readonly string default_name = "ㅇㅇ";

 /*   public NetPlayer(bool isLocal, int actorID, string name) {
        this.isLocal = isLocal;
        this.actorID = actorID;
        this.NickName = name;
        CustomProperties = new Dictionary<PlayerProperty, string>();
    }
*/
    public NetPlayer(bool isLocal, LexNetworkMessage netMessage)
    {
        this.isLocal = isLocal;
        CustomProperties = new Dictionary<PlayerProperty, string>();
        this.actorID = Int32.Parse(netMessage.GetNext());
        this.IsMasterClient = netMessage.GetNext() == "1";
        int numParam = Int32.Parse(netMessage.GetNext());
        int i = 0;
        Debug.Log(string.Format("Received Player {0}, isMaster{1}", actorID, IsMasterClient));
        while (i < numParam) {
            PlayerProperty key =(PlayerProperty) Int32.Parse(netMessage.GetNext());
            string value = netMessage.GetNext();
            Debug.Log("Key " + key + " / " + value);
            CustomProperties.Add(key, value);
            i++;
        }
        UpdateName();
    }

    internal void UpdateName() {
        if (!CustomProperties.ContainsKey(PlayerProperty.NickName))
        {
            CustomProperties.Add(PlayerProperty.NickName, default_name);
        }
        NickName = CustomProperties[PlayerProperty.NickName];
    }
    //ActorID, isMaster, numParam ....key values
    public string EncodeToString() {
        LexNetworkMessage netMessage = new LexNetworkMessage(actorID, IsMasterClient, CustomProperties.Count);
        foreach (var entry in CustomProperties) {
            //   string cleanValue = entry.Value.Replace(LexNetworkConnection.NET_DELIM,)
            netMessage.Add(entry.Key);
            netMessage.Add(entry.Value);
        }
        return netMessage.Build();
    }

    public void SetCustomProperty(PlayerProperty key, string value) {
        if (!CustomProperties.ContainsKey(key))
        {
            CustomProperties.Add(key, value);
        }
        else
        {
            CustomProperties[key] = value;
        }
        LexNetwork.instance.SetPlayerCustomProperty_Send(actorID, (int)key, value);
    }
    public object GetCustomProperty(PlayerProperty key, string defaultValue)
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
public enum PlayerProperty { 
    NickName,Team
}
public enum RoomProperty
{
    GameMode
}