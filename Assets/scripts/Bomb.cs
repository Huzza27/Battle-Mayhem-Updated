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
            targetView = collision.gameObject.GetComponent<PhotonView>();
            ExplodeOnHit("Player");
        }
        else if (collision.collider.CompareTag("Ground"))
        {
            ExplodeOnHit("Ground");
        }
    }

    public void ExplodeOnHit(string target)
    {
        if (target.Equals("Player"))
        {
            HitPlayer(100f);
        }
        else if (target.Equals("Ground"))
        {
            DealAoEDamage();
            PlayParticles();
            DestroyBomb();
        }
    }

    private void ExplodeOnTimer()
    {
        PlayParticles();
        DealAoEDamage();
        DestroyBomb();
    }

    private void DealAoEDamage()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 3f);
        foreach (Collider2D collider in colliders)
        {
            if (collider.CompareTag("Player"))
            {
                targetView = collider.GetComponentInParent<PhotonView>();
                if (targetView.ViewID != thrower_view.ViewID)
                {
                    HitPlayer(CalculateDamage(transform.position, collider.transform.position, 100f, 3f));
                }
            }
        }
    }

    void HitPlayer(float damage)
    {
        PlayParticles();
        targetView.RPC("ReduceHealth", RpcTarget.All, damage);
        DestroyBomb();
    }

    public void PlayParticles()
    {
        var explosionInstance = PhotonNetwork.Instantiate(explosionParticles.name, transform.position, Quaternion.identity);
        explosionInstance.GetComponent<ParticleSystem>().Play();
        Destroy(explosionInstance, explosionParticles.main.duration); // Clean up particles after duration
    }

    private void DestroyBomb()
    {
            PhotonNetwork.Destroy(gameObject);
    }

    public float CalculateDamage(Vector2 attackCenter, Vector2 targetPosition, float maxDamage, float radius)
    {
        float distance = Vector3.Distance(attackCenter, targetPosition);
        if (distance > radius) return 0f;
        return Mathf.Max(maxDamage * (1 - (distance / radius)), 0f);
    }
}
