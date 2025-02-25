using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraItem : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public float initiateSpeed = 0.1f;    // Default values in case not set in inspector
    public float fadesSpeed = 0.3f;       // Kept your original variable name
    public float duration = 2f;           // How long the flash stays at full brightness
    public MiscItemDependencies dependencies;

    private void Awake()
    {
        // Make sure canvasGroup is assigned
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        // Initialize alpha to 0 (invisible)
        canvasGroup.alpha = 0f;
    }

    void Start()
    {

        // Skip the flash effect for the shooter
        if (dependencies.shooterView != null)
        {
            return;
        }

        // Flash everyone else
        FadeIn();
    }

    public void FadeIn()
    {
        // Cancel any existing tweens on this object to avoid conflicts
        LeanTween.cancel(gameObject);

        // Fade in
        LeanTween.value(gameObject, 0f, 1f, initiateSpeed)
            .setOnUpdate((float val) => canvasGroup.alpha = val)
            .setOnComplete(() => {
                // Only invoke FadeOut if this gameObject is still active
                if (gameObject.activeInHierarchy)
                    Invoke("FadeOut", duration);
            });
    }

    public void FadeOut()
    {
        // Cancel any existing tweens on this object
        LeanTween.cancel(gameObject);

        // Fade out
        LeanTween.value(gameObject, 1f, 0f, fadesSpeed)
            .setOnUpdate((float val) => canvasGroup.alpha = val)
            .setOnComplete(() => {
                // Optional: Destroy or deactivate after fadeout
                // Uncomment the appropriate line if needed:
                // gameObject.SetActive(false);
                // Destroy(gameObject);
            });
    }

    private void OnDisable()
    {
        // Clean up when object is disabled
        CancelInvoke();
        LeanTween.cancel(gameObject);
    }
}