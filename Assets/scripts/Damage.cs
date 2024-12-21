using UnityEngine;
using System.Collections;
using Photon.Pun; // For multiplayer synchronization, if needed

public class Damage : MonoBehaviour
{
    private SpriteRenderer[] spriteRenderers;
    public float blinkDuration = 1f;
    public float blinkInterval = 0.2f;
    public bool isInvinible = false;
    PhotonView view;
    private void Awake()
    {
        // Get all sprite renderers attached to this character
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        view = GetComponent<PhotonView>();
    }

    [PunRPC]
    public void HitPlayer(float damage, int targetViewID, int bulletViewID)
    {
        Debug.Log("Recieve HitPlayer RPC Call from Bullet");
        PhotonView targetView = PhotonView.Find(targetViewID);
        Debug.Log("Target view = " + targetView);
        Debug.Log("target view equals my view? " + (targetView.Owner == view.Owner));
        if (targetView != null && targetView.Owner == view.Owner) // Ensure correct ownership
        {
            Debug.Log("Ownership of view ensured");

            targetView.RPC("ReduceHealth", RpcTarget.All, damage);

            Debug.Log("Damaging player over network");

            targetView.RPC("activateVisuals", RpcTarget.All);

            Debug.Log("Activating inviciblity visuals");

        }

    }

    [PunRPC]
    private void SetInvincibleFlag(bool flag)
    {
        isInvinible = flag;
    }


    [PunRPC]
    private void activateVisuals()
    {
        view.RPC("SetInvincibleFlag", RpcTarget.All, true);
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
        view.RPC("SetInvincibleFlag", RpcTarget.All, false);
    }
}
