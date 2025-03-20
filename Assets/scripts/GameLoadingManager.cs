using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using UnityEngine.UI;
using TMPro;
using Photon.Realtime;

public class GameLoadingManager : MonoBehaviour
{
    [SerializeField] Image backgroundImage;
    [SerializeField] float backgroundFadeDuration = 5f;
    [SerializeField] private TextMeshProUGUI countdownText;
    [SerializeField] private float countdownDuration = 1f;
    [SerializeField] private PhotonView view;

    [Header("UI References")]
    [SerializeField] private Image loadingBar;
    [SerializeField] private TextMeshProUGUI loadingText;

    [Header("Settings")]
    [SerializeField] private float totalLoadingTime = 4f;
    [SerializeField] private int steps = 3;
    [SerializeField] float startLoadingDelay = 5f;

    private float elapsedTime = 0f;
    private float progress = 0f;
    private bool canStartCountdown = false;

    private void Awake()
    {
        ResetGameLoadingManagerState();

        GameManager.OnGameReset += ResetGameLoadingManagerState;
    }

    public void ResetGameLoadingManagerState()
    {
        progress = 0f;
        elapsedTime = 0f;

        if (loadingBar != null)
        {
            loadingBar.fillAmount = 0f;
            loadingBar.enabled = true;
        }

        if (loadingText != null)
        {
            loadingText.text = "Loading...";
        }

        if (countdownText != null)
        {
            countdownText.text = "";
            countdownText.alpha = 1f;
            LeanTween.cancel(countdownText.gameObject);
            countdownText.transform.localScale = Vector3.one;
        }

        if (backgroundImage != null)
        {
            Color color = backgroundImage.color;
            color.a = 1f;
            backgroundImage.color = color;
        }

        SetPlayerLoadingState(true);
        SetLobbyLoadingState(true);
        StartCoroutine(LoadingRoutine());
    }

    private IEnumerator LoadingRoutine()
    {
        yield return new WaitForSeconds(startLoadingDelay);

        loadingBar.fillAmount = 0f;
        loadingText.text = "Loading...";

        for (int i = 0; i < steps; i++)
        {
            float randomProgress = Random.Range(0.1f, 0.3f);
            float targetProgress = Mathf.Clamp01(progress + randomProgress);
            float stepDuration = totalLoadingTime * 0.75f / steps;

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

        float finalDuration = totalLoadingTime * 0.25f;
        float finalElapsed = 0f;
        while (finalElapsed < finalDuration)
        {
            finalElapsed += Time.deltaTime;
            elapsedTime += Time.deltaTime;
            progress = Mathf.Lerp(progress, 1f, finalElapsed / finalDuration);
            loadingBar.fillAmount = progress;
            yield return null;
        }

        loadingText.text = "Complete!";
        OnLoadingComplete();
    }

    private void OnLoadingComplete()
    {
        SetPlayerLoadingState(false);
        loadingBar.enabled = false;
        loadingText.text = null;
        canStartCountdown = true;

        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(WaitForAllPlayers());
        }
    }

    private IEnumerator WaitForAllPlayers()
    {
        while (!CheckIfAllPlayersAreLoaded())
        {
            yield return new WaitForSeconds(1f);
        }

        Debug.Log(" All players have loaded. Starting game...");
        view.RPC("StartCountDown", RpcTarget.All);
    }

    private bool CheckIfAllPlayersAreLoaded()
    {
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (!player.CustomProperties.TryGetValue("IsLoading", out object isLoading) || (bool)isLoading)
            {
                Debug.Log($" Player {player.NickName} is still loading...");
                return false;
            }
        }

        Debug.Log(" All players are loaded!");
        return true;
    }

    private void Update()
    {
        if (canStartCountdown && CheckIfAllPlayersAreLoaded())
        {
            canStartCountdown = false;
            FadeOutLoadingScreen();
            view.RPC("StartCountDown", RpcTarget.All);
        }
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
        countdownText.alpha = 1f;

        LeanTween.scale(countdownText.gameObject, Vector3.one * 1.2f, 0.3f)
            .setEase(LeanTweenType.easeOutCubic)
            .setOnComplete(() =>
            {
                LeanTween.scale(countdownText.gameObject, Vector3.one, 0.3f)
                    .setEase(LeanTweenType.easeInCubic);
            });

        yield return new WaitForSeconds(0.6f);

        LeanTween.value(countdownText.gameObject, 1f, 0f, 0.3f)
            .setOnUpdate((float alpha) =>
            {
                countdownText.alpha = alpha;
            })
            .setOnComplete(() =>
            {
                countdownText.text = "";
                SetLobbyLoadingState(false);
            });
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

    public void SetPlayerLoadingState(bool isLoading)
    {
        Hashtable properties = new Hashtable
        {
            { "IsLoading", isLoading }
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(properties);
        Debug.Log($" {PhotonNetwork.LocalPlayer.NickName} set IsLoading to {isLoading}");
    }

    public void SetLobbyLoadingState(bool isLoading)
    {
        Hashtable properties = new Hashtable
        {
            { "IsLoading", isLoading }
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(properties);
        Debug.Log($" {PhotonNetwork.LocalPlayer.NickName} set IsLoading to {isLoading}");
    }

    /// <summary>
    /// Checks if the game is currently in a loading state.
    /// </summary>
    public static bool IsLoading()
    {
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("IsLoading", out object isLoadingValue))
        {
            return (bool)isLoadingValue;
        }
        return false; // Default to false if the property is not set
    }
}
