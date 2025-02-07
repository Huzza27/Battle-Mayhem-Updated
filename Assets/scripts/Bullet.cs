using Photon.Pun.Demo.Asteroids;
using Photon.Pun;
using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour
{
    public PhotonView view;
    public float moveSpeed;
    public float destroyTime;

    private bool isBullet = false;
    private Vector2 direction;

    public SpriteRenderer spriteRenderer;
    [SerializeField] public Item gun;
    public Sprite bullet;
    bool hasHitPlayer = false;
    Damage damageScript;
    public int shooterViewID;
    bool hasHitGroundOnUnderneath = false;
    public bool hasDeflected = false;
    private Coroutine destroyTimerCoroutine; // Store the destroy timer coroutine

    public GameObject hitParticles;

    private void Awake()
    {
        destroyTimerCoroutine = StartCoroutine(DestroyTimer());
        StartCoroutine(changeSprite());
    }

    private void Update()
    {
        if (isBullet && !hasHitGroundOnUnderneath)
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
        if (hasDeflected)
            return;

        if (collision.CompareTag("Mirror"))
        {
            Deflect(collision.gameObject);
        }

        if (collision.CompareTag("Ground") && -direction.y > 0)
        {
            hasHitGroundOnUnderneath = true;
            PhotonNetwork.Instantiate(hitParticles.name, transform.position, Quaternion.identity);
            view.RPC("DestroyObject", RpcTarget.AllBuffered);
        }

        if (collision.CompareTag("Player") && !hasHitPlayer)
        {
            damageScript = collision.gameObject.transform.root.GetComponent<Damage>();

            PhotonView targetView = collision.transform.root.GetComponent<PhotonView>();
            if (targetView.ViewID != shooterViewID)
            {
                PhotonNetwork.Instantiate(hitParticles.name, transform.position, Quaternion.identity);
                targetView.RPC("HitPlayer", targetView.Owner, gun.GetDamage(), targetView.ViewID, view.ViewID);
                view.RPC("DestroyObject", RpcTarget.AllBuffered);
            }
        }
    }

    private void Deflect(GameObject mirror)
    {
        if (hasDeflected) return; // Prevent multiple deflections

        Debug.Log("Bullet hit a mirror!");

        hasDeflected = true; // Mark as deflected
        hasHitPlayer = true; // Disable damage

        // Stop bullet movement
        StopBullet();
        Debug.Log("Bullet stopped.");
        Deflect();
    }

    private void StopBullet()
    {
        moveSpeed = 0f; // Stop the bullet from moving
        direction = Vector2.zero;
        if (destroyTimerCoroutine != null)
        {
            StopCoroutine(destroyTimerCoroutine); // Stop destroy timer
        }
    }

    private void Deflect()
    {
       
        // Find shooter
        PhotonView shooterPhotonView = PhotonView.Find(shooterViewID);
        if (shooterPhotonView == null)
        {
            Debug.LogError("Shooter not found!");
           
        }

        Vector2 directionToShooter = (shooterPhotonView.transform.position - transform.position).normalized;

        Debug.Log("Bullet rotated towards shooter.");
        RotateBullet(directionToShooter);
       
        Debug.Log("Bullet moving towards shooter.");
        MoveBullet(directionToShooter);
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

    private IEnumerator DestroyTimer()
    {
        yield return new WaitForSeconds(destroyTime);
        view.RPC("DestroyObject", RpcTarget.AllBuffered);
    }

    [PunRPC]
    public void DestroyObject()
    {
        Destroy(this.gameObject);
    }

    IEnumerator changeSprite()
    {
        yield return new WaitForSeconds(0.02f);
        spriteRenderer.sprite = bullet;
        isBullet = true;
    }
}
