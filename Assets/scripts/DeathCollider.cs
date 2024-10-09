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
            
            collision.gameObject.GetComponent<PhotonView>().RPC("Death", RpcTarget.AllBuffered);
        }
        else
        {
            PhotonNetwork.Destroy(collision.gameObject);    
        }
    }
}
