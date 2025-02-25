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

    private void Start()
    {
        shooterView = dependencies.shooterView;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        targetView = collision.gameObject.transform.root.GetComponent<PhotonView>();
        if (collision.tag == "Ground")
        {
            rb.simulated = false;
            isGrounded = true;
        }

        if (targetView == null || !isGrounded)
        {
            return;
        }
        if(collision.CompareTag("Player") && targetView != shooterView)
        {
            targetView = collision.transform.root.gameObject.GetComponent<PhotonView>();
            BlowTheFuckUp();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        
    }

    public void BlowTheFuckUp()
    {
        HitPlayer(damage);
    }

    void HitPlayer(float damage)
    {
        PlayParticles();
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
