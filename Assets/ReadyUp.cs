using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using TMPro;
using ExitGames.Client.Photon;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class ReadyUp : MonoBehaviourPunCallbacks
{
    public PhotonView view;
    public Button readyButton;
    public TextMeshProUGUI text;
    public TextMeshProUGUI readyButtonText;
    public GameManager gameManager;
    public SelectColor selectColor;

    private bool isReady = false;
    private bool isLoadingGame = false; // Add this to prevent multiple scene loads
    private bool arrowsVisible = false;

    [Header("Animation")]
    [SerializeField] private RectTransform uiElement;
    [SerializeField] private float targetYPosition = -2000f;
    [SerializeField] private float animationDuration = 1.5f;
    [SerializeField] private RectTransform movereadyLeftTransform;
    private Vector2 originalPosition;
    public RectTransform originalReadyButtonPos;

    public LevelLoader levelLoader;


    [Header("Arrow  Visiblity")]
    [SerializeField] GameObject[] arrows;

    private void Awake()
    {
        originalPosition = uiElement.anchoredPosition;
        gameManager.DiableLivesInputField();

        // Set initial "Ready" state for this player
        SetReadyState(false);

        
    }

    private void Update()
    {
        if (PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount > 1 && !arrowsVisible)
        {
            arrowsVisible = true;
            foreach (GameObject go in arrows)
            {
                go.SetActive(true);
            }
        }
    }



    public void OnClick()
    {
        // Close the player select wheel if open
        if (selectColor.isOpen)
        {
            selectColor.ToggleColorWheel();
        }

        // Toggle the ready state
        isReady = !isReady;
        SetReadyState(isReady);

        // Update UI animations
        if (isReady)
        {
            AnimateOutOfFrame();
            readyButtonText.text = "Cancel";
        }
        else
        {
            AnimateIntoFrame();
            readyButtonText.text = "Ready";
        }

        // Add a delay before checking if all players are ready
        Invoke(nameof(CheckAllPlayersReady), 0.1f);
    }

    private void SetReadyState(bool ready)
    {
        Hashtable playerProperties = new Hashtable { { "Ready", ready } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        // Check if the "Ready" property was updated
        if (changedProps.ContainsKey("Ready"))
        {
            Debug.Log($"{targetPlayer.NickName} updated Ready state to {changedProps["Ready"]}");
            Invoke(nameof(CheckAllPlayersReady), 0.1f); // Add a delay before checking readiness
        }
    }

    private void CheckAllPlayersReady()
    {
        if (isLoadingGame) return; // Prevent multiple scene loads

        bool allPlayersReady = true;
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (!player.CustomProperties.ContainsKey("Ready") || !(bool)player.CustomProperties["Ready"])
            {
                allPlayersReady = false;
                text.text = $"{CountReadyPlayers()}/{PhotonNetwork.CurrentRoom.PlayerCount}";
                break;
            }
        }

        // If all players are ready
        if (allPlayersReady)
        {
            text.text = $"{PhotonNetwork.CurrentRoom.PlayerCount}/{PhotonNetwork.CurrentRoom.PlayerCount}";

            // Set the game loading flag
            isLoadingGame = true;

            if (PhotonNetwork.IsMasterClient)
            {
                Debug.Log("All players are ready. Starting game...");
                gameManager.SetLives();

                // First, synchronize that we're about to start
                view.RPC("PrepareForGameStart", RpcTarget.All);

                // Wait a short moment to ensure all clients are prepared
                Invoke(nameof(InitiateGameStart), 0.5f);
            }
        }
    }

    [PunRPC]
    private void PrepareForGameStart()
    {
        // This ensures all clients know we're about to start
        levelLoader.LoadNextLevel();
        isLoadingGame = true;
    }

    private void InitiateGameStart()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            // Send the final start command to all clients
            view.RPC("StartGameNetwork", RpcTarget.All);
        }
    }

    [PunRPC]
    void StartGameNetwork()
    {
        // Double check we're in loading state to prevent any race conditions
        if (!isLoadingGame) return;

        // If you need to ensure scene loading synchronization, you can use:
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.CurrentRoom.IsOpen = false; // Prevent new players from joining mid-load
        }

        PhotonNetwork.LoadLevel("Game");
    }

    private int CountReadyPlayers()
    {
        int count = 0;
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (player.CustomProperties.ContainsKey("Ready") && (bool)player.CustomProperties["Ready"])
            {
                count++;
            }
        }
        return count;
    }

    public void AnimateOutOfFrame()
    {
        if (readyButton != null)
        {
            readyButton.interactable = false;
        }
        MoveGunAndHealthUIOutOfFrame();
        MoveReadyButtonLeft();
    }

    private void MoveGunAndHealthUIOutOfFrame()
    {
        LeanTween.moveY(uiElement, targetYPosition, animationDuration * 0.3f)
            .setEase(LeanTweenType.easeOutBounce)
            .setOnComplete(() => readyButton.interactable = true);
    }

    public void AnimateIntoFrame()
    {
        if (readyButton != null)
        {
            readyButton.interactable = false;
        }
        MoveGunAndHealthUIIntoFrame();
        MoveReadyButtonRight();
    }

    private void MoveGunAndHealthUIIntoFrame()
    {
        LeanTween.moveY(uiElement, originalPosition.y, animationDuration * 0.3f)
            .setEase(LeanTweenType.linear)
            .setOnComplete(() => readyButton.interactable = true);
    }

    private void MoveReadyButtonLeft()
    {
        LeanTween.moveX(readyButton.gameObject, movereadyLeftTransform.position.x, animationDuration * 0.3f)
            .setEase(LeanTweenType.linear);
    }

    private void MoveReadyButtonRight()
    {
        LeanTween.moveX(readyButton.gameObject, originalReadyButtonPos.position.x, animationDuration * 0.3f)
            .setEase(LeanTweenType.linear);
    }
}
