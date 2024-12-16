using Photon.Pun;
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
        PhotonNetwork.LeaveRoom(true);
    }
}
