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
    private bool canLoad = true;
    private int currentNumber = 3; 

    private void Update()
    {
        // Check if both players are ready
        if (AllPlayersReady() && !IsLoading() && canLoad)
        {
            canLoad = false; // Prevent multiple triggers
            SetLoadingState(true); // Set loading state to true
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

    public void SetLoadingState(bool isLoading)
    {
        Hashtable properties = new Hashtable
    {
        { "IsLoading", isLoading }
    };
        PhotonNetwork.CurrentRoom.SetCustomProperties(properties);
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


}
