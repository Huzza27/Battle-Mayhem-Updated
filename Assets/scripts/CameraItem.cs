using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraItem : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public float initiateSpeed;
    public float fadesSpeed;
    public float duration;
    void Start()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
        FadeIn();
    }

    public void FadeIn()
    {
        LeanTween.value(gameObject, 0f, 1f, initiateSpeed)
            .setOnUpdate((float val) => canvasGroup.alpha = val);
        Invoke("FadeOut", duration);
    }

    public void FadeOut()
    {
        LeanTween.value(gameObject, 1f, 0f, fadesSpeed)
            .setOnUpdate((float val) => canvasGroup.alpha = val);
    }
}
