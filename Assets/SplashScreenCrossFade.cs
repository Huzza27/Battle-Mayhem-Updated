using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class SplashScreenCrossFade : MonoBehaviour
{
    public Image imageToFade; // Assign your image in the Inspector
    public float fadeDuration = 3f;
    public float waitTime = 5f;
    public string nextSceneName; // Set the name of the next scene in the Inspector

    private void Start()
    {
        if (imageToFade != null)
        {
            // Start the crossfade process
            StartCoroutine(CrossfadeLoop());
        }
    }

    private IEnumerator CrossfadeLoop()
    {
        while (true)
        {
            // Fade in
            LeanTween.alpha(imageToFade.rectTransform, 1f, fadeDuration);
            yield return new WaitForSeconds(fadeDuration);

            // Wait before fading out
            yield return new WaitForSeconds(waitTime);

            // Fade out
            LeanTween.alpha(imageToFade.rectTransform, 0f, fadeDuration);
            yield return new WaitForSeconds(fadeDuration);

            // Transition to the next scene after fading out
            if (!string.IsNullOrEmpty(nextSceneName))
            {
                SceneManager.LoadScene(nextSceneName);
                yield break;
            }

            // Wait before fading in again
            yield return new WaitForSeconds(waitTime);
        }
    }
}
