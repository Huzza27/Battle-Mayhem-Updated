using Photon.Pun;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    public Vector2 dir;
    public PhotonView thrower_view;
    private Rigidbody2D rb;
    public float lifetTime;
    public PhotonView targetView;
    public float defaultTossForce;
    public float damage;
    public AudioSource source;
    public AudioClip EXPLOSION_SFX;
    public PhotonView bomb_view;
    private Transform targetPlayer;

    [SerializeField] private ParticleSystem explosionParticles;


    public float maxTossForce; 
    public float minTossForce ; 

    public float proximityThresholdAngle = 45f; // Angle threshold to consider aiming near the enemy

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (thrower_view.IsMine)
        {            
            rb.AddForce(-dir * defaultTossForce, ForceMode2D.Impulse);
            StartCoroutine("lifeTimer");

        }
    }

    private IEnumerator lifeTimer()
    {
        yield return new WaitForSeconds(lifetTime);
        ExplodeOnTimer();
    }

    

    public void ExplodeOnHit(string target)
    {
        if (target.Equals("Player"))
        {
            HitPlayer(52f);
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
                targetView.RPC("HitPlayer", targetView.Owner, damage, targetView.ViewID, thrower_view.ViewID);
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
