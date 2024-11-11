using UnityEngine;
using System.Collections;
using Photon.Pun; // For multiplayer synchronization, if needed

public class Damage : MonoBehaviour
{
    private SpriteRenderer[] spriteRenderers;
    public float blinkDuration = 1f;
    public float blinkInterval = 0.2f;
    Collider2D top, bottom;
    private void Awake()
    {
        // Get all sprite renderers attached to this character
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
    }

    [PunRPC]
    public void HitPlayer(float damage, int targetViewID)
    {
        PhotonView targetView = PhotonView.Find(targetViewID);
        targetView.RPC("ReduceHealth", RpcTarget.All, damage);
        targetView.RPC("activateVisuals", RpcTarget.All);
    }

    [PunRPC]
    private void EnableColliders(bool enable)
    {
        top.enabled = enable;
        bottom.enabled = enable;
    }

    [PunRPC]
    private void activateVisuals()
    {
        StartCoroutine(InvincibilityAnimation(blinkDuration, blinkInterval));
    }

    // Set alpha for all sprites
    public void SetAlpha(float alpha)
    {
        foreach (SpriteRenderer renderer in spriteRenderers)
        {
            Color color = renderer.color;
            color.a = alpha;
            renderer.color = color;
        }
    }

    // Invincibility animation coroutine
    public IEnumerator InvincibilityAnimation(float duration, float blinkInterval)
    {
        float elapsed = 0f;
        bool isVisible = true;

        while (elapsed < duration)
        {
            // Toggle visibility
            SetAlpha(isVisible ? 1f : 0.5f); // Adjust to desired alpha for transparency
            isVisible = !isVisible;

            elapsed += blinkInterval;
            yield return new WaitForSeconds(blinkInterval);
        }

        // Ensure visibility is fully restored at the end
        SetAlpha(1f);
    }
}
