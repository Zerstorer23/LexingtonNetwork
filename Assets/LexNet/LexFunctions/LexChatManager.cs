
namespace Lex
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class LexChatManager : MonoBehaviour
    {
        static List<string> chatQueue = new List<string>();
        private static LexChatManager prChatManager;

        public static LexChatManager instance
        {
            get
            {
                if (!prChatManager)
                {
                    prChatManager = FindObjectOfType<LexChatManager>();
                    if (!prChatManager)
                    {
                        Debug.LogWarning("There needs to be one active LexChatManager script on a GameObject in your scene.");
                    }
                    else
                    {

                    }
                }
                return prChatManager;
            }
        }
        public static void AddChat(string msg)
        {
            chatQueue.Add(msg);
        }
    }
}