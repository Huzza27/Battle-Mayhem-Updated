using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombExternalCollider : MonoBehaviour
{
    public Bomb bomb;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && collision.gameObject.transform.root.GetComponent<PhotonView>().ViewID != bomb.thrower_view.ViewID)
        {
            bomb.targetView = collision.gameObject.GetComponent<PhotonView>();
            bomb.ExplodeOnHit("Player");
        }
    }
}
