﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using System;

public class MenuManager : MonoBehaviourPunCallbacks
{
    [SerializeField] InputField userNameInput;

    #region UIMethods

    public void OnNameField_Changed() {
        LexNetwork.LocalPlayer.NickName = userNameInput.text;
        userNameInput.text = "";
    }
   
    #endregion






}