using Photon.Pun.Demo.Asteroids;
using Photon.Pun;
using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour
{
    public PhotonView view;
    public bool MoveDir = false;
    public float moveSpeed;
    public float destroyTime;

    private bool isBullet = false;

    SpriteRenderer spriteRenderer;
    [SerializeField] public Item gun;
    public Sprite bullet;
    bool hasHitPlayer = false;
    Damage damageScript;
    private Vector2 direction;

    private void Awake()
    {
        StartCoroutine("DestroyTimer");
        spriteRenderer = GetComponent<SpriteRenderer>();
        StartCoroutine("changeSprite");
    }

    private void Update()
    {
        if (isBullet)
        {
            // Move bullet toward target position
            transform.Translate(-direction * moveSpeed * Time.deltaTime);
        }
    }
    public void SetDirection(Vector2 shootDirection)
    {
        direction = shootDirection.normalized;
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player") && !hasHitPlayer)
        {
            damageScript = collision.gameObject.transform.root.GetComponent<Damage>();
            
            PhotonView targetView = collision.transform.root.GetComponent<PhotonView>();

            targetView.RPC("HitPlayer", targetView.Owner, gun.GetDamage(), targetView.ViewID);
            view.RPC("DestroyObject", RpcTarget.All);
        }
    }

    private IEnumerator DestroyTimer()
    {
        yield return new WaitForSeconds(destroyTime);
        view.RPC("DestroyObject", RpcTarget.AllBuffered);
    }

    [PunRPC]
    public void changeDir_Left()
    {
        spriteRenderer.flipX = true;
        MoveDir = true;
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