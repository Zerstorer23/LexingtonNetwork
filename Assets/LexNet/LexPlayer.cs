using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LexPlayer 
{
    public Player pPlayer;
    bool isLocal;
    object tagObject;
    public int actorID;
    public string NickName {
        get { return NickName; }
        set => SetNickName(value);
       }


    public bool IsMasterClient;
    public LexHashTable CustomProperties { get; private set; }
    public static readonly string default_name = "ㅇㅇ";

 /*   public NetPlayer(bool isLocal, int actorID, string name) {
        this.isLocal = isLocal;
        this.actorID = actorID;
        this.NickName = name;
        CustomProperties = new Dictionary<PlayerProperty, string>();
    }
*/
    public LexPlayer(bool isLocal, LexNetworkMessage netMessage)
    {
        this.isLocal = isLocal;
        CustomProperties = new LexHashTable(this);
        this.actorID = Int32.Parse(netMessage.GetNext());
        this.IsMasterClient = netMessage.GetNext() == "1";
        int numHash = Int32.Parse(netMessage.GetNext());
        Debug.Log(string.Format("Received Player {0}, isMaster{1}", actorID, IsMasterClient));
        for (int i = 0; i < numHash; i++)
        {
            int key =Int32.Parse(netMessage.GetNext());
            string value = netMessage.GetNext();
            Debug.Log("Key " + (PlayerProperty)key + " / " + value);
            CustomProperties.Add(key, value);
        }
    }
    private void SetNickName(string name)
    {
        LexHashTable lexHash = new LexHashTable();
        lexHash.Add(PlayerProperty.NickName, name);
        SetCustomProperty(lexHash);
    }

    //ActorID, isMaster, numParam ....key values
/*    public string EncodeToString() {
        LexNetworkMessage netMessage = new LexNetworkMessage(actorID, IsMasterClient, CustomProperties.lexHash.Count);
        foreach (var entry in CustomProperties.lexHash) {
            //   string cleanValue = entry.Value.Replace(LexNetworkConnection.NET_DELIM,)
            netMessage.Add(entry.Key);
            netMessage.Add(entry.Value);
        }
        return netMessage.Build();
    }*/

    public void SetCustomProperty(LexHashTable lexHash) {
        Debug.Log("Update hash " + lexHash.lexHash.Count);
        CustomProperties.UpdateProperties(lexHash);
        LexNetwork.instance.CustomProperty_Send(actorID, lexHash);
    }


    public override bool Equals(object obj)
    {

        return true;
    }

    LexPlayer Get(int id) {
        return null;
    }
    LexPlayer GetNext() {
        return null;
    }
    LexPlayer GetNextFor(LexPlayer player) {
        return null;
    }

    static LexPlayer Find(int ID) {
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