namespace Lex
{

    using Photon.Pun;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;


    public class LexHashTable
    {
        public Dictionary<int, object> lexHash = new Dictionary<int, object>();
        public ExitGames.Client.Photon.Hashtable phash;
        public object this[int i]
        {
            get { return (LexNetwork.useLexNet)?lexHash[i] : owner.pPlayer.CustomProperties[i.ToString()]; }
        }
        public object this[object i]
        {
            get { return (LexNetwork.useLexNet) ? lexHash[(int)i] : owner.pPlayer.CustomProperties[((int)i).ToString()]; }
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
        public LexHashTable(ExitGames.Client.Photon.Hashtable phash)
        {
            lexHash.Clear();
            foreach (var entry in phash)
            {
                lexHash.Add(Int32.Parse(entry.Key.ToString()), entry.Value);
            }
        }
        public ExitGames.Client.Photon.Hashtable ToPhotonHash()
        {
            var pHash = new ExitGames.Client.Photon.Hashtable();
            foreach (var entry in lexHash)
            {
                pHash.Add(((int)entry.Key).ToString(), entry.Value);
                Debug.Log(entry.Key);
            }
            return pHash;
        }


        public void Add(object key, object value) => Add((int)key, value);
        public void Add(int key, object value)
        {
            lexHash.Add(key, value);
        }

        public T Get<T>(object key, T value) => Get((int)key, value);
        public T Get<T>(int key, T value)
        {
            if (lexHash.ContainsKey(key))
            {
                return (T)lexHash[key];
            }
            return value;
        }

        public bool ContainsKey(int key) => lexHash.ContainsKey(key);
        public void UpdateProperties(LexHashTable hash)
        {
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
        public void UpdateProperties(int key, object value)
        {

            if (!lexHash.ContainsKey(key))
            {
                lexHash.Add(key, value);
            }
            else
            {
                lexHash[key] = value;
            }

        }
    }
}