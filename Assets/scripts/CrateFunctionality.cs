using UnityEngine;
using Photon.Pun;
using ExitGames.Client.Photon;



public class CrateFunctionality : MonoBehaviourPunCallbacks 
{
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

            PhotonView playerview = collision.gameObject.GetComponent<PhotonView>();
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

}

