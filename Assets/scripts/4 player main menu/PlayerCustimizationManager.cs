using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
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
    [SerializeField] bool colorSelectionUIEabled = false;
    [SerializeField] float colorSelectionAnimationDuration;
    [SerializeField] Vector3 playerDisplayInactiveScale;

    /* VERY IMPORTANT!!!!!
     * The only way any of this code leads to having the right gun and or color for the player is due to the 
     * player preferences manager. Do not forget this exists, it is extremely important. 
     */
    private void Awake()
    {
        Player newPlayer = PhotonNetwork.LocalPlayer;
        Debug.Log("New Player Joined Lobby");
        AssignDisplayArea(newPlayer);
        view.RPC("UpdateDisplayArea", RpcTarget.AllBuffered, newPlayer.ActorNumber);
        if(PhotonNetwork.IsMasterClient)
        {
            leftMapSelectArrow.SetActive(true);
            rightMapSelectArrow.SetActive(true);
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


    #region Color Selction
    public void ArrowPress(int direction)
    {
        currentColorIndex += direction;
        if(currentColorIndex >= colors.Count) 
        {
            currentColorIndex = 0;
        }

        else if(currentColorIndex < 0) 
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
        if(!colorSelectionUIEabled) 
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

        colorSelectionUIEabled = !colorSelectionUIEabled;
    }

    void AnimateColorSelectionUI(float finalArrowScale, Vector3 finalPlayerDisplayScale)
    {
        LeanTween.scale(playerDisplay.gameObject, finalPlayerDisplayScale, colorSelectionAnimationDuration).setEaseInOutBounce();
        LeanTween.scaleX(leftArrow.gameObject, finalArrowScale, colorSelectionAnimationDuration);
        LeanTween.scaleX(rightArrow.gameObject, finalArrowScale, colorSelectionAnimationDuration);
    }

    void ActivateColorSelectionUI()
    {

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

    [PunRPC]
    public void UpdateDisplayArea(int playerActorNumber)
    {
        playerUsernameTags[playerActorNumber].GetComponent<TMP_Text>().text = "User: " + playerActorNumber; 
    }

    #endregion

    #region Ready Up

    public void OnReadyButtonClick()
    {
        isReady = !isReady;
        if(isReady)
        {
            readyButtonText.text = "Cancel";
        }
        else
        {
            readyButtonText.text = "Ready";
        }
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
        // Double check we're in loading state to prevent any race conditions
        

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

    #endregion
}
