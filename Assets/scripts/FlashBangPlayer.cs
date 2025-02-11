using System.Collections;
using UnityEngine;
using Photon.Pun;

public class FlashBangPlayer : MonoBehaviourPunCallbacks
{
    public CanvasGroup canvasGroup;  // This is on a UI element, not the player
    public float initiateSpeed = 0.1f;
    public float fadeSpeed = 0.5f;
    public float duration = 2.0f;

    private PhotonView photonView;
    private int fadeInTweenId;
    private int fadeOutTweenId;

    private void Awake()
    {
        photonView = GetComponent<PhotonView>();
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
    }

    [PunRPC]
    public void ActivateFlashBang()
    {
        if (!photonView.IsMine) // Ensure the shooter isn't affected
        {
            if (canvasGroup != null)
            {
                // Cancel any existing tweens on the CanvasGroup's GameObject
                if (fadeInTweenId != 0) LeanTween.cancel(canvasGroup.gameObject, fadeInTweenId);
                if (fadeOutTweenId != 0) LeanTween.cancel(canvasGroup.gameObject, fadeOutTweenId);

                StopAllCoroutines();
                StartCoroutine(FlashSequence());
            }
            else
            {
                Debug.LogError("CanvasGroup is null on FlashBangPlayer!");
            }
        }
    }

    private IEnumerator FlashSequence()
    {
        Debug.Log($"Starting flash sequence. Initial alpha: {canvasGroup.alpha}");

        // Explicitly set starting alpha
        canvasGroup.alpha = 0f;

        // Use canvasGroup.gameObject since the tween should be on the UI element
        fadeInTweenId = LeanTween.value(canvasGroup.gameObject, updateAlpha, 0f, 1f, initiateSpeed)
            .setOnComplete(() => {
                Debug.Log($"Fade in complete. Alpha: {canvasGroup.alpha}");
            }).id;

        yield return new WaitForSeconds(duration);

        fadeOutTweenId = LeanTween.value(canvasGroup.gameObject, updateAlpha, 1f, 0f, fadeSpeed)
            .setOnComplete(() => {
                Debug.Log($"Fade out complete. Alpha: {canvasGroup.alpha}");
            }).id;
    }

    private void updateAlpha(float value)
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = value;
            Debug.Log($"Updating alpha to: {value}");
        }
    }

    private void OnDisable()
    {
        // Clean up any running tweens on the CanvasGroup's GameObject
        if (fadeInTweenId != 0) LeanTween.cancel(canvasGroup.gameObject, fadeInTweenId);
        if (fadeOutTweenId != 0) LeanTween.cancel(canvasGroup.gameObject, fadeOutTweenId);
    }
}