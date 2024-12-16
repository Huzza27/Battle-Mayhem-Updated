using ExitGames.Client.Photon;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class SelectColor : MonoBehaviour
{
    /*
     * For selecting color button
     */
    public Button p1Button; // Drag and drop your button in the Unity Inspector
    public float scaleAmount = 1.2f; // How much the button will scale up
    public float animationTime = 0.2f; // How long the animation will take
    public ReadyUp readyUpScript;

  

    /*
     * For going into color select wheel
     */
    public GameObject container;
    public Button leftArrow, rightArrow;
    public bool isOpen = false;
    private Vector3 originalScale;

    void Start()
    {
        // Store the original scale of the button
        originalScale = container.transform.localScale;
    }

    public void ToggleColorWheel()
    {
        Hashtable properties = PhotonNetwork.LocalPlayer.CustomProperties;
        if(properties.ContainsKey("Ready") && (bool)properties["Ready"])
        {
            return;
        }
        if (!isOpen)
        {
            // Scale up the container
            ScaleUpContainer();
        }
        else
        {
            ScaleDownContainer();
        }
        isOpen = !isOpen;

    }

    void ScaleUpContainer()
    {
        
        OpenColorWheel();
    }

    void ScaleDownContainer()
    {
        // First, scale up a bit for feedback, then scale down
        LeanTween.scale(container.gameObject, originalScale * 1.1f, animationTime / 4) // Small scale-up before shrinking
            .setEase(LeanTweenType.easeOutQuad) // Smooth transition for the initial scale-up
            .setOnComplete(() =>
            {
                // Then scale down with a bounce effect
                LeanTween.scale(container.gameObject, originalScale * scaleAmount, animationTime)
                    .setEase(LeanTweenType.easeOutBounce) // Bounce effect for the shrink down
                    .setOnComplete(ResetScale); // Once done, return to the original scale
            });
    }

    public void ToggleButtons(bool on, GameObject button)
    {
        button.SetActive(on);
        this.gameObject.SetActive(!on);
    }

    void ResetScale()
    {
        // After scaling down, return to the original size with bounce
        LeanTween.scale(container.gameObject, originalScale, animationTime)
            .setEase(LeanTweenType.easeOutBounce); // Bounce back to original size
        ToggleArrows(isOpen);
    }



    public void OpenColorWheel()
    {
        LeanTween.scale(container.gameObject, originalScale * 1.2f, animationTime)
            .setEase(LeanTweenType.easeOutBounce); // Bouncy effect when scaling up
        ToggleArrows(!isOpen);
    }

    private void ToggleArrows(bool isOpen)
    {
        if (isOpen)
        {
            //Scale Arrows Up
            LeanTween.scale(leftArrow.gameObject, new Vector2(1f, 1f), animationTime)
                .setEase(LeanTweenType.easeOutBounce); // Bouncy effect when scaling up
            LeanTween.scale(rightArrow.gameObject, new Vector2(1f, 1f), animationTime)
               .setEase(LeanTweenType.easeOutBounce); // Bouncy effect when scaling up
        }
        else if(!isOpen)
        {
            //Scale Arrows Down
            LeanTween.scale(leftArrow.gameObject, new Vector2(0f, 0f), animationTime)
            .setEase(LeanTweenType.easeOutBounce); // Add bounce when returning to original size
            LeanTween.scale(rightArrow.gameObject, new Vector2(0f, 0f), animationTime)
            .setEase(LeanTweenType.easeOutBounce);
        }
    }
}
