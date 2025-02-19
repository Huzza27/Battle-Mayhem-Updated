using System.Collections;
using UnityEngine;

public class DummyBullet : MonoBehaviour
{
    public float moveSpeed;
    private bool isBullet = false;
    private Vector2 direction;
    public SpriteRenderer spriteRenderer;
    public Sprite bullet, muzzleFlash;
    public bool hasDeflected = false;
    public GameObject hitParticles;
    public BulletPool pool;

    public void Initialize()
    {
        // Set initial sprite to muzzle flash
        spriteRenderer.sprite = muzzleFlash;
        StartCoroutine(changeSprite());
    }

    void Update()
    {
        if (isBullet)
        {
            transform.Translate(-direction * moveSpeed * Time.deltaTime, Space.World);
        }
    }

    public void SetDirection(Vector2 shootDirection)
    {
        direction = shootDirection.normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle + 180); // Added 180 to correct orientation
    }

    IEnumerator changeSprite()
    {
        yield return new WaitForSeconds(0.02f);
        spriteRenderer.sprite = bullet;
        isBullet = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) // Using CompareTag for better performance
        {
            Destroy(gameObject);
        }
    }
}