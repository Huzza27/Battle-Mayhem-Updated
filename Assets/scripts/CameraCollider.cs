using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraCollider : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Physics2D.IgnoreLayerCollision(6, 11);
        if(collision.collider.tag == "Knife")
        {
            PhotonNetwork.Destroy(collision.gameObject.transform.root.gameObject);
        }

    }
}
