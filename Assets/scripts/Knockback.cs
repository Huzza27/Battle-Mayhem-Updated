using UnityEngine;
using Photon.Pun;
using System.Collections;
using Smooth;

public class Knockback : MonoBehaviour
{
    [SerializeField] private float knockbackDuration = 0.3f;
    [SerializeField] private float syncDelay = 0.1f; // Delay before re-enabling sync

    private PhotonView view;
    private bool isBeingKnockedBack = false;
    private Movement movement;
    private SmoothSyncPUN2 sync;
    private Rigidbody2D rb;

    private void Awake()
    {
        view = GetComponent<PhotonView>();
        movement = GetComponent<Movement>();
        sync = GetComponent<SmoothSyncPUN2>();
        rb = GetComponent<Rigidbody2D>();
    }

    [PunRPC]
    public void TakeKnockBackFromBullet(float knockbackForce, int viewId)
    {
        if (view.ViewID != viewId || isBeingKnockedBack) return;

        StartCoroutine(ApplyKnockback(knockbackForce));
    }

    private IEnumerator ApplyKnockback(float knockbackForce)
    {
        isBeingKnockedBack = true;

        // Disable movement and sync temporarily
        movement.enabled = false;
        sync.enabled = false;

        // Apply knockback force
        rb.velocity = Vector2.zero; // Reset velocity first
        rb.AddForce(new Vector2(knockbackForce, 0), ForceMode2D.Impulse);

        // Wait for knockback duration
        yield return new WaitForSeconds(knockbackDuration);

        // Reset velocity after knockback
        rb.velocity = Vector2.zero;

        // Re-enable movement
        movement.enabled = true;

        // Wait a short delay to ensure physics have settled
        yield return new WaitForSeconds(syncDelay);

        // Re-enable sync
        sync.enabled = true;

        isBeingKnockedBack = false;
    }

    // Optional: Add method to check if knockback can be applied
    public bool CanBeKnockedBack()
    {
        return !isBeingKnockedBack;
    }
}