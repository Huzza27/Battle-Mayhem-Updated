using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LoopingClouds : MonoBehaviour
{
    // Assign these in the inspector
    public Sprite[] animationFrames;  // Array of sprites for the animation
    public Image targetImage;         // UI Image component to display the animation

    public float framesPerSecond = 30f;  // Set the frame rate (30 FPS)
    private int currentFrame = 0;
    private float frameDuration;  // Time between frames

    void Start()
    {
        // Calculate the time between each frame (1 second / FPS)
        frameDuration = 1f / framesPerSecond;

        // Start the animation coroutine
        StartCoroutine(PlayAnimation());
    }

    IEnumerator PlayAnimation()
    {
        // Loop indefinitely
        while (true)
        {
            // Set the current sprite on the UI Image component
            targetImage.sprite = animationFrames[currentFrame];

            // Wait for the duration of one frame
            yield return new WaitForSeconds(frameDuration);

            // Move to the next frame, looping back to the first if at the end
            currentFrame = (currentFrame + 1) % animationFrames.Length;
        }
    }
}
