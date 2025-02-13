using Photon.Pun;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CameraItem : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public float initiateSpeed = 0.1f;    // Quick flash
    public float fadeSpeed = 0.5f;        // Slower fade out
    public float duration = 2.0f;         // How long the flash stays at full brightness
    public PhotonView shooterView;
    PhotonView view;
    public MiscItemDependencies dependencies;

    private void Awake()
    {
        // Ensure canvas group is set
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        // Start with canvas fully transparent
        canvasGroup.alpha = 0f;

    }

    private void Start()
    {
        shooterView = dependencies.shooterView;
        if (shooterView == null)
        {
            
            UseCamera();
            return;
        }
    }

    public void UseCamera()
    {
            ActivateFlashBang();
    }

    public void ActivateFlashBang()
    {
        // Start the flash effect
        StopAllCoroutines();  // Stop any ongoing effects
        StartCoroutine(FlashSequence());
    }

    private IEnumerator FlashSequence()
    {
        // Fade in quickly
        LeanTween.cancel(gameObject);
        LeanTween.value(gameObject, 0f, 1f, initiateSpeed)
            .setOnUpdate((float val) => canvasGroup.alpha = val);

        // Wait for duration
        yield return new WaitForSeconds(duration);

        // Fade out
        LeanTween.value(gameObject, 1f, 0f, fadeSpeed)
            .setOnUpdate((float val) => canvasGroup.alpha = val);
    }
}