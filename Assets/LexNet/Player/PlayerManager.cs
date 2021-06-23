
namespace Lex
{
    using Photon.Pun;
    using Photon.Realtime;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    public partial class LexNetwork : MonobehaviourLexCallbacks
    {
        public static double NetTime { get; private set; }
        public static string NickName { get; private set; }
        public static int countOfPlayersInRoom;
        public static bool IsConnected { get; private set; }
        public static bool IsMasterClient { get; private set; }//TODO divide into photon
        public static LexPlayer[] PlayerList { get { return GetPlayerList(); } }
        public static LexPlayer[] PlayerListOthers { get { return GetPlayerListOthers(); } }
        public static int PlayerCount { get { return GetPlayerCount(); } }

        public static LexPlayer MasterClient { get; private set; }
        public static LexPlayer LocalPlayer{get; private set; }

        internal static void AddBotPlayer(LexPlayer botPlayer)
        {
            playerDictionary.Add(botPlayer.uid, botPlayer);
        }
        public static void RemoveBotPlayer(string uid)
        {
            if (playerDictionary.ContainsKey(uid))
            {
                playerDictionary.Remove(uid);
            }
        }

        public static int botIDnumber = 0;
        internal static string PollBotID()
        {
            botIDnumber++;
            return "T-" + PhotonNetwork.LocalPlayer.UserId + "-" + botIDnumber;
        }
        public static void ResetBotID() {
            botIDnumber = 0;
        }

        public static Dictionary<string, LexPlayer> GetPlayerDictionary()
        {
            return playerDictionary;
        }

        internal static int GetMyIndex(LexPlayer myPlayer, LexPlayer[] players, bool useRandom = false)
        {
            SortedSet<string> myList = new SortedSet<string>();
            foreach (LexPlayer p in players)
            {
                int seed = p.GetProperty(PlayerProperty.Seed, 0);
                string id = (useRandom) ? seed + p.uid : p.uid;
                myList.Add(id);
            }
            int i = 0;
            int mySeed = myPlayer.GetProperty(PlayerProperty.Seed, 0);
            string myID = (useRandom) ? mySeed + myPlayer.uid : myPlayer.uid;
            foreach (var val in myList)
            {
                if (val == myID) return i;
                i++;
            }
            return 0;
        }
        internal static SortedDictionary<string, int> GetIndexMap(LexPlayer[] players, bool useRandom = false)
        {
            instance.Init();
            SortedDictionary<string, string> decodeMap = new SortedDictionary<string, string>();
            foreach (LexPlayer p in players)
            {
                int seed = p.GetProperty(PlayerProperty.Seed, 0);
                string id = (useRandom) ? seed + p.uid : p.uid;
                decodeMap.Add(id, p.uid);
            }
            int i = 0;
            SortedDictionary<string, int> indexMap = new SortedDictionary<string, int>();
            foreach (var val in decodeMap)
            {
                indexMap.Add(val.Value, i++);
            }
            return indexMap;
        }

        internal static LexPlayer[] GetHumanPlayers()
        {
            var list = from LexPlayer p in playerDictionary.Values
                       where p.IsHuman
                       select p;
            return list.ToArray();
        }
        internal static LexPlayer[] GetBotPlayers()
        {
            var list = from LexPlayer p in playerDictionary.Values
                       where p.IsBot
                       select p;
            return list.ToArray();
        }
        public static void RemoveAllBots()
        {

            var list = (from LexPlayer p in playerDictionary.Values
                        where p.IsBot
                        select p.uid).ToArray();
            foreach (string s in list)
            {
                playerDictionary.Remove(s);
            }
        }
        internal static Player GetRandomPlayerExceptMe()
        {
            Player[] players = PhotonNetwork.PlayerListOthers;
            if (players.Length > 0)
            {
                return players[UnityEngine.Random.Range(0, players.Length)];

            }
            else
            {
                return null;
            }
        }

        public override void OnPlayerEnteredRoom(LexPlayer newPlayer)
        {
            if (!playerDictionary.ContainsKey(newPlayer.uid))
            {
                playerDictionary.Add(newPlayer.uid, newPlayer);
                Debug.Log("<color=#00ff00> Addplayer </color> " + playerDictionary.Count);
            }
        }
        public override void OnPlayerLeftRoom(LexPlayer newPlayer)
        {
            if (playerDictionary.ContainsKey(newPlayer.uid))
            {
                playerDictionary.Remove(newPlayer.uid);
                Debug.Log("<color=#00ff00> removePlayer </color> " + playerDictionary.Count);
            }
            // EventManager.TriggerEvent(MyEvents.EVENT_PLAYER_LEFT, new EventObject(newPlayer.UserId));
        }
        public override void OnJoinedRoom()
        {
            if (!useLexNet) {

                var playerList = PhotonNetwork.PlayerList;
                foreach (var p in playerList) {
                    LexPlayer lp = new LexPlayer(p);
                    playerDictionary.Add(lp.uid, lp);
                }
            }
        }
        public static LexPlayer GetPlayerByID(string id)
        {
            instance.Init();
            if (id == null) return null;
            if (playerDictionary.ContainsKey(id))
            {
                return playerDictionary[id];
            }
            else
            {
                Debug.LogWarning("Couldnt find " + id + " size " + playerDictionary.Count);
                return null;
            }
        }

   

    }
}