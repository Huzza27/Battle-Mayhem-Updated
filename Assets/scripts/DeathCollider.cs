using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class DeathCollider : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            GameObject player;
            if(collision.gameObject.transform.parent != null)
            {
                player = collision.gameObject.transform.root.gameObject;
            }
            var health = collision.gameObject.GetComponent<Health>();
            if (!health.isDead && collision.GetComponent<PhotonView>().IsMine) // Only local player triggers death
            {
                collision.gameObject.GetComponent<PhotonView>().RPC("Death", RpcTarget.AllBuffered, collision.gameObject.GetComponent<PhotonView>().Owner.ActorNumber);
            }
        }
        else if(collision.CompareTag("bullet"))
        {
            Bullet bullet = collision.gameObject.GetComponent<Bullet>();
            bullet.DestroyObject();
        }
        else
        {
            PhotonNetwork.Destroy(collision.gameObject);
            Debug.Log("Item Destroyed");
        }
    }
}
