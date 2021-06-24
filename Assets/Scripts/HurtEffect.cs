using ExitGames.Client.Photon;
using Lex;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HurtEffect : MonoBehaviourLex
{
    public SpriteRenderer sprite;


    public void GotHit() {
        if (!lexView.IsMine) return;
        lexView.RPC("DoHit", RpcTarget.AllBuffered);
    }
    IEnumerator ChangeColorOverTime() {
        yield return new WaitForSeconds(0.2f);
        ChangeColor(1, 1, 1);
    }

    private void ChangeColor(float r, float g, float b)
    {
        sprite.color = new Color(r, g, b);
    }
    [LexRPC]
    public void DoHit()
    {
        ChangeColor(1, 0, 0);
        StartCoroutine(ChangeColorOverTime());
    }
}
