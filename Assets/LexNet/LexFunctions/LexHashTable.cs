using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LexHashTable
{
    public ExitGames.Client.Photon.Hashtable phash = new ExitGames.Client.Photon.Hashtable();
    public Dictionary<int, string> lexHash = new Dictionary<int, string>();
    public string this[int i]
    {
        get { return lexHash[i]; }
    }
    public string this[PlayerProperty i]
    {
        get { return lexHash[(int)i]; }
    }
    public string this[RoomProperty i]
    {
        get { return lexHash[(int)i]; }
    }
    LexPlayer owner = null;
    public LexHashTable()
    {
        owner = null;
    }
    public LexHashTable(LexPlayer owner)
    {
        this.owner = owner;
    }



    public void Add(object key, string value) => Add((int)key, value);
    public void Add(int key, string value)
    {
        if (!LexNetwork.useLexNet)
        {
            phash.Add(key, value);
        }
        else
        {
            lexHash.Add(key, value);
        }
    }

    public string Get(object key, string value) => Get((int)key, value);
    public string Get(int key, string value)
    {
        if (!LexNetwork.useLexNet)
        {
            if (phash.ContainsKey(key))
            {
                return (string)phash[key];
            }
        }
        else
        {
            if (lexHash.ContainsKey(key))
            {
                return (string)lexHash[key];
            }
        }
        // PushASetting(key, value.ToString());
        return value;
    }
   /* private void PushASetting(int key, string value)
    {
        LexHashTable hash = new LexHashTable();
        hash.Add(key, value);
        if (owner == null)
        {
            LexNetwork.SetRoomCustomProperties(hash);
        }
        else
        {
            owner.SetCustomProperties(hash);
        }
    }*/
    public bool ContainsKey(int key) => lexHash.ContainsKey(key);
    public void UpdateProperties(LexHashTable hash)
    {
        if (!LexNetwork.useLexNet)
        {
            if (owner == null)
            {
                PhotonNetwork.CurrentRoom.SetCustomProperties(hash.phash);
            }
            else
            {
                owner.pPlayer.SetCustomProperties(hash.phash);
            }
            return;
        }
        foreach (var entry in hash.lexHash)
        {
            if (!lexHash.ContainsKey(entry.Key))
            {
                lexHash.Add(entry.Key, entry.Value);
            }
            else
            {
                lexHash[entry.Key] = entry.Value;
            }
        }
    }

}
