using Photon.Pun;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CycleColors : MonoBehaviour
{
    private int colorIndex;
    public GameObject cycleContainer;
    public float cycleAmount = 288f;  // Adjust to your exact distance
    public float transitionDuration = 0.3f;  // Time it takes to slide to the next color

    private bool isCycling = false;

    public Image blueImage, redImage;


    public void CycleNextColor(int add)
    {
        // If already cycling, prevent further action
        if (isCycling)
            return;

        // Update the color index
        if(colorIndex == 3 && add > 0)
        {
            colorIndex = 0;
        }

        else if(colorIndex == 0 && add < 0)
        {
            colorIndex = 3;
        }

        else
        {
            colorIndex += add;
        }

        
        // Start the sliding animation
        StartCoroutine(SmoothCycle(add));

    }

    private IEnumerator SmoothCycle(int add)
    {
        isCycling = true;

        RectTransform rectTransform = cycleContainer.GetComponent<RectTransform>();
        Vector2 startPos = rectTransform.anchoredPosition; // Get the current position
        Vector2 targetPos = startPos + new Vector2(cycleAmount * add, 0f); // Calculate the target position

        float elapsedTime = 0f;

        // Smoothly interpolate the position over transitionDuration time
        while (elapsedTime < transitionDuration)
        {
            rectTransform.anchoredPosition = Vector2.Lerp(startPos, targetPos, elapsedTime / transitionDuration);
            elapsedTime += Time.deltaTime;
            yield return null;  // Wait for the next frame
        }

        // Ensure the final position is set exactly
        rectTransform.anchoredPosition = targetPos;
        isCycling = false;

        Debug.Log("Color set to " + colorIndex);
    }
}
