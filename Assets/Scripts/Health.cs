using Lex;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviourLex
{
    public Image fillImage;
    public float health = 1;

    public CowBoy player;
    private void Awake()
    {
        player = GetComponent<CowBoy>();
    }
    public void CheckHealth() {
        if (lexView.IsMine && health <= 0) {
            this.GetComponent<LexView>().RPC("DoDeath", RpcTarget.AllBuffered);
        }
    }
    [LexRPC]
    internal void internalFunction()
    {
    }

    [LexRPC]
    public static void publicStaticFunction()
    {
    }
    [LexRPC]
    private void PrivateFunction() { 
    
    }

    [LexRPC]
    public void DoDeath() {
        gameObject.SetActive(false);
        EnableInput(false);
        GameManager.instance.EnableRespawn();
    }

    [LexRPC]
    public void Resurrect()
    {
        gameObject.SetActive(true);
        health = 1;
        fillImage.fillAmount = health;
        EnableInput(true);
    }

    public void EnableInput(bool enable)
    {
        player.AllowInputs = enable;

    }


    [LexRPC]
    public void HealthUpdate(float damage) {
        fillImage.fillAmount -= damage;
        health -= damage;
        CheckHealth();
    }

}
