namespace Lex
{
    using Photon.Pun;
    using Photon.Realtime;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    [Serializable]
    public class LexPlayer
    {
        public static readonly string[] botNames = {
        "Langley","Saratoga","Lexington","Hornet","Ranger",
        "Yorktown","Enterprise","Wasp","Essex","Intrepid",
    "Franklin","Independence","Princeton","Bunker Hill",
    "Bataan","Kearsarge","Shangri-La","Midway","Saipan"};
        public static readonly string default_name = "ㅇㅇ";

        [SerializeField] [ReadOnly] string myNickName = default_name;
        object tagObject;
        public int actorID;
        string botID;
        public Player pPlayer;


        public bool IsMasterClient { get; internal set; }

        LexHashTable prHash;
        public LexHashTable CustomProperties { get; private set; }
        //  public SerializeDictionary<string,string> dict = new SerializeDictionary<string, string>();

        //--Universal
        ControllerType controllerType = ControllerType.Human;
        public string uid {
            get {
                if (IsHuman)
                {
                    if (LexNetwork.useLexNet)
                    {
                        return actorID.ToString();
                    }
                    else
                    {
                        return pPlayer.UserId;
                    }
                }
                else {
                    return botID;
                }  
            }
        }


        public bool IsBot
        {
            get => controllerType == ControllerType.Bot;
        }
        public bool IsHuman
        {
            get => controllerType == ControllerType.Human;
        }
        public string NickName
        {
            get => GetNickName();
            set => SetNickName(value);
        }
        private bool isLocalPlayer;
        public bool IsLocal
        {
            get => GetIsLocal();
            private set => isLocalPlayer = value;
        }

        private bool GetIsLocal()
        {
            if (LexNetwork.useLexNet)
            {
                return IsHuman && isLocalPlayer;
            }
            else
            {
                return IsHuman && pPlayer.IsLocal;
            }
        }

        public bool AmController
        {
            get
            {
                if (IsHuman)
                {
                    if (LexNetwork.useLexNet)
                    {
                        return isLocalPlayer;
                    }
                    else
                    {
                        return pPlayer.IsLocal;
                    }
                }
                else
                {
                    return LexNetwork.IsMasterClient;
                }
            }
        }
        private void SetNickName(string value)
        {
            if (IsHuman)
            {
                if (LexNetwork.useLexNet)
                {
                    myNickName = value;
                    LexHashTable lexHash = new LexHashTable();
                    lexHash.Add(PlayerProperty.NickName, myNickName);
                    SetCustomProperties(lexHash);
                }
                else
                {
                    pPlayer.NickName = value;
                }
            }
            else
            {
                ReceiveBotProperty((int)PlayerProperty.NickName, value);
            }
        }

        private string GetNickName()
        {
            if (LexNetwork.useLexNet)
            {
                return CustomProperties.Get(PlayerProperty.NickName, myNickName);
            }
            else
            {
                return pPlayer.NickName;
            }
        }



        public T GetProperty<T>(object key) => GetProperty<T>((int)key);
        public T GetProperty<T>(object key, T value) => GetProperty((int)key,value);
        public T GetProperty<T>(int key)
        {
            return (T)CustomProperties[key];
        }
        public T GetProperty<T>(int key, T value)
        {
            if (HasProperty(key))
            {
                return (T)CustomProperties[key];
            }
            return value;
        }

        public bool HasProperty(int key)
        {
            return CustomProperties.ContainsKey(key);
        }
        public void SetCustomProperties(LexHashTable lexHash)
        {
            Debug.Log("Update hash " + lexHash.lexHash.Count);
            CustomProperties.UpdateProperties(lexHash);
            if (IsHuman)
            {
                if (LexNetwork.useLexNet)
                {
                    LexNetwork.instance.CustomProperty_Send(actorID, lexHash);
                }
                else
                {
                    Debug.LogWarning(pPlayer);
                    pPlayer.SetCustomProperties(lexHash.ToPhotonHash());
                }
            }
            else
            {
                if (LexNetwork.IsMasterClient)
                {
                    foreach (var entry in lexHash.lexHash)
                    {
#if USE_LEX
                        LexNetwork.instance.lexView.RPC("SetBotProperty",RpcTarget.AllBuffered,uid,entry.Key,entry.GetType().Name,entry.Value);
#else

                        LexNetwork.instance.GetComponent<PhotonView>().RPC("SetBotProperty", RpcTarget.AllBuffered, uid, entry.Key, entry.GetType().Name, entry.Value);
#endif
                    }
                }
            }
        }
        internal void ReceiveBotProperty(int tag, object value)
        {
            Debug.Assert(IsBot, "Not a bot ??");
            CustomProperties.UpdateProperties(tag, value);
        }
        public override string ToString()
        {
             return uid;
        }


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
            controllerType = ControllerType.Human;
            Debug.Log(string.Format("Received Player {0}, isMaster{1}", actorID, IsMasterClient));
            for (int i = 0; i < numHash; i++)
            {
                int key = Int32.Parse(netMessage.GetNext());
                string value = netMessage.GetNext();
                Debug.Log("Key " + (PlayerProperty)key + " / " + value);
                CustomProperties.Add(key, value);
            }
        }
        public LexPlayer(Player player)
        {
            controllerType = ControllerType.Human;
            this.pPlayer = player;
            Debug.LogWarning("PLauyer " + player);
            controllerType = ControllerType.Human;
            this.IsMasterClient = player.IsMasterClient;
            this.IsLocal = player.IsLocal;
            CustomProperties = new LexHashTable(this);
        }
        public LexPlayer(string uid)
        {
            controllerType = ControllerType.Bot;
            this.botID = uid;
            NickName = botNames[UnityEngine.Random.Range(0, botNames.Length)];
            CustomProperties = new LexHashTable(this);
        }

        public LexPlayer Next() {
            var players = LexNetwork.PlayerList;
            int i = 0;
            while (i < players.Length && players[i].uid != uid) {
                i++;
            }
            int index = (i + 1) % players.Length;
            return players[index];
        }



    }
    public enum PlayerProperty
    {
        NickName, Team,Seed
    }
    public enum RoomProperty
    {
        GameMode,Seed
    }

}