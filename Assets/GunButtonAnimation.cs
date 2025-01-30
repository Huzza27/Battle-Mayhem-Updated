using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GunButtonAnimation : MonoBehaviour
{
    public float hoverScaleAmount = 1.2f; // How much the button scales up when hovered
    public float animationTime = 0.2f; // Time for the scaling animation
    private Vector3 originalScale;

    public GameObject checkmark;
    public bool isSelected = false;

    public GunButtonAnimation[] gunButtonAnimation;

    public GameObject stats;

    void Start()
    {
        // Store the original scale of the button
        originalScale = transform.localScale;
    }

    // Triggered when the mouse pointer enters the button area
    public void OnPointerEnter()
    {
        ScaleUp(this.gameObject);
        stats.SetActive(true);
    }

    // Triggered when the mouse pointer exits the button area
    public void OnPointerExit()
    {
        ScaleDown(this.gameObject);
        stats.SetActive(false);
    }

    private void ScaleUp(GameObject image)
    {
        // Scale the button up with a cartoony bounce effect
        LeanTween.scale(gameObject, originalScale * hoverScaleAmount, animationTime)
            .setEase(LeanTweenType.easeOutBounce); // Bouncy effect for the expansion
    }

    private void ScaleDown(GameObject image)
    {
        // Scale the button back to its original size
        LeanTween.scale(gameObject, originalScale, animationTime)
            .setEase(LeanTweenType.easeOutBounce); // Bouncy effect when returning to original size
    }

    public void Select()
    {
        isSelected = !isSelected;
        // Shrink the button back to its original size with a bounce effect
        LeanTween.scale(gameObject, originalScale, animationTime)
            .setEase(LeanTweenType.easeOutBounce); // Bouncy effect as it shrinks to original size

        CheckAllCeckMarks();
        ToggleCheckMark(isSelected);
    }

    public void CheckAllCeckMarks()
    {
        foreach(var item in gunButtonAnimation)
        {
            if (item.gameObject != this.gameObject && item.isSelected == true)
            {
                item.ToggleCheckMark(false);
                item.isSelected = false;
            }
        }
    }

    public void ToggleCheckMark(bool on)
    {
        if (on)
        {
            LeanTween.scale(checkmark, new Vector2(1f, 1f), animationTime)
            .setEase(LeanTweenType.easeOutBounce); // Bouncy effect for the expansio
        }

        if (!on)
        {
            LeanTween.scale(checkmark, new Vector2(0f, 0f), animationTime)
           .setEase(LeanTweenType.easeOutBounce); // Bouncy effect for the expansio
           
        }
    }
}
