using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;
using Cinemachine;
using UnityEngine.UI;

public class CowBoy : MonobehaviourLexSerialised
{
    public float moveSpeed = 5f;
    public SpriteRenderer sprite;
    public  Animator animator;

    public bool canMove = true;
    public GameObject BulletPrefab;
    public Transform[] gunPosition;
    public Text playerNameText;

    public bool AllowInputs = true;
    public bool isGrounded = false;

    Rigidbody2D rigidBody;
    public float jumpForce;

    void Start()
    {//TODO 원래 AWAKE였음
        Debug.Log("Cowboy awake");
        if (lexView.IsMine)
        {
            var CM = GameObject.Find("CMvcam").GetComponent<CinemachineVirtualCamera>();
            CM.Follow = transform;
            //  CM.LookAt = transform;

            playerNameText.text = LexNetwork.NickName;
            playerNameText.text+= LexNetwork.LocalPlayer.CustomProperties[PlayerProperty.Team];
            ;
            playerNameText.color = Color.green;
            GameManager.instance.LocalPlayer = this.gameObject;
        }
        else {
            playerNameText.text = lexView.Owner.NickName+ lexView.Owner.CustomProperties[PlayerProperty.Team];
                ;
            playerNameText.color = Color.red;
        }
        rigidBody = GetComponent<Rigidbody2D>();


        Debug.Log("Cowboy awake finish");
    }

    // Update is called once per frame
    private void Update()
    {
        if (lexView.IsMine && AllowInputs) {
            CheckInputs();
        }
        WriteSync();
    }

    private void CheckInputs()
    {


        if (Input.GetKeyDown(KeyCode.LeftControl)
            && !animator.GetBool("IsMove"))
        {
            Shoot(true);

        }
        else if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            Shoot(false);
        }

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded) {
            Jump();
        }

        if (canMove)
        {
            CheckMove();
        }
    }

    private void CheckMove()
    {
        var movement = new Vector3(Input.GetAxisRaw("Horizontal"), 0);
        transform.position += movement * moveSpeed * Time.deltaTime;
        if (Input.GetKeyDown(KeyCode.D))
        {
            lexView.RPC("FlipSprite", RpcTarget.AllBuffered, false);
            animator.SetBool("IsMove", true);
        }
        else if (Input.GetKeyUp(KeyCode.D))
        {
            animator.SetBool("IsMove", false);
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            lexView.RPC("FlipSprite", RpcTarget.AllBuffered, true);
            animator.SetBool("IsMove", true);
        }
        if (Input.GetKeyUp(KeyCode.A))
        {
            animator.SetBool("IsMove", false);
        }
    }

    [LexRPC]
    void FlipSprite(bool enable) {
        sprite.flipX = enable;
    }


    void Shoot(bool enable) {
        animator.SetBool("IsShot", enable);
        canMove = !enable;
        Debug.Log("Shoot " + enable);
        if (enable) {
            var gunPos = (sprite.flipX) ? gunPosition[0] : gunPosition[1];
            GameObject bullet=  LexNetwork.Instantiate(BulletPrefab.name,
                gunPos.position,
                Quaternion.identity
                ,0 );
            if (sprite.flipX)
            {
                bullet.GetComponent<LexView>().RPC("ChangeDirection", RpcTarget.AllBuffered);

            }
        
        }

    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Ground")
        {
            isGrounded = true;
        }
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Ground")
        {
            isGrounded = false;

        }
    }


    void Jump() {
        rigidBody.AddForce(new Vector2(0, jumpForce * Time.deltaTime));
    
    }
    Vector3 oldPos;
    public override void OnSyncView(params object[] parameters)
    {
        if (isWriting)
        {
            if (transform.position == oldPos) return;
            Debug.Log("Sync send " + oldPos);
            PushSync(transform.position);
            oldPos = transform.position;
        }
        else
        {
            Vector3 pos = (Vector3)parameters[0];
            transform.position = pos;
        }
    }
}
