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

    private void Update()
    {
        if(!isGrounded)
        {
            //isGrounded = TryPlace();
        }
    }

    public bool TryPlace()
    {
        // Cast a ray downward from the player position to find ground
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 10f);

        if (hit.collider != null && hit.collider.CompareTag("Ground"))
        {
            // Position the mine slightly above the ground point to prevent clipping
            transform.position = new Vector3(hit.point.x, hit.point.y + 0.05f, 0);
            return true;
        }

        return false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ground"))
        {
            rb.gravityScale = 0f;
            rb.velocity = Vector3.zero;
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
