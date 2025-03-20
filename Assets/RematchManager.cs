using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using System.Linq;
using System.Collections.Generic;

/// <summary>
/// Manages the rematch functionality in a multiplayer game, handling player votes and scene reloading.
/// </summary>
public class RematchManager : MonoBehaviourPunCallbacks
{
    #region Variables
    // Local state tracking
    private bool wantsRematch = false;
    private Toggle playerToggle;

    // References
    [SerializeField] PhotonView view;
    [SerializeField] private GameObject rematchUIContainer;
    public Sprite[] colors;
    public Image rematchAvatar;
    public List<Toggle> rematchToggleList;
    public TriggerEndGame endGameScript;
    #endregion

    #region Initialization

    private void Awake()
    {
        ResetManager();
    }

    private void OnEnable()
    {
        GameManager.OnGameReset += ResetManager;
    }

    private void OnDisable()
    {
        GameManager.OnGameReset -= ResetManager;

    }
    /// <summary>
    /// Initializes the rematch UI and sets up the player toggles.
    /// </summary>
    private void Start()
    {
        view.RPC("RematchLayoutBasedOnPlayerCount", RpcTarget.All);
        AssignTogglesAndAvatarsRematchUI();
    }

    /// <summary>
    /// Resets the scene by reassigning toggles and setting avatars for all players.
    /// </summary>
    public void AssignTogglesAndAvatarsRematchUI()
    {
        AssignRematchToggles();

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            view.RPC("SetRematchAvatar", RpcTarget.All, player.ActorNumber);
        }
    }

    /// <summary>
    /// Called when this player joins or rejoins the room.
    /// Updates the rematch UI based on current player count and assigns toggles.
    /// </summary>
    public override void OnJoinedRoom()
    {
        // Update the rematch layout based on current player count
        view.RPC("RematchLayoutBasedOnPlayerCount", RpcTarget.All);

        // Reassign toggles for this player
        AssignRematchToggles();

        Debug.Log($"Player {PhotonNetwork.LocalPlayer.NickName} has joined/rejoined the room after rematch");
    }
    #endregion

    #region Reset
    /// <summary>
    /// Resets the rematch manager state completely, clearing all toggle states and local flags.
    /// Called during Awake to ensure clean state at initialization.
    /// </summary>
    public void ResetManager()
    {
        endGameScript.ResetAnimators();
        // Reset local state
        wantsRematch = false;

        // Reset all toggle UI elements
        foreach (Toggle toggle in rematchToggleList)
        {
            if (toggle != null)
            {
                toggle.isOn = false;
            }
        }

        // Reset player custom properties related to rematch if in a room
        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
        {
            Hashtable props = new Hashtable { { "Rematch", false } };
            PhotonNetwork.LocalPlayer.SetCustomProperties(props);
        }

        Debug.Log("RematchManager has been reset");
    }
    #endregion

    #region UI Management
    /// <summary>
    /// Shows the rematch UI when the game ends.
    /// Resets local rematch flags and ensures UI is visible for all players.
    /// </summary>
    public void ShowRematchUI()
    {
        // Reset local rematch flags
        wantsRematch = false;


        // Make sure all players have the UI showing
        view.RPC("EnsureRematchUIVisible", RpcTarget.Others);
    }

    /// <summary>
    /// RPC called to ensure the rematch UI is visible for all players.
    /// Resets toggle states to ensure a clean UI state.
    /// </summary>
    [PunRPC]
    private void EnsureRematchUIVisible()
    {
        if (rematchUIContainer != null)
        {
            rematchUIContainer.SetActive(true);
        }
    }

    /// <summary>
    /// RPC called to update the rematch UI layout based on the number of players in the room.
    /// Activates only the necessary toggles for the current player count.
    /// </summary>
    [PunRPC]
    public void RematchLayoutBasedOnPlayerCount()
    {
        int numPlayers = PhotonNetwork.CurrentRoom.PlayerCount;

        // First deactivate all toggles
        foreach (Toggle toggle in rematchToggleList)
        {
            toggle.gameObject.SetActive(false);
        }

        // Then activate only the needed ones
        for (int i = 0; i < numPlayers && i < rematchToggleList.Count; i++)
        {
            rematchToggleList[i].gameObject.SetActive(true);
            rematchToggleList[i].isOn = false;  // Reset toggle state
        }
    }

    /// <summary>
    /// Assigns the appropriate toggle to each player and sets up listeners.
    /// Each player can only interact with their own toggle.
    /// </summary>
    private void AssignRematchToggles()
    {
        int playerIndex = GetPlayerIndex(PhotonNetwork.LocalPlayer.ActorNumber);

        if (playerIndex >= 0 && playerIndex < rematchToggleList.Count)
        {
            // First remove any existing listeners
            foreach (Toggle toggle in rematchToggleList)
            {
                toggle.onValueChanged.RemoveAllListeners();
            }

            for (int i = 0; i < rematchToggleList.Count; i++)
            {
                // Allow interaction only with the player's own toggle
                rematchToggleList[i].interactable = (i == playerIndex);
            }

            playerToggle = rematchToggleList[playerIndex];

            if (playerToggle.transform.GetChild(0) != null)
            {
                rematchAvatar = playerToggle.transform.GetChild(0).GetComponent<Image>();
            }

            // Add listener for this player's toggle
            playerToggle.onValueChanged.AddListener(delegate { SetRematchFlagForPlayer(); });
        }
    }
    #endregion

    #region Player Avatar Management
    /// <summary>
    /// RPC called to set the correct avatar image for each player's toggle.
    /// Uses the player's color choice from their custom properties.
    /// </summary>
    /// <param name="actorNumber">The actor number of the player to set the avatar for</param>
    [PunRPC]
    public void SetRematchAvatar(int actorNumber)
    {
        // Find the player by actor number
        Player targetPlayer = PhotonNetwork.PlayerList.FirstOrDefault(p => p.ActorNumber == actorNumber);

        if (targetPlayer != null)
        {
            // Find the correct toggle for this player
            int playerIndex = GetPlayerIndex(actorNumber);

            if (playerIndex >= 0 && playerIndex < rematchToggleList.Count)
            {
                // Get color choice
                object colorChoice = 0; // Default color
                if (targetPlayer.CustomProperties.TryGetValue("PlayerColor", out object playerColor))
                {
                    colorChoice = playerColor;
                }
                else
                {
                    // Fallback color selection
                    colorChoice = targetPlayer.ActorNumber == 1 ? 0 : 2;
                }

                // Set the avatar sprite for this player's toggle
                Transform imageTransform = rematchToggleList[playerIndex].transform.GetChild(0);
                if (imageTransform != null)
                {
                    Image avatarImage = imageTransform.GetComponent<Image>();
                    if (avatarImage != null && (int)colorChoice < colors.Length)
                    {
                        avatarImage.sprite = colors[(int)colorChoice];
                        Debug.Log($"Setting avatar for player {targetPlayer.NickName} (Actor {actorNumber}) to color {colorChoice}");
                    }
                }
            }
        }
    }

    /// <summary>
    /// Gets the index of a player in the PhotonNetwork.PlayerList array based on their actor number.
    /// Used to match players to their corresponding UI elements.
    /// </summary>
    /// <param name="actorNumber">The actor number to find</param>
    /// <returns>The index in the player list, or -1 if not found</returns>
    private int GetPlayerIndex(int actorNumber)
    {
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            if (PhotonNetwork.PlayerList[i].ActorNumber == actorNumber)
            {
                return i;
            }
        }
        return -1; // Return -1 if player is not found
    }
    #endregion

    #region Rematch Logic
    /// <summary>
    /// Updates the player's rematch preference when their toggle is changed.
    /// Syncs the state across all clients and checks if all players have agreed to rematch.
    /// </summary>
    public void SetRematchFlagForPlayer()
    {
        if(playerToggle == null)
        {
            Debug.LogWarning("Player toggle is null. Cannot set rematch flag.");
            return;
        }
        wantsRematch = playerToggle.isOn;

        // Update the player's custom properties
        Hashtable playerProperties = new Hashtable { { "Rematch", wantsRematch } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);

        // Sync toggle state visually across all clients
        view.RPC("SyncToggleState", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber, wantsRematch);

        // Check if all players want to rematch
        StartCoroutine(DelayedCheckRematchCount());
    }

    /// <summary>
    /// RPC called to synchronize toggle states across all clients.
    /// Ensures all players see the current rematch vote status.
    /// </summary>
    /// <param name="playerActorNumber">The actor number of the player whose toggle changed</param>
    /// <param name="isOn">The new state of the toggle</param>
    [PunRPC]
    private void SyncToggleState(int playerActorNumber, bool isOn)
    {
        // Find the player toggle based on actor number
        int index = GetPlayerIndex(playerActorNumber);
        if (index >= 0 && index < rematchToggleList.Count)
        {
            rematchToggleList[index].isOn = isOn;
        }
    }

    /// <summary>
    /// Coroutine that adds a small delay before checking the rematch count.
    /// This ensures all player properties have time to sync across the network.
    /// </summary>
    private IEnumerator DelayedCheckRematchCount()
    {
        yield return new WaitForSeconds(0.3f); // Slightly longer delay to ensure properties sync
        CheckRematchCount();
    }

    /// <summary>
    /// Checks if all players have agreed to a rematch.
    /// If everyone agrees, the master client reloads the scene.
    /// Only the master client performs this check.
    /// </summary>
    void CheckRematchCount()
    {
        // Only the MasterClient should perform the check
        if (!PhotonNetwork.IsMasterClient) return;

        int numRematchPlayers = 0;
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            object rematchFlag;
            if (player.CustomProperties.TryGetValue("Rematch", out rematchFlag) && (bool)rematchFlag)
            {
                numRematchPlayers++;
            }
        }

        Debug.Log($"Rematch Count: {numRematchPlayers}/{PhotonNetwork.CurrentRoom.PlayerCount}");

        // If all players want a rematch, reload the scene
        if (numRematchPlayers == PhotonNetwork.CurrentRoom.PlayerCount)
        {
            Debug.Log("All players agreed to rematch. Reloading scene...");

            // Reset player properties 
            foreach (Player player in PhotonNetwork.PlayerList)
            {
                Hashtable props = new Hashtable { { "Rematch", false } };
                player.SetCustomProperties(props);
            }

            view.RPC("StartRematch", RpcTarget.All);
        }
    }
    [PunRPC]
    // In RematchManager.cs, when starting a rematch
    public void StartRematch()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            // Clear winner property first
            Hashtable clearWinner = new Hashtable { { "Winner", null } };
            PhotonNetwork.CurrentRoom.SetCustomProperties(clearWinner);

            // Then set StartRematch property to trigger rematch
            Hashtable rematchProperties = new Hashtable { { "StartRematch", true } };
            PhotonNetwork.CurrentRoom.SetCustomProperties(rematchProperties);
        }

        endGameScript.ResetAnimators();

        // Trigger game reset
        GameManager.TriggerGameReset();
    }

    #endregion
}