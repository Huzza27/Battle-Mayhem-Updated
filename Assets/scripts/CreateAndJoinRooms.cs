using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using TMPro; // Required for Custom Properties

public class CreateAndJoinRooms : MonoBehaviourPunCallbacks
{
    // UI Elements
    public GameObject settingsMenu; // The settings menu that appears when creating a room
    public InputField joinRoomInput;
    public TextMeshProUGUI roomNameDisplay;
    public TMP_Dropdown maxPlayersDropdown; // Dropdown for max player count (2-4)
    public Toggle privateToggle; // Toggle for Public/Private lobby

    private string lobbyCode;

    private void Awake()
    {
        PhotonNetwork.SendRate = 30;
        PhotonNetwork.SerializationRate = 30;

        if (!PhotonNetwork.IsConnected)
        {
            Debug.Log("Not connected to Photon. Connecting...");
            PhotonNetwork.ConnectUsingSettings();
        }
        else if (!PhotonNetwork.InLobby)
        {
            Debug.Log("Connected but not in lobby. Joining lobby...");
            PhotonNetwork.JoinLobby();
        }
        else
        {
            Debug.Log("Already connected and in lobby.");
        }
    }

    public void OpenSettingsMenu()
    {
        settingsMenu.SetActive(true); // Show the settings menu when "Create Room" is clicked
    }

    public void CloseSettingsMenu()
    {
        settingsMenu.SetActive(false);
    }

    public void CreateRoom()
    {
        // Ensure we are connected to the Master Server before trying to create a room
        if (!PhotonNetwork.IsConnectedAndReady || PhotonNetwork.Server != ServerConnection.MasterServer)
        {
            Debug.LogError("Cannot create room. Not connected to Master Server yet!");
            return;
        }

        settingsMenu.SetActive(false); // Hide settings menu after creation

        // Generate a unique lobby code
        lobbyCode = GenerateLobbyCode();

        // Get selected max player count from dropdown
        byte maxPlayers = (byte)(maxPlayersDropdown.value + 2); // Dropdown starts at 0, so adding 2 makes it 2-4

        RoomOptions options = new RoomOptions
        {
            MaxPlayers = maxPlayers,
            IsVisible = !privateToggle.isOn, // Public if toggle is off, private if on
            IsOpen = true,
            CleanupCacheOnLeave = true,
            EmptyRoomTtl = 0 // Delete the room immediately when empty
        };

        // Set custom properties (store lobby code & name for retrieval)
        // Set custom properties (store lobby code & name for retrieval)
        Hashtable roomProperties = new Hashtable
    {
        { "LobbyCode", lobbyCode },
        { "LobbyName", SteamManager.GetSteamUserName() + "'s Room" },
        { "MaxPlayers", maxPlayers },
        { "IsPrivate", privateToggle.isOn } // Add this property
    };
        options.CustomRoomProperties = roomProperties;
        options.CustomRoomPropertiesForLobby = new string[] { "LobbyCode", "LobbyName", "MaxPlayers", "IsPrivate" }; 

        PhotonNetwork.CreateRoom(lobbyCode, options, TypedLobby.Default);
    }

    public override void OnCreatedRoom()
    {
        Debug.Log("Room successfully created. Loading Character Select Scene...");
        PhotonNetwork.LoadLevel(4); // Load Character Select Scene
    }


    public override void OnJoinedRoom()
    {
        Debug.Log("Joined Room: " + PhotonNetwork.CurrentRoom.Name);
        PhotonNetwork.LoadLevel(4); // Load Character Select Scene after joining
    }

    public void JoinRoom()
    {
        PhotonNetwork.JoinRoom(joinRoomInput.text);
    }


    private string GenerateLobbyCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        System.Text.StringBuilder result = new System.Text.StringBuilder(5);
        System.Random random = new System.Random();

        for (int i = 0; i < 5; i++)
        {
            result.Append(chars[random.Next(chars.Length)]);
        }

        return result.ToString();
    }

    // Add to LobbyListManager:
    public override void OnJoinedLobby()
    {
        Debug.Log("Successfully joined the Photon lobby. Waiting for room list...");
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogError($"Disconnected from Photon: {cause}");
    }
}