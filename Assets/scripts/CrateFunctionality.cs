using UnityEngine;
using Photon.Pun;
using ExitGames.Client.Photon;
using UnityEngine.EventSystems;
using UnityEngine;
using Photon.Pun;
using System;
using System.Collections;

public class CrateFunctionality : MonoBehaviourPunCallbacks
{
    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private float lifeSpan;
    [SerializeField] private ParticleSystem destroyParticle;
    [SerializeField] private ItemDatabase itemDatabase;

    public Action OnCrateDestroyed;

    private void Start()
    {
        StartCoroutine(HandleCrateLifetime());
    }

    private IEnumerator HandleCrateLifetime()
    {
        yield return new WaitForSeconds(lifeSpan);
        DestroyCrate();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (IsGround(collision)) HandleGroundCollision();
        if (IsPlayer(collision, out PhotonView playerView)) HandlePlayerCollision(playerView);
    }

    private bool IsGround(Collider2D collider) => collider.CompareTag("Ground");

    private bool IsPlayer(Collider2D collider, out PhotonView view)
    {
        view = collider.transform.root.GetComponent<PhotonView>();
        return collider.CompareTag("Player") && view != null;
    }

    private void HandleGroundCollision()
    {
        StopCratePhysics();
        PlaySpawnAnimation();
    }

    private void HandlePlayerCollision(PhotonView playerView)
    {
        GivePlayerCrateItem(playerView);
        PlayPickupEffect();
    }

    private void StopCratePhysics()
    {
        rb.velocity = Vector3.zero;
        rb.isKinematic = true;
    }

    private void PlaySpawnAnimation()
    {
        animator.Play("CrateSpawnAnimation");
    }

    private void GivePlayerCrateItem(PhotonView playerView)
    {
        
        int itemIndex = itemDatabase.GetIdexOfCrateItem(itemDatabase.GetRandomCrateItem());
        SwapPlayerCurrentItemWith(playerView, itemIndex);
    }

    private void SwapPlayerCurrentItemWith(PhotonView playerView, int newItemIndex)
    {
        playerView.RPC("SwapItems", RpcTarget.AllBuffered, newItemIndex);
    }

    private void PlayPickupEffect()
    {
        PhotonNetwork.Instantiate(destroyParticle.name, transform.position, Quaternion.identity);
        DestroyCrate();
    }

    private void DestroyCrate()
    {
        OnCrateDestroyed?.Invoke();
        PhotonNetwork.Destroy(gameObject);
    }

}
    
    /*
    [SerializeField] public SpawnCrate spawner;
    public int itemIndex;
    public bool hasLanded = false;
    public ParticleSystem destroyParticle;
    // This function is called when this GameObject collides with another GameObject

    private void Start()
    {
        this.gameObject.GetComponent<SpriteRenderer>().enabled = false;
    }
    private void Update()
    {
        if(transform.position.y < -13)
        {
            DestroyCrateNetworked();
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!hasLanded && collision.gameObject.CompareTag("Ground"))
        {
            hasLanded = true;
            this.gameObject.GetComponent<SpriteRenderer>().enabled = true;
            this.gameObject.GetComponent<Rigidbody2D>().velocity = Vector3.zero; // Stops the velocity
            this.gameObject.GetComponent<Rigidbody2D>().isKinematic = true;      // Makes the object not be affected by physics
            gameObject.GetComponent<Animator>().Play("CrateSpawnAnimation");
            return;
        }

        if (hasLanded && collision.gameObject.CompareTag("Player"))
        {
            if (spawner == null || spawner.items == null || spawner.items.Length == 0)
            {
                Debug.LogError("Spawner or items array is null/empty. Ensure it is properly initialized.");
                return;
            }

            itemIndex = Random.Range(0, spawner.items.Length); // Fixed range
            Debug.Log($"Selected itemIndex: {itemIndex}");

            PhotonView playerview = collision.gameObject.transform.root.GetComponent<PhotonView>();
            if (playerview != null)
            {
                playerview.RPC("SwapItems", RpcTarget.AllBuffered, itemIndex);
                spawner.view.RPC("SetSpawnFlag", RpcTarget.All, false);
            }
            else
            {
                Debug.LogError("PhotonView is null on the player GameObject. Ensure the player has a PhotonView component.");
            }

            PhotonNetwork.Instantiate(destroyParticle.name, transform.position, Quaternion.identity);
            DestroyCrateNetworked();
        }
    }

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        if(propertiesThatChanged.ContainsKey("Winner"))
        {
            PhotonNetwork.Destroy(gameObject); 
        }
    }

    public void DestroyCrateNetworked()
    {
        if (this.gameObject != null)
        {
            this.GetComponent<PhotonView>().RPC("DestroyCrateOnAllClients", RpcTarget.AllBuffered);
        }
    }

    [PunRPC]
    void DestroyCrateOnAllClients()
    {
        // Destroy this crate
        spawner.StartCoroutine("crateSpawnTimer");
        PhotonNetwork.Destroy(gameObject);
    }
    */

