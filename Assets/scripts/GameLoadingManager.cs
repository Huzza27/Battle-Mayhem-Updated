using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;



public class GameLoadingManager : MonoBehaviour
{
    [SerializeField] Image backgroundImage;
    [SerializeField] float backgroundFadeDuration = 5f;
    [SerializeField] private TextMeshProUGUI countdownText; // Reference to the text object
    [SerializeField] private float countdownDuration = 1f;  // Duration for each number's animation
    [SerializeField] private PhotonView view;
    public bool canStartCountdown = false;

    [Header("UI References")]
    [SerializeField] private Image loadingBar; // Reference to the loading bar UI (fill image)
    [SerializeField] private TextMeshProUGUI loadingText;

    [Header("Settings")]
    [SerializeField] private float totalLoadingTime = 4f; // Total duration of the loading process
    [SerializeField] private int steps = 3; // Number of intermediate steps before completing
    [SerializeField] float startLoadingDelay = 5f;

    private float elapsedTime = 0f;
    private float progress = 0f;

    private void Awake()
    {
        ResetGameLoadingManagerState();
   
    }


    public void ResetGameLoadingManagerState()
    {

        // Reset loading bar progress
        progress = 0f;
        elapsedTime = 0f;
        if (loadingBar != null)
        {
            loadingBar.fillAmount = 0f;
            loadingBar.enabled = true; // Ensure the loading bar is visible
        }

        // Reset loading text
        if (loadingText != null)
        {
            loadingText.text = "Loading...";
        }

        // Reset countdown text
        if (countdownText != null)
        {
            countdownText.text = "";
            countdownText.alpha = 1f; // Ensure visibility
            LeanTween.cancel(countdownText.gameObject); // Cancel any active animations
            countdownText.transform.localScale = Vector3.one; // Reset scale
        }

        // Reset background image opacity
        if (backgroundImage != null)
        {
            Color color = backgroundImage.color;
            color.a = 1f; // Fully opaque
            backgroundImage.color = color;
        }

        // Reset internal state flags
        canStartCountdown = false;
        // Reset Photon "IsLoading" property
        SetLoadingState(true);
        StartCoroutine(LoadingRoutine());
    }

    private IEnumerator LoadingRoutine()
    {
        yield return new WaitForSeconds(startLoadingDelay);
        // Optional: Set initial state
        loadingBar.fillAmount = 0f;
        if (loadingText != null)
        {
            loadingText.text = "Loading...";
        }

        // Divide the loading time into random steps
        for (int i = 0; i < steps; i++)
        {
            // Calculate random progress increment for this step
            float randomProgress = Random.Range(0.1f, 0.3f);
            float targetProgress = Mathf.Clamp01(progress + randomProgress); // Ensure it doesn't exceed 1

            // Calculate time for this step (proportionate to total time)
            float stepDuration = totalLoadingTime * 0.75f / steps; // Use 75% of total time for steps

            // Lerp the progress over the step duration
            float stepElapsed = 0f;
            while (stepElapsed < stepDuration)
            {
                stepElapsed += Time.deltaTime;
                elapsedTime += Time.deltaTime;
                progress = Mathf.Lerp(progress, targetProgress, stepElapsed / stepDuration);
                loadingBar.fillAmount = progress;
                yield return null;
            }
        }

        // Final step to complete the loading process
        float finalDuration = totalLoadingTime * 0.25f; // Use the remaining 25% of time
        float finalElapsed = 0f;
        while (finalElapsed < finalDuration)
        {
            finalElapsed += Time.deltaTime;
            elapsedTime += Time.deltaTime;
            progress = Mathf.Lerp(progress, 1f, finalElapsed / finalDuration);
            loadingBar.fillAmount = progress;
            yield return null;
        }

        // Optional: Finalize loading text
        if (loadingText != null)
        {
            loadingText.text = "Complete!";
        }

        // Simulate loading completion
        OnLoadingComplete();
    }

    private void OnLoadingComplete()
    {
        loadingBar.enabled = false;
        loadingText.text = null;
        canStartCountdown = true;
    }


    private void Update()
    {
        // Check if both players are ready
        if (AllPlayersReady() && canStartCountdown)
        {
            canStartCountdown = false; // Prevent multiple 
            FadeOutLoadingScreen(); // Begin fading out the loading screen
            view.RPC("StartCountDown", RpcTarget.All); // Start the countdown on all clients
        }
    }

    /// <summary>
    /// Checks if all players in the room have the IsReady property set to true.
    /// </summary>
    private bool AllPlayersReady()
    {
        foreach (var player in PhotonNetwork.PlayerList)
        {
            if (player.CustomProperties.TryGetValue("IsReady", out object isReady))
            {
                if (!(bool)isReady)
                    return false; // A player is not ready
            }
            else
            {
                return false; // IsReady property not found for a player
            }
        }
        return true; // All players are ready
    }

    [PunRPC]
    void StartCountDown()
    {
        StartCoroutine(CountdownRoutine());
    }

    private IEnumerator CountdownRoutine()
    {
        for (int i = 3; i > 0; i--)
        {
            countdownText.text = i.ToString();
            yield return new WaitForSeconds(1f);
        }
        countdownText.text = "GO!";
        countdownText.alpha = 1f; // Ensure it's visible


        // Add a button-like scale effect
        LeanTween.scale(countdownText.gameObject, Vector3.one * 1.2f, 0.3f)
            .setEase(LeanTweenType.easeOutCubic)
            .setOnComplete(() =>
            {
                // Scale back to normal size
                LeanTween.scale(countdownText.gameObject, Vector3.one, 0.3f)
                    .setEase(LeanTweenType.easeInCubic);
            });

        // Wait for the "GO!" animation to complete
        yield return new WaitForSeconds(0.6f);

        // Fade out the "GO!" text
        LeanTween.value(countdownText.gameObject, 1f, 0f, 0.3f)
            .setOnUpdate((float alpha) =>
            {
                countdownText.alpha = alpha; // Adjust transparency
            })
            .setOnComplete(() =>
            {
                // Clear the text after fading out
                countdownText.text = "";
            });

        SetLoadingState(false);
    }



    public void FadeOutLoadingScreen()
    {
        LeanTween.value(backgroundImage.gameObject, 1f, 0f, backgroundFadeDuration)
            .setOnUpdate((float value) => {
                Color color = backgroundImage.color;
                color.a = value;
                backgroundImage.color = color;
            })
            .setEase(LeanTweenType.easeInOutQuad);
    }


    

    public static bool IsLoading()
    {
        // Check if the room has the "IsLoading" property
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("IsLoading", out object isLoadingValue))
        {
            // Return the value cast as a boolean
            return (bool)isLoadingValue;
        }

        // Default to false if the property is not set
        return false;
    }

    public void SetLoadingState(bool isLoading)
    {
        Hashtable properties = new Hashtable
    {
        { "IsLoading", isLoading }
    };
        PhotonNetwork.CurrentRoom.SetCustomProperties(properties);
    }


}
