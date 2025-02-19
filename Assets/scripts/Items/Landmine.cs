using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Landmine : MonoBehaviour
{

    public ParticleSystem explosionParticles;
    public float damage;
    PhotonView targetView;
    public PhotonView shooterView;
    public Rigidbody2D rb;
    bool isGrounded = false;
    public MiscItemDependencies dependencies;
    public float placementDistance;

    private void Start()
    {
        shooterView = dependencies.shooterView;
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ground"))
        {
            rb.velocity = Vector3.zero;
            rb.gravityScale = 0f;
            isGrounded = true;
        }
        if (!isGrounded)
        {
            return;
        }

        if (collision.CompareTag("Player"))
        {
            targetView = collision.GetComponent<PhotonView>();
            if (targetView == null)
            {
                targetView = collision.gameObject.transform.parent.GetComponent<PhotonView>();
            }
                BlowTheFuckUp();
        }
    }



    public void BlowTheFuckUp()
    {
        HitPlayer(damage);
    }

    void HitPlayer(float damage)
    {
        PlayParticles();
        shooterView.RPC("PlayExplosionSound", RpcTarget.All);
        targetView.RPC("ReduceHealth", RpcTarget.All, damage);
        DestroyMine();
    }

    private void DestroyMine()
    {
        PhotonNetwork.Destroy(gameObject);
    }

    [PunRPC]
    private void PlayParticles()
    {
        targetView.RPC("PlayExplosionSound", RpcTarget.All);
        var explosionInstance = PhotonNetwork.Instantiate(explosionParticles.name, transform.position, Quaternion.identity);
    }
}
