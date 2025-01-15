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
    public AudioSource source;
    public AudioClip EXPLOSION_SFX;
    public PhotonView bomb_view;

    [SerializeField] private ParticleSystem explosionParticles;


    private Vector2 directionToMouse;
    private Vector3 playerPosition;
    private float distanceToMouse;

    public float maxTossForce; // Maximum force applied to the knife
    public float minTossForce ;  // Minimum force applied to the knife

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (thrower_view.IsMine)
        {
            // Get the mouse position in world space
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            // Calculate direction and distance
            playerPosition = thrower_view.transform.position;
            directionToMouse = (mousePosition - playerPosition).normalized;
            distanceToMouse = Vector2.Distance(playerPosition, mousePosition);

            // Adjust toss force based on distance to mouse
            float tossForce = Mathf.Lerp(minTossForce, maxTossForce, distanceToMouse / maxTossForce);

            // Apply force to the knife
            rb.AddForce(directionToMouse * tossForce, ForceMode2D.Impulse);
        }

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
            if(rb.velocity.y < 0)
            {
                return;
            }

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
        thrower_view.RPC("PlayExplosionSound", RpcTarget.All);
        var explosionInstance = PhotonNetwork.Instantiate(explosionParticles.name, transform.position, Quaternion.identity);
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
