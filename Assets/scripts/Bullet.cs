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

    private void Awake()
    {
        StartCoroutine("DestroyTimer");

        StartCoroutine("changeSprite");
    }

    private void Update()
    {
        if (isBullet)
        {
            // Move bullet in the stored direction
            transform.Translate(-direction * moveSpeed * Time.deltaTime, Space.World);
        }
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
        if (collision.CompareTag("Player") && !hasHitPlayer)
        {
            damageScript = collision.gameObject.transform.root.GetComponent<Damage>();

            PhotonView targetView = collision.transform.root.GetComponent<PhotonView>();
            if (targetView.ViewID != shooterViewID)
            {
                Debug.Log("Bullet Collided with Player " + PhotonNetwork.LocalPlayer.ActorNumber);
                Debug.Log("Owner of bullet: Player " + PhotonNetwork.LocalPlayer.ActorNumber + " Damage dealt:" + gun.GetDamage());

                targetView.RPC("HitPlayer", targetView.Owner, gun.GetDamage(), targetView.ViewID, view.ViewID);
            }
        }
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