using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using System.Linq;

public class RematchManager : MonoBehaviourPunCallbacks
{
    private bool wantsRematch = false;
    private Toggle playerToggle; // This player's toggle
    [SerializeField] PhotonView view;
    public Sprite[] colors;
    public Image rematchAvatar;
    public Toggle[] rematchToggleList; // Array of toggles for all players


    private void Start()
    {
        AssignRematchToggles();

        // Iterate through all players and set their avatars
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            view.RPC("SetRematchAvatar", RpcTarget.All, player.ActorNumber);
        }
    }

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.ContainsKey("StartRematch"))
        {
            bool startRematch = (bool)propertiesThatChanged["StartRematch"];
            Debug.Log($"StartRematch property changed: {startRematch}");

            if (startRematch)
            {
                ResetScene();
            }
        }
    }

    public void ResetScene()
    {
        // ALL clients will trigger the scene reset
        view.RPC("RPC_ResetScene", RpcTarget.All);
    }

    [PunRPC]
    private void RPC_ResetScene()
    {
        // Ensure gameOver flag is reset before scene reload
        if (GameManager.Instance != null)
        {
            GameManager.Instance.Reset();
        }

        // Destroy all existing network objects
        PhotonNetwork.DestroyAll();

        // Disconnect and reconnect to ensure a clean slate
        if (PhotonNetwork.IsMessageQueueRunning)
        {
            PhotonNetwork.IsMessageQueueRunning = false;
        }

        // Completely reload the current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);

        // Reconnect to Photon Network after scene load
        StartCoroutine(ReconnectToPhotonNetwork());
    }


    private IEnumerator ReconnectToPhotonNetwork()
    {
        // Wait a frame to ensure scene is loaded
        yield return null;

        // Rejoin the room
        PhotonNetwork.JoinRoom(PhotonNetwork.CurrentRoom.Name);

        // Re-enable message queue
        PhotonNetwork.IsMessageQueueRunning = true;

        // Reset game-specific states
        ResetGameState();
    }

    private void ResetGameState()
    {
        // Reset any specific game state here
        // For example, reset player positions, scores, etc.
        GameManager.Instance.Reset();
        wantsRematch = false;
        AssignRematchToggles();
        view.RPC("SetRematchAvatar", RpcTarget.All);
    }

    public void SetRematchFlagForPlayer()
    {
        wantsRematch = playerToggle.isOn;

        // Update the player's custom properties
        Hashtable playerProperties = new Hashtable { { "Rematch", wantsRematch } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);

        // Sync toggle state visually across all clients
        view.RPC("SyncToggleState", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber, wantsRematch);

        // Add a slight delay before checking rematch count
        StartCoroutine(DelayedCheckRematchCount());
    }

    private IEnumerator DelayedCheckRematchCount()
    {
        yield return new WaitForSeconds(0.1f); // Small delay to allow properties to sync
        CheckRematchCount();
    }

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

        // If all players want a rematch, start the game
        if (numRematchPlayers == PhotonNetwork.CurrentRoom.PlayerCount)
        {
            Debug.Log("All players agreed to rematch. Setting StartRematch property...");
            PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable { { "StartRematch", true } });
        }
    }

    [PunRPC]
    public void SetRematchAvatar(int actorNumber)
    {
        // Find the player by actor number
        Player targetPlayer = PhotonNetwork.PlayerList.FirstOrDefault(p => p.ActorNumber == actorNumber);

        if (targetPlayer != null)
        {
            // Find the correct toggle for this player
            int playerIndex = GetPlayerIndex(actorNumber);

            if (playerIndex >= 0 && playerIndex < rematchToggleList.Length)
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
                rematchToggleList[playerIndex].transform.GetChild(0).GetComponent<Image>().sprite = colors[(int)colorChoice];

                Debug.Log($"Setting avatar for player {targetPlayer.NickName} (Actor {actorNumber}) to color {colorChoice}");
            }
        }
    }


    [PunRPC]
    private void SyncToggleState(int playerActorNumber, bool isOn)
    {
        // Find the player toggle based on actor number
        int index = GetPlayerIndex(playerActorNumber);
        if (index >= 0 && index < rematchToggleList.Length)
        {
            rematchToggleList[index].isOn = isOn;
        }
    }

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

    private void AssignRematchToggles()
    {
        int playerIndex = GetPlayerIndex(PhotonNetwork.LocalPlayer.ActorNumber);

        if (playerIndex >= 0 && playerIndex < rematchToggleList.Length)
        {
            for (int i = 0; i < rematchToggleList.Length; i++)
            {
                // Allow interaction only with the player's own toggle
                rematchToggleList[i].interactable = (i == playerIndex);
            }

            playerToggle = rematchToggleList[playerIndex];
            rematchAvatar = playerToggle.transform.GetChild(0).GetComponent<Image>();

            // Add listener for this player's toggle
            playerToggle.onValueChanged.AddListener(delegate { SetRematchFlagForPlayer(); });
        }
    }

    private void LogRoomProperties()
    {
        foreach (var key in PhotonNetwork.CurrentRoom.CustomProperties.Keys)
        {
            Debug.Log($"Room Property - Key: {key}, Value: {PhotonNetwork.CurrentRoom.CustomProperties[key]}");
        }
    }
}
