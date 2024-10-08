using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;

public class KatanaColllisionDetection : MonoBehaviour
{
    public GameObject player;
    PhotonView view;
    private Vector2 explosionDir;
    private float force;
    // Start is called before the first frame update
    void Start()
    {
        view = player.GetComponent<PhotonView>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!view.IsMine)
            return;
        PhotonView target = collision.gameObject.GetComponent<PhotonView>();

        if (target != null && (!target.IsMine || target.IsRoomView))
        {
            if (target.tag == "Player")
            {
                GetComponent<BoxCollider2D>().enabled = false;    
                Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
                Item katana = player.GetComponent<GunMechanicManager>().heldItem;
                if (rb != null)
                {
                    // Calculate direction from the explosion point to the object
                    explosionDir = rb.transform.position - transform.position;
                    float distance = explosionDir.magnitude;

                    // Normalize the direction vector
                    explosionDir.Normalize();

                    // Apply force inversely proportional to the distance (optional)
                    force =  katana.GetHitKB() / (distance == 0 ? 1 : distance);  // Prevent division by zero

                    explosionDir.y = katana.GetVerticalBoost();

                }
                GameObject targetPlayer = collision.gameObject;
                int targetViewID = target.ViewID;
                target.RPC("HitMarker", RpcTarget.AllBuffered, 0f);
                target.RPC("ReduceHealth", RpcTarget.AllBuffered, player.GetComponent<GunMechanicManager>().heldItem.GetDamage(), targetViewID);
                target.RPC("TakeKnockBackFromBomb", RpcTarget.AllBuffered, explosionDir, force);
            }
        }
    }
}
