using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RoomManager : MonoBehaviourPunCallbacks
{
    public static RoomManager Instance;
    private const string OTHER_PLAYER_FULLY_LEFT_ERROR = "The other player has left the game";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ShowMessageToPlayer(string message)
    {
        Debug.Log(message);
        // TODO: Hook up to a UI pop-up manager to display messages
        PopupManager.Instance.ShowMessage(message);
    }

    public void EndGameForRemainingPlayer()
    {

        // End the game and return to the Lobby Select
        LeaveRoom();
        SceneManager.LoadScene("Lobby");
        PopupManager.Instance.ShowMessage(OTHER_PLAYER_FULLY_LEFT_ERROR);
    }

    public void ReturnToMainMenu()
    {
        PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene("MainMenu");
    }

    public void HandlePlayerReconnect()
    {
        ShowMessageToPlayer("The other player has reconnected!");
    }

    public void HandleReconnectTimeout()
    {
        ShowMessageToPlayer("The other player failed to reconnect. The room will now close.");
        EndGameForRemainingPlayer();
    }

    public void LeaveRoom()
    {
        Debug.Log($"Leaving Room - IsMasterClient: {PhotonNetwork.IsMasterClient}, Player: {PhotonNetwork.LocalPlayer.ActorNumber}");

        // Clean up player state before leaving
        CleanupPlayerState();

        // Leave the room
        PhotonNetwork.LeaveRoom(true);
    }

    private void CleanupPlayerState()
    {
        // Find and reset all player objects
        var players = FindObjectsOfType<Movement>();
        foreach (var player in players)
        {
            if (player.photonView.IsMine)
            {
                player.ResetMovementState();
            }
        }

        // Find and reset spawn manager if it exists
        var spawnManager = FindObjectOfType<SpawnPlayers>();
        if (spawnManager != null)
        {
            spawnManager.ResetSpawnPlayersState();
        }
    }

    public override void OnJoinedRoom()
    {
        Debug.Log($"Joined Room - IsMasterClient: {PhotonNetwork.IsMasterClient}, Player: {PhotonNetwork.LocalPlayer.ActorNumber}");

        // Set initial player properties
        var properties = new ExitGames.Client.Photon.Hashtable
        {
            { "IsInitialized", false },
            { "IsReady", false }
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(properties);
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        Debug.Log($"Master client switched to player {newMasterClient.ActorNumber}");

        // Reset initialization state for the new master client
        if (PhotonNetwork.LocalPlayer.ActorNumber == newMasterClient.ActorNumber)
        {
            var properties = new ExitGames.Client.Photon.Hashtable
            {
                { "IsInitialized", false }
            };
            PhotonNetwork.LocalPlayer.SetCustomProperties(properties);
        }
    }
}
