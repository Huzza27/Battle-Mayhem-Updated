using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Landmine : MonoBehaviourPun
{
    public ParticleSystem explosionParticles;
    public AudioSource source;
    public float damage;
    public PhotonView shooterView;
    public Rigidbody2D rb;
    private bool canBlowTheFuckUp = false;
    public MiscItemDependencies dependencies;
    [SerializeField] private float enableDelay = 1f;

    private void Start()
    {
        shooterView = dependencies.shooterView;
        Invoke("EnableBlowTheFuckUp", enableDelay);
    }

    void EnableBlowTheFuckUp()
    {
        canBlowTheFuckUp = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ground"))
        {
            rb.velocity = Vector3.zero;
            rb.gravityScale = 0;
        }

        if (!canBlowTheFuckUp)
        {
            return;
        }

        if (collision.CompareTag("Player") && photonView.IsMine)
        {
            PhotonView targetView = collision.transform.root.gameObject.GetComponent<PhotonView>();
            if (targetView != null)
            {
                // Call RPC to ensure explosion happens on all clients
                photonView.RPC("NetworkedBlowTheFuckUp", RpcTarget.All, targetView.ViewID);
            }
        }
    }

    [PunRPC]
    void NetworkedBlowTheFuckUp(int targetViewID)
    {
        PlayExplosionSFX();
        PhotonView targetView = PhotonView.Find(targetViewID);
        if (targetView != null)
        {
            PlayParticles();
            targetView.RPC("ReduceHealth", RpcTarget.All, damage, shooterView.Owner.ActorNumber);

            // Only the owner of the landmine should destroy it
            if (photonView.IsMine)
            {
                PhotonNetwork.Destroy(gameObject);
            }
        }
    }

    [PunRPC]
    private void PlayParticles()
    {
        var explosionInstance = PhotonNetwork.Instantiate(explosionParticles.name, transform.position, Quaternion.identity);
    }

    void PlayExplosionSFX()
    {
        source.PlayOneShot(source.clip);
    }

}