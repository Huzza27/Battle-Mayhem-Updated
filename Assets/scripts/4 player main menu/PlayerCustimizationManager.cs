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
    public GameObject leftMapSelectArrow, rightMapSelectArrow;

    [Header("Ready Up")]
    public LevelLoader LevelLoader;
    public Button readyButton;
    public TextMeshProUGUI readyButtonText;
    public GameManager gameManager;

    private bool isReady = false;
    private bool isLoadingGame = false;
    bool canStartGame = false;

    [Header("Username Tags")]
    public Dictionary<int, GameObject> playerUsernameTags = new Dictionary<int, GameObject>();
    public GameObject[] usernameTags;
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
        ResetCustomizationManagerState();

        // Initialize username tags for all existing players
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            AssignDisplayArea(player);
            view.RPC("UpdateDisplayArea", RpcTarget.All, player.ActorNumber, player.NickName);
        }

        if (PhotonNetwork.IsMasterClient)
        {
            leftMapSelectArrow.SetActive(true);
            rightMapSelectArrow.SetActive(true);
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        // When a new player joins, update their display for everyone
        AssignDisplayArea(newPlayer);
        view.RPC("UpdateDisplayArea", RpcTarget.All, newPlayer.ActorNumber, newPlayer.NickName);

        // Send existing players' info to the new player
        foreach (Player existingPlayer in PhotonNetwork.PlayerList)
        {
            if (existingPlayer != newPlayer)
            {
                view.RPC("UpdateDisplayArea", RpcTarget.All, existingPlayer.ActorNumber, existingPlayer.NickName);
            }
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        // Clear the username tag when a player leaves
        if (playerUsernameTags.ContainsKey(otherPlayer.ActorNumber))
        {
            view.RPC("ClearDisplayArea", RpcTarget.All, otherPlayer.ActorNumber);
        }
    }

    private void Update()
    {
        if (canStartGame && !isLoadingGame)
        {
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

    #region Reset State
    public void ResetCustomizationManagerState()
    {
        // Reset ready and loading states
        isReady = false;
        isLoadingGame = false;
        canStartGame = false;

        // Reset UI elements
        if (readyButtonText != null) readyButtonText.text = "Ready";
        if (readyButton != null) readyButton.interactable = true;

        if (playerDisplay != null) playerDisplay.sprite = null;
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
    void AssignDisplayArea(Player player)
    {
        if (!playerUsernameTags.ContainsKey(player.ActorNumber) &&
            (player.ActorNumber - 1) < usernameTags.Length)
        {
            playerUsernameTags.Add(player.ActorNumber, usernameTags[player.ActorNumber - 1]);
        }
    }

    [PunRPC]
    public void UpdateDisplayArea(int playerActorNumber, string playerNickname)
    {
        if (playerUsernameTags.ContainsKey(playerActorNumber))
        {
            TextMeshProUGUI usernameText = playerUsernameTags[playerActorNumber].GetComponent<TextMeshProUGUI>();
            if (usernameText != null)
            {
                usernameText.text = $"User: {playerNickname}";
            }
        }
    }

    [PunRPC]
    public void ClearDisplayArea(int playerActorNumber)
    {
        if (playerUsernameTags.ContainsKey(playerActorNumber))
        {
            TextMeshProUGUI usernameText = playerUsernameTags[playerActorNumber].GetComponent<TextMeshProUGUI>();
            if (usernameText != null)
            {
                usernameText.text = "";
            }
            playerUsernameTags.Remove(playerActorNumber);
        }
    }
    #endregion

    #region Ready Up
    public void OnReadyButtonClick()
    {
        isReady = !isReady;
        readyButtonText.text = isReady ? "Cancel" : "Ready";
        SetReadyState(isReady);
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
}