using ExitGames.Client.Photon;
using Lex;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HurtEffect : MonobehaviourLexSerialised
{
    public SpriteRenderer sprite;

    float r, g, b;
    bool flagChange = false;

    public void GotHit() {
        if (!lexView.IsMine) return;
        ChangeColor(1,0,0);
        StartCoroutine(ChangeColorOverTime());
    }
    IEnumerator ChangeColorOverTime() {
        yield return new WaitForSeconds(0.2f);
        ChangeColor(1, 1, 1);
    }

    private void ChangeColor(float r, float g, float b)
    {
 
        this.r = r;
        this.g = g;
        this.b = b;
        flagChange = true;

    }

    public override void OnSyncView(params object[] parameters)
    {
        if (isWriting)
        {
            if (!flagChange) return;
            PushSync(new object[] { r,g,b});
            sprite.color = new Color(r, g, b);
        }
        else
        {
            r = (float)parameters[0];
            g = (float)parameters[1];
            b = (float)parameters[2];
            flagChange = false;
            sprite.color = new Color(r, g, b);
        }
    }
}
