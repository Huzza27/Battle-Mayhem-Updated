using Photon.Pun;
using UnityEngine;

public class MapBulletBoundary : MonoBehaviourPunCallbacks
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("bullet"))
        {
            PhotonView bulletView = collision.GetComponent<PhotonView>();
            if (bulletView != null)
            {
                // Ensure the bullet is deactivated on all clients
                bulletView.RPC("DestroyObject", RpcTarget.All);
            }
        }
    }
}