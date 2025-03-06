using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public static NetworkManager Instance;

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

    public override void OnJoinedRoom()
    {
        // Reset initialization state when joining room
        var props = new ExitGames.Client.Photon.Hashtable
        {
            { "IsInitialized", false },
            { "IsReady", false } // Make sure to reset IsReady here as well
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
        PhotonNetwork.LoadLevel("CharacterSelect");
    }

    // This method is important for clearing player properties
    public override void OnLeftRoom()
    {
        Debug.Log("Left Room - Clearing player properties");
        // Reset all player properties when leaving a room
        var props = new ExitGames.Client.Photon.Hashtable
        {
            { "IsInitialized", false },
            { "IsReady", false }
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log($"{otherPlayer.NickName} has left the room.");
        // Check if the player left intentionally
        if (PhotonNetwork.CurrentRoom != null && PhotonNetwork.CurrentRoom.PlayerCount == 1)
        {
            RoomManager.Instance.EndGameForRemainingPlayer();
        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        // Make sure RoomManager exists before accessing it
        if (RoomManager.Instance != null)
        {
            RoomManager.Instance.ShowMessageToPlayer("An error occurred and you were disconnected from the lobby!");
        }
        else
        {
            Debug.LogError("RoomManager instance is null when handling disconnect.");
        }
    }
}