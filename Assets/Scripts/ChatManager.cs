using Lex;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChatManager :MonoBehaviour
{
    public Text chatText;
   [SerializeField] InputField chatInput;

    public void OnInputEnd() {
        LexNetwork.SendChat(chatInput.text);
        chatInput.text = "";
        
    }
 



}
