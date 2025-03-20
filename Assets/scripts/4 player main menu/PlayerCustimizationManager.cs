using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PlayerCustimizationManager : MonoBehaviourPunCallbacks
{
    [Header("UI References")]
    public TMP_InputField livesInput;
    public GameObject leftMapSelectArrow, rightMapSelectArrow;
    public GameObject heartImageContainer;

    [Header("Room Code")]
    public TextMeshProUGUI roomCodeText;

    [Header("Ready Up")]
    public LevelLoader LevelLoader;
    public Button readyButton;
    public TextMeshProUGUI readyButtonText;
    public GameManager gameManager;
    public Image usernameTag;
    public float animDuration = 0.4f;

    private bool isReady = false;
    private bool isLoadingGame = false;
    bool canStartGame = false;

    [Header("Username Tags")]
    public Dictionary<int, GameObject> playerUsernameTags = new Dictionary<int, GameObject>();
    public GameObject[] usernameTags;
    string userName;
    public PhotonView view;

    [Header("Color Selection")]
    public List<Sprite> colors = new List<Sprite>();
    public int currentColorIndex = 0;
    public Image playerDisplay;
    public Button changeColorButton;
    public TMP_Text buttonText;
    public GameObject leftArrow, rightArrow;

    [SerializeField] float originalArrowScale;
    [SerializeField] bool colorSelectionUIEnabled = false;
    [SerializeField] float colorSelectionAnimationDuration;
    [SerializeField] Vector3 playerDisplayInactiveScale;

    private void Awake()
    {
        if(GameManager.Instance != null)
        {
            GameManager.Instance.SetReferences(livesInput);
        }
        // Reset state every time the scene loads
        ResetCustomizationManagerState();

        if (PhotonNetwork.IsMasterClient)
        {
            SetRoomCodeText();
            heartImageContainer.SetActive(true);
        }

        Player newPlayer = PhotonNetwork.LocalPlayer;
        Debug.Log("New Player Joined Lobby");

        // Set the Steam username as a custom property for this player
        userName = SteamManager.GetSteamUserName();

        // Store the username as a custom property
        Hashtable playerProps = new Hashtable();
        playerProps.Add("Username", userName);
        // Make sure the player always starts as not ready when joining
        playerProps.Add("Ready", false);
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProps);

        // Assign display area for this player
        AssignDisplayArea(newPlayer);

        // Update all players' display areas
        view.RPC("UpdateAllDisplayAreas", RpcTarget.AllBuffered);

        if (PhotonNetwork.IsMasterClient)
        {
            leftMapSelectArrow.SetActive(true);
            rightMapSelectArrow.SetActive(true);
        }
    }

    private void OnEnable()
    {
        // Reset ready state on scene activation
        isReady = false;
        if (readyButtonText != null)
        {
            readyButtonText.text = "Ready";
        }

        // Reset player's ready property when scene is enabled
        Hashtable playerProperties = new Hashtable
        {
            { "Ready", false }
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);
    }

    private void Update()
    {
        if(!readyButton.interactable)
        {
            if(PhotonNetwork.PlayerList.Length > 1)
            {
                readyButton.interactable = true;
            }
            return;
        }
        if (canStartGame && !isLoadingGame && isReady)
        {
            Debug.Log("canStartGame is true and isLoadingGame is false");
            view.RPC("StartGameNetwork", RpcTarget.All);
            isLoadingGame = true;
        }

        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }



        if (CountReadyPlayers() == PhotonNetwork.PlayerList.Length && !canStartGame)
        {
            canStartGame = true;
        }
    }

    void SetRoomCodeText()
    {
        roomCodeText.transform.parent.parent.gameObject.SetActive(true);
        // Display room code
        if (roomCodeText != null)
        {
            string roomCode = PhotonNetwork.CurrentRoom.Name;
            roomCodeText.text = roomCode;
            Debug.Log("Room code set to: " + roomCode);
        }
        else
        {
            Debug.LogError("roomCodeText reference is missing!");
        }
    }

    public void CopyRoomCode()
    {
        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
        {
            string roomCode = PhotonNetwork.CurrentRoom.Name;
            GUIUtility.systemCopyBuffer = roomCode;
        }
    }

    public void CallSetLivesOnGameManager()
    {
        GameManager.Instance.SetLives();
    }

    #region Reset State

    public void ResetCustomizationManagerState()
    {
        // Reset ready and loading states
        isReady = false;
        isLoadingGame = false;
        canStartGame = false;

        if(PhotonNetwork.IsMasterClient)
        {
            readyButton.interactable = false;
        }

        // Reset UI elements
        if (readyButtonText != null) readyButtonText.text = "Ready";

        if (playerDisplay != null) playerDisplay.sprite = colors[0];
        if (buttonText != null) buttonText.text = "Change";

        if (leftMapSelectArrow != null) leftMapSelectArrow.SetActive(false);
        if (rightMapSelectArrow != null) rightMapSelectArrow.SetActive(false);

        colorSelectionUIEnabled = false;

        // Clear assigned usernames
        playerUsernameTags.Clear();

        // Reset Player Custom Properties
        Hashtable playerProperties = new Hashtable
        {
            { "Ready", false }
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);

        // Reset color selection
        currentColorIndex = 0;
        SetPlayerPreferences.SetPlayerColorChoice(currentColorIndex);

        Debug.Log("Customization Manager reset.");
    }

    #endregion

    #region Color Selection
    public void ArrowPress(int direction)
    {
        currentColorIndex += direction;
        if (currentColorIndex >= colors.Count)
        {
            currentColorIndex = 0;
        }
        else if (currentColorIndex < 0)
        {
            currentColorIndex = colors.Count - 1;
        }
        playerDisplay.sprite = colors[currentColorIndex];
    }

    void OnSelectColor()
    {
        Debug.Log("Setting player color choice to " + colors[currentColorIndex].name);
        SetPlayerPreferences.SetPlayerColorChoice(currentColorIndex);
    }

    public void OnChangeColorButtonPress()
    {
        if (!colorSelectionUIEnabled)
        {
            buttonText.text = "Submit";
            AnimateColorSelectionUI(originalArrowScale, playerDisplayInactiveScale);
        }
        else
        {
            OnSelectColor();
            buttonText.text = "Change";
            AnimateColorSelectionUI(0f, Vector3.one);
        }

        colorSelectionUIEnabled = !colorSelectionUIEnabled;
    }

    void AnimateColorSelectionUI(float finalArrowScale, Vector3 finalPlayerDisplayScale)
    {
        LeanTween.scale(playerDisplay.gameObject, finalPlayerDisplayScale, colorSelectionAnimationDuration).setEaseInOutBounce();
        LeanTween.scaleX(leftArrow.gameObject, finalArrowScale, colorSelectionAnimationDuration);
        LeanTween.scaleX(rightArrow.gameObject, finalArrowScale, colorSelectionAnimationDuration);
    }

    #endregion

    #region Gun Selection

    public void SelectGun(int index)
    {
        SetPlayerPreferences.SetPlayerColorChoice(index);
    }

    #endregion

    #region Other Player Display
    void AssignDisplayArea(Player newPlayer)
    {
        if (!playerUsernameTags.ContainsKey(newPlayer.ActorNumber))
        {
            playerUsernameTags.Add(newPlayer.ActorNumber, usernameTags[newPlayer.ActorNumber - 1]);
        }
    }

    // This method updates all display areas with Steam names
    [PunRPC]
    public void UpdateAllDisplayAreas()
    {
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (!playerUsernameTags.ContainsKey(player.ActorNumber))
            {
                if (player.ActorNumber - 1 < usernameTags.Length)
                {
                    playerUsernameTags.Add(player.ActorNumber, usernameTags[player.ActorNumber - 1]);
                }
            }

            if (playerUsernameTags.ContainsKey(player.ActorNumber))
            {
                // Get the player's username from their custom properties
                string playerUsername = "Player";
                if (player.CustomProperties.ContainsKey("Username"))
                {
                    playerUsername = (string)player.CustomProperties["Username"];
                }

                // Get the username tag image component
                Image playerUsernameTag = playerUsernameTags[player.ActorNumber].transform.parent.GetComponent<Image>();

                // Update the display text with the player's actual username
                playerUsernameTags[player.ActorNumber].transform.parent.parent.gameObject.SetActive(true);
                playerUsernameTags[player.ActorNumber].GetComponent<TextMeshProUGUI>().text = playerUsername;

                // Apply animation based on player's ready status
                bool isPlayerReady = false;
                if (player.CustomProperties.ContainsKey("Ready"))
                {
                    isPlayerReady = (bool)player.CustomProperties["Ready"];
                }

                // Store the reference to the local player's username tag
                if (player.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
                {
                    usernameTag = playerUsernameTag;
                }

                // Animate the username tag position based on ready status
                AnimateUsernameDisplay(playerUsernameTag.gameObject, isPlayerReady);
            }
        }
    }

    #endregion

    #region Ready Up

    public void OnReadyButtonClick()
    {
        isReady = !isReady;
        readyButtonText.text = isReady ? "Cancel" : "Ready";
        SetReadyState(isReady);
        // Animation is now handled via the SetReadyState and OnPlayerPropertiesUpdate
    }

    public void AnimateUsernameDisplay(GameObject usernameTagObject, bool ready)
    {
        int finalYPos = 31; // Default position (not ready)
        if (ready)
        {
            finalYPos = -15; // Ready position
        }
        LeanTween.moveLocalY(usernameTagObject, finalYPos, animDuration).setEase(LeanTweenType.easeInCubic);
    }

    private void SetReadyState(bool ready)
    {
        Hashtable playerProperties = new Hashtable { { "Ready", ready } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);
    }

    [PunRPC]
    void StartGameNetwork()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.CurrentRoom.IsOpen = false;
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

    #endregion

    #region Photon Callbacks

    // Handle player joining the room
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        // Update all display areas when a new player joins
        view.RPC("UpdateAllDisplayAreas", RpcTarget.All);
    }

    // Called when any player's custom properties change
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        base.OnPlayerPropertiesUpdate(targetPlayer, changedProps);

        // If username or ready status changed, update display
        if (changedProps.ContainsKey("Username") || changedProps.ContainsKey("Ready"))
        {
            // Instead of calling an RPC, we can update the specific player's UI directly
            if (playerUsernameTags.ContainsKey(targetPlayer.ActorNumber))
            {
                // Get the ready status of the player
                bool isPlayerReady = false;
                if (targetPlayer.CustomProperties.ContainsKey("Ready"))
                {
                    isPlayerReady = (bool)targetPlayer.CustomProperties["Ready"];
                }

                // Get the username tag gameObject
                GameObject usernameTagObject = playerUsernameTags[targetPlayer.ActorNumber].transform.parent.gameObject;

                // Animate the username tag position
                AnimateUsernameDisplay(usernameTagObject, isPlayerReady);

                // If this is the local player, also update the ready button text
                if (targetPlayer.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
                {
                    readyButtonText.text = isPlayerReady ? "Cancel" : "Ready";
                    isReady = isPlayerReady;
                }
            }
        }
    }
    #endregion
}