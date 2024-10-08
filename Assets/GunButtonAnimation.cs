using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GunButtonAnimation : MonoBehaviour
{
    public float hoverScaleAmount = 1.2f; // How much the button scales up when hovered
    public float animationTime = 0.2f; // Time for the scaling animation
    private Vector3 originalScale;

    void Start()
    {
        // Store the original scale of the button
        originalScale = transform.localScale;
    }

    // Triggered when the mouse pointer enters the button area
    public void OnPointerEnter()
    {
        // Scale the button up with a cartoony bounce effect
        LeanTween.scale(gameObject, originalScale * hoverScaleAmount, animationTime)
            .setEase(LeanTweenType.easeOutBounce); // Bouncy effect for the expansion
    }

    // Triggered when the mouse pointer exits the button area
    public void OnPointerExit()
    {
        // Scale the button back to its original size
        LeanTween.scale(gameObject, originalScale, animationTime)
            .setEase(LeanTweenType.easeOutBounce); // Bouncy effect when returning to original size
    }
}
