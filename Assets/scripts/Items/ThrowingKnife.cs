using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ThrowingKnife : MonoBehaviour
{
    public Vector2 dir;
    public PhotonView thrower_view;
    public Rigidbody2D rb;
    public float lifetTime;
    public PhotonView targetView;
    public float tossForce;
    public float damage;
    public PhotonView knife_view;
    bool startDestroyTimer = true;

    private Vector2 directionToMouse;
    private Vector3 playerPosition;
    private float distanceToMouse;

    bool hitSomething = false;
    public float rotationSpeed;
    public SpriteRenderer knifeRenderer;


    void Start()
    {
        if (thrower_view.IsMine)
        {
            rb.AddForce(-dir * tossForce, ForceMode2D.Impulse);
        }
    }

    public void ExternalCollisionCheck(Collider2D collision)
    {
        if (collision.CompareTag("Player") && collision.gameObject.transform.root.GetComponent<PhotonView>().ViewID != thrower_view.ViewID)
        {
            startDestroyTimer = false;
            targetView = collision.gameObject.GetComponent<PhotonView>();
            HitPlayer(damage);
            DestroyKnife();
        }

        if(collision.CompareTag("Boundary Collider"))
        {
            DestroyKnife();
        }

        if (startDestroyTimer)
        {
            Invoke("DestroyKnife", 5f);
        }
    }

    void MoveToCollisionPoint(Collider2D collision)
    {
        // Get the point of collision (approximation for Rigidbody2D)
        Vector3 collisionPoint = collision.ClosestPoint(transform.position);
        transform.position = collisionPoint; // Set knife's position to the collision point
    }

    void DestroyKnife()
    {
        PhotonNetwork.Destroy(gameObject);
    }

    void HitPlayer(float damage)
    {
        targetView.RPC("ReduceHealth", RpcTarget.All, damage);
    }

    void Update()
    {
        if (!hitSomething)
        {
            transform.Rotate(0, 0, -rotationSpeed * Time.deltaTime);
        }
    }
}
