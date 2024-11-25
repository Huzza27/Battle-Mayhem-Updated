using Photon.Pun;
using System.Collections;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    public Vector2 dir;
    public PhotonView thrower_view;
    private Rigidbody2D rb;
    public float lifetTime;
    public PhotonView targetView;
    public float tossForce;

    public float damage;

    [SerializeField] private ParticleSystem explosionParticles;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.AddForce(-dir * tossForce, ForceMode2D.Impulse);
        StartCoroutine("lifeTimer");
    }

    private IEnumerator lifeTimer()
    {
        yield return new WaitForSeconds(lifetTime);
        ExplodeOnTimer();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player") && collision.gameObject.GetComponent<PhotonView>().ViewID != thrower_view.ViewID)
        {
            targetView = collision.gameObject.GetComponent<PhotonView>(); ;
            ExplodeOnHit();
        }
    }

    private void ExplodeOnHit()
    {
        //Do something silly
    }

    private void ExplodeOnTimer()
    {
        PhotonNetwork.Instantiate(explosionParticles.name, transform.position, Quaternion.identity);

        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 3f);
        foreach (Collider2D collider in colliders)
        {
            if (collider.CompareTag("Player"))
            {
                targetView = collider.GetComponent<PhotonView>();
            }
        }
        PhotonNetwork.Destroy(gameObject);

    }
}
