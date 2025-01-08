using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowingKnife : MonoBehaviour
{
    public Vector2 dir;
    public PhotonView thrower_view;
    public Rigidbody2D rb;
    public float lifetTime;
    public PhotonView targetView;
    public float tossForce;
    public float damage;
    //public AudioSource source;
    //public AudioClip EXPLOSION_SFX;
    public PhotonView knife_view;
    bool startDestroyTimer = true;
    bool hitSomething = false;
    public float rotationSpeed;

    // Start is called before the first frame update

        void Start()
        {
            Debug.Log("Knife Spawned");
            rb.AddForce(-dir * tossForce, ForceMode2D.Impulse);
        }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        hitSomething = true;
        transform.parent = collision.gameObject.transform.root;
        if (collision.CompareTag("Player") && collision.gameObject.GetComponent<PhotonView>().ViewID != thrower_view.ViewID)
        {
            startDestroyTimer = false;
            targetView = collision.gameObject.GetComponent<PhotonView>();
            HitPlayer(damage);
        }
        rb.velocity = Vector2.zero;

        // Look at the center of the player
        LookAtCenterOfPlayer(collision);

        if(startDestroyTimer)
        {
            Invoke("DestroyKnife", 8f);
        }

    }


    void DestroyKnife()
    {
        PhotonNetwork.Destroy(this.gameObject);
    }

    void LookAtCenterOfPlayer(Collider2D collision)
    {
        Vector3 directionToCenter = collision.transform.position - transform.position; // Direction to the center of the collided object
        float angle = Mathf.Atan2(directionToCenter.y, directionToCenter.x) * Mathf.Rad2Deg; // Calculate angle
        transform.rotation = Quaternion.Euler(0, 0, angle); // Rotate to face the center
    }

    void HitPlayer(float damage)
    {
        targetView.RPC("ReduceHealth", RpcTarget.All, damage);
    }

    // Update is called once per frame
    void Update()
    {
            transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
    }
}
