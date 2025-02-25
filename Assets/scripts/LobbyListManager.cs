using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;

public class LobbyListManager : MonoBehaviourPunCallbacks
{
    public Transform lobbyListParent;
    public GameObject lobbyEntryPrefab;
    private List<GameObject> lobbyEntries = new List<GameObject>();

    private void Start()
    {
        if (PhotonNetwork.IsConnectedAndReady)
        {
            Debug.Log("Connected and ready. Joining lobby...");
            PhotonNetwork.JoinLobby();
        }
        else
        {
            Debug.Log("Not connected yet. Will join lobby when connected to master server...");
            // Register for the OnConnectedToMaster event
            PhotonNetwork.AddCallbackTarget(this);
        }
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("LobbyListManager: Connected to master server. Joining lobby...");
        PhotonNetwork.JoinLobby();
    }

    private void OnDestroy()
    {
        // Important: Remove this callback when the object is destroyed
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        // Clear previous entries
        foreach (GameObject entry in lobbyEntries)
        {
            Destroy(entry);
        }
        lobbyEntries.Clear();

        // Loop through all available rooms
        foreach (RoomInfo room in roomList)
        {
            if (room.RemovedFromList) continue;
            GameObject entry = Instantiate(lobbyEntryPrefab, lobbyListParent);
            LobbyEntry entryScript = entry.GetComponent<LobbyEntry>();

            // Pass method reference with room name parameter
            entryScript.SetRoomInfo(
                room.CustomProperties["LobbyName"].ToString(),
                room.PlayerCount,
                room.MaxPlayers,
                room.CustomProperties["LobbyCode"].ToString(),
                JoinRoom  // Pass method reference directly
            );

            lobbyEntries.Add(entry);
        }
    }

    public void JoinRoom(string roomName)
    {
        if (PhotonNetwork.IsConnectedAndReady)
        {
            Debug.Log($"Attempting to join room: {roomName}");
            PhotonNetwork.JoinRoom(roomName);
        }
        else
        {
            Debug.LogError("Cannot join room. Not connected to Photon.");
        }
    }
}