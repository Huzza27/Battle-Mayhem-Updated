using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class BombPlatformExplosion : MonoBehaviour
{
    GameObject bomb;
    Rigidbody2D rb;
    public PhotonView view;
    public float disableDuration;
    public BoxCollider2D mainCollider, bombCollider;
    private Transform orignalTransform;

    private void Awake()
    {
        orignalTransform = gameObject.transform;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        rb = collision.GetComponent<Rigidbody2D>();
        if (rb == null) return;
        if (collision.CompareTag("Bomb") && rb.velocity.y > 0f)
        {
            bomb = collision.gameObject;
            bomb.GetComponent<Bomb>().ExplodeOnHit("Ground");
            view.RPC("DisablePlatform", RpcTarget.All);
        }
    }

    [PunRPC]
    private void DisablePlatform()
    {
        if(this.GetComponent<SpriteRenderer>() == null) 
        {
            foreach(SpriteRenderer renderer in gameObject.GetComponentsInChildren<SpriteRenderer>())
            {
                renderer.enabled = false;            
            }
        }
        else
        {
            gameObject.GetComponent<SpriteRenderer>().enabled = false;   

        }
        mainCollider.enabled = false;
        bombCollider.enabled = false;
        Invoke("RespawnTimer", disableDuration);
    }

    private void RespawnTimer()
    {
        //gameObject.transform.localScale = Vector2.zero;
        if (this.GetComponent<SpriteRenderer>() == null)
        {
            foreach (SpriteRenderer renderer in gameObject.GetComponentsInChildren<SpriteRenderer>())
            {
                renderer.enabled = true;
            }
        }
        else
        {
            gameObject.GetComponent<SpriteRenderer>().enabled = true;
        }
        mainCollider.enabled= true;
        bombCollider.enabled= true;
        //EnableAnimation();
    }
    
    void EnableAnimation()
    {
        Vector2 originalScale = new Vector2(orignalTransform.localScale.x, orignalTransform.localScale.y);
        LeanTween.scale(gameObject, originalScale, 0.5f) // Animate to full scale (1, 1, 1) over 0.5 seconds
            .setEaseOutBack(); // Adds a smooth overshoot effect
    }
}
