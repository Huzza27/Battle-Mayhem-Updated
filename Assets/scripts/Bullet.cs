using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Pun.Demo.Asteroids;
using Unity.VisualScripting;
using Smooth;

public class Bullet : MonoBehaviour
{
    public PhotonView view;

    public bool MoveDir = false;

    public float moveSpeed;

    public float destroyTime;

    SpriteRenderer spriteRenderer;

    public Sprite bullet;

    bool isBullet = false;

    [SerializeField] public Item gun;

    float kb;

    GameObject targetPlayer;

    private void Awake()
    {
        StartCoroutine("DestroyTimer");
        spriteRenderer = GetComponent<SpriteRenderer>();
        StartCoroutine("changeSprite");

    }

    IEnumerator DestroyTimer()
    {
        yield return new WaitForSeconds(destroyTime);
        GetComponent<PhotonView>().RPC("DestroyObject", RpcTarget.AllBuffered);
    }

    [PunRPC]
    public void changeDir_Left()
    {
        //Debug.Log("Changing Direction");
        spriteRenderer.flipX = true;
        MoveDir = true;
    }

    [PunRPC]
    public void DestroyObject()
    {
        Destroy(this.gameObject);
    }

    private void Update()
    {
        if (isBullet)
        {
            if (!MoveDir)
            {
                transform.Translate(Vector2.right * moveSpeed * Time.deltaTime);
            }
            else
            {
                transform.Translate(Vector2.left * moveSpeed * Time.deltaTime);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Ensure the bullet interaction only occurs for the owner of the bullet
        if (!view.IsMine)
            return;

        // Get the PhotonView of the target hit by the bullet
        PhotonView target = collision.gameObject.GetComponent<PhotonView>();

        if (target != null && (!target.IsMine || target.IsRoomView))
        {
            if (target.tag == "Player")
            {
                targetPlayer = collision.gameObject;
                int targetViewID = target.ViewID;

                // Calculate the knockback force based on direction, temporarily increasing it for visibility
                if (!MoveDir)
                {
                    kb = gun.GetHitKB() * 100;  // Increase force for testing
                }
                else
                {
                    kb = -gun.GetHitKB() * 100;
                }

                Debug.Log("Applying knockback force: " + kb);

                // Commented out the sync code for now as we're focusing on local knockback
                target.RPC("TakeKnockBackFromBullet",RpcTarget.All, kb, targetViewID);

                // Sync health reduction and hit marker (these should not be buffered for instant feedback)
                target.RPC("ReduceHealth", RpcTarget.All, gun.GetDamage());
                target.RPC("HitMarkerAnimation", RpcTarget.All);

                // Destroy the bullet (no need to buffer this, we want immediate action)
                this.GetComponent<PhotonView>().RPC("DestroyObject", RpcTarget.All);
            }
        }
    }



    IEnumerator changeSprite()
    {
        yield return new WaitForSeconds(0.02f);
        spriteRenderer.sprite = bullet;
        isBullet = true;
    }
}
