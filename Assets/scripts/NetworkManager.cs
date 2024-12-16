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

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log($"{otherPlayer.NickName} has left the room.");

        // Check if the player left intentionally
        if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
        {
           RoomManager.Instance.EndGameForRemainingPlayer();
        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        RoomManager.Instance.ShowMessageToPlayer("An error occured and you were disconnected from the lobby!");
    }
}
