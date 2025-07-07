using Photon.Pun.Demo.Asteroids;
using Photon.Pun;
using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour
{
    //public PhotonView view;
    public float moveSpeed;

    private bool isBullet = false;
    private Vector2 direction;

    public SpriteRenderer spriteRenderer;
    [SerializeField] public Item gun;
    public Sprite bullet, muzzleFlash;
    PhotonView currentTarget;

    Damage damageScript;
    public int shooterViewID;

    public GameObject hitParticles;
    public BulletPool pool;

    [Header("Bullet State Management")]
    public bool hasDeflected = false;
    public bool isCurrentlyDeflecting = false;
    bool hasHitPlayer = false;
    

    public void ResetBullet()
    {
        direction = Vector2.zero;
        spriteRenderer.sprite = muzzleFlash;
        hasHitPlayer = false;
        hasDeflected = false;
        isCurrentlyDeflecting = false;
        isBullet = false;
    }

    private void OnEnable()
    {
        StartCoroutine(changeSprite());
    }

    private void Update()
    {
        if (isBullet)
        {
            // Move bullet in the stored direction
            transform.Translate(-direction * moveSpeed * Time.deltaTime, Space.World);
        }
    }


    [PunRPC]
    public void SetShooterID(int viewID)
    {
        shooterViewID = viewID;
    }

    public void SetDirection(Vector2 shootDirection)
    {
        // Directly assign the provided shootDirection without inversion
        direction = shootDirection.normalized;

        // Calculate the angle in degrees for the bullet rotation
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Apply the rotation to the bullet to match its direction
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isCurrentlyDeflecting)
            return;

        if (collision.CompareTag("Mirror"))
        {
            Deflect();
            Mirror mirror = collision.gameObject.GetComponent<Mirror>();
            mirror.OnHitMirror();
        }

        if (collision.CompareTag("Player"))
        {
            if(!PhotonNetwork.IsMasterClient)
            {
                DestroyObject();
                return;
            }
            damageScript = collision.gameObject.transform.root.GetComponent<Damage>();

            PhotonView targetView = collision.transform.root.GetComponent<PhotonView>();
            currentTarget = targetView;
            gun = targetView.GetComponent<GunMechanicManager>().heldItem;
            if ((targetView.ViewID != shooterViewID) || hasDeflected)
            {
                Vector2 knockbackForce = direction.normalized * 4; // Or just hardcode for now

                // Send knockback via stream by setting the knockback flag on the target
                KnockbackReceiver knockbackReceiver = collision.transform.root.GetComponent<KnockbackReceiver>();
                if (knockbackReceiver != null)
                {
                    knockbackReceiver.QueueKnockback(-direction.normalized * 40);
                }

                PhotonNetwork.Instantiate(hitParticles.name, transform.position, Quaternion.identity);
                PhotonView shooterView = PhotonView.Find(shooterViewID);
                targetView.RPC("HitPlayer", targetView.Owner, gun.GetDamage(), targetView.ViewID, shooterView.Owner.ActorNumber, false);
                DestroyObject();
            }
        }
    }

    void VisualKnockBackMaskForShooter(PhotonView view)
    {
        if(view.ViewID == shooterViewID)
        {

        }
    }


    private void StopBullet()
    {
        moveSpeed = 0f; // Stop the bullet from moving
        direction = Vector2.zero;
    }

    private void Deflect()
    {
        // Find shooter
        isCurrentlyDeflecting = true;
        PhotonView shooterPhotonView = PhotonView.Find(shooterViewID);
        if (shooterPhotonView == null)
        {
            shooterPhotonView.RPC("PlayHitSound", RpcTarget.All);
            Debug.LogError("Shooter not found!");
        }

        Vector2 directionToShooter = (shooterPhotonView.transform.position - transform.position).normalized;

        Debug.Log("Bullet rotated towards shooter.");
        RotateBullet(directionToShooter);

        Debug.Log("Bullet moving towards shooter.");
        MoveBullet(directionToShooter);
        isCurrentlyDeflecting = false;
        hasDeflected = true;
        MatchStatsManager.Instance.RecordBlock(currentTarget.OwnerActorNr.ToString());
    }

    private void RotateBullet(Vector2 newDirection)
    {
        // Rotate the bullet to face the shooter
        float angle = Mathf.Atan2(newDirection.y, newDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private void MoveBullet(Vector2 newDirection)
    {
        moveSpeed = 25f;
        direction = -newDirection;
    }

    public void DestroyObject()
    {
        ResetBullet();
        gameObject.SetActive(false);
    }

    IEnumerator changeSprite()
    {
        yield return new WaitForSeconds(0.02f);
        spriteRenderer.sprite = bullet;
        isBullet = true;
    }
}