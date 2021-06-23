namespace Lex
{
    
    using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public class LexRPC : Attribute
    {
    }

    public class MonoBehaviourLex : MonoBehaviour
    {
        //SyncVar, Callback 의 Parent
        //LexRPC 태그 찾는데 사용
        //MonoBehaviourPun, IConnectionCallbacks , IMatchmakingCallbacks , IInRoomCallbacks, ILobbyCallbacks, IWebRpcCallback, IErrorInfoCallback

        private LexView lvCache;
        public LexView lexView
        {
            get
            {
#if UNITY_EDITOR
                // In the editor we want to avoid caching this at design time, so changes in PV structure appear immediately.
                if (!Application.isPlaying || this.lvCache == null)
                {
                    this.lvCache = LexView.Get(this);
                }
#else
                if (this.lvCache == null)
                {
                    this.lvCache = GetComponent<LexView>();
                }
#endif
                return this.lvCache;
            }
        }
    }

}