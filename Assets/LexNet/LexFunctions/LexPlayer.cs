using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LexPlayer
{
    [SerializeField] [ReadOnly] string myNickName = default_name;
    public  bool IsLocal { get; private set; }
    object tagObject;
    public int actorID;
    public Player pPlayer;
    public string NickName {
        get => GetNickName();
        set => SetNickName(value);
       }

    public  bool IsMasterClient;
    public  LexHashTable CustomProperties { get; private set; }
  //  public SerializeDictionary<string,string> dict = new SerializeDictionary<string, string>();
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
        this.IsLocal = isLocal;
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
        myNickName = name;
        LexHashTable lexHash = new LexHashTable();
        lexHash.Add(PlayerProperty.NickName, myNickName);
        SetCustomProperties(lexHash);
    }
    private string GetNickName() {
        myNickName = CustomProperties.Get(PlayerProperty.NickName, myNickName);
        return myNickName;
    }


    public void SetCustomProperties(LexHashTable lexHash) {
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