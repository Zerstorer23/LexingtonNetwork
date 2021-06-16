using Photon.Realtime;
using System.Collections;
using UnityEngine;
using Photon.Pun;

public class Bullet : MonoBehaviourLex
{
    public bool goLeft = false;
    public float moveSpeed = 8;
    public float destroyTime = 2f;
    public float damage = 0.3f;
    IEnumerator destroyBullet()
    {
        yield return new WaitForSeconds(destroyTime);
        LexNetwork.Destroy(lexView);
    }
    private void OnEnable()
    {
        StartCoroutine(destroyBullet());
    }
    private void Update()
    {
        if (goLeft)
        {
            transform.Translate(Vector3.left * moveSpeed * Time.deltaTime);

        }
        else {

            transform.Translate(Vector3.right * moveSpeed * Time.deltaTime);
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!lexView.IsMine) {
            return;
        }

        LexView target = collision.gameObject.GetComponent<LexView>();
        if (target != null && (!target.IsMine || target.IsRoomView)) {

            if (target.tag == "Player") {
                target.RPC("HealthUpdate", RpcTarget.AllBuffered, damage);
                target.GetComponent<HurtEffect>().GotHit();

            }
            LexNetwork.Destroy(lexView);

        }

    }

    [LexRPC]
    public void ChangeDirection()
    {
        goLeft = true;
    }

}
