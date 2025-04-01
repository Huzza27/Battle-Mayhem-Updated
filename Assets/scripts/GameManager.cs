using ExitGames.Client.Photon.StructWrapping;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class GameManager : MonoBehaviourPunCallbacks
{
    // Game state enum
    public enum GameState
    {
        Lobby,      // Players in lobby, configuring game
        Loading,    // Game is loading, initialize player list
        Playing,    // Game in progress
        GameOver    // Game has ended
    }

    public GameState CurrentState = GameState.Lobby;

    public int StartingLives;
    public TMP_InputField livesInputField;
    public static GameManager Instance;
    public GameObject livesDisplay;
    public int MapSelection;

    private List<int> lobbyPlayers = new List<int>();
    private bool playerListInitialized = false;
    public int initialPlayerCount = 0;

    public static event Action OnGameReset;
    public static Action<int> OnGameEnd;

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

    private void Start()
    {
        // Start in Lobby state
        ChangeState(GameState.Lobby);
    }

    public void SetReferences(TMP_InputField livesInputField)
    {
        this.livesInputField = livesInputField;
    }

    // In GameManager.cs
    public static void TriggerGameReset()
    {
        // Find any GameLoadingManager instances in the scene
        GameLoadingManager loadingManager = FindObjectOfType<GameLoadingManager>();

        // Only call ResetGameLoadingManagerState if we found a valid instance
        if (loadingManager != null && loadingManager.isActiveAndEnabled)
        {
            loadingManager.ResetGameLoadingManagerState();
        }
        else
        {
            Debug.Log("No active GameLoadingManager found during game reset");
        }

        // Always trigger the event for other listeners
        if (OnGameReset != null)
        {
            OnGameReset.Invoke();
        }
    }

    private void OnEnable()
    {
        GameManager.OnGameReset += Reset;
    }

    private void OnDisable()
    {
        GameManager.OnGameReset -= Reset;
    }

    // Change game state and handle transitions
    public void ChangeState(GameState newState)
    {
        GameState previousState = CurrentState;
        CurrentState = newState;

        Debug.Log($"[GameManager] State changed: {previousState} -> {newState}");

        switch (newState)
        {
            case GameState.Loading:
                // Initialize player list ONLY during Loading state
                if (!playerListInitialized)
                {
                    InitializePlayerList();
                }
                break;

            case GameState.Lobby:
                // Initialize player list ONLY during Loading state
                playerListInitialized = false;
                lobbyPlayers.Clear();
                break;

            case GameState.GameOver:
                // Initialize player list ONLY during Loading state
                playerListInitialized = false;
                break;
        }
    }

    public void SubscribeToPlayerElimEvent(KeepTrackOfEliminatedPlayers script)
    {
        script.OnPlayerEliminated += CheckIfGameShouldEnd;
    }

    private void InitializePlayerList()
    {
        lobbyPlayers.Clear();
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if(!lobbyPlayers.Contains(player.ActorNumber))
            {
                lobbyPlayers.Add(player.ActorNumber);
                Debug.Log($"Adding player {player.ActorNumber} to lobby player list");
            } 
        }

        // Store in room properties so all clients have this data
        Hashtable roomProperties = new Hashtable
        {
            { "PlayerList", lobbyPlayers.ToArray() }
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(roomProperties);

        playerListInitialized = true;
        initialPlayerCount = lobbyPlayers.Count;
        Debug.Log($"Player list initialized with {lobbyPlayers.Count} players");
    }


    public void Reset()
    {
        Debug.Log("[GameManager] Resetting game...");

        playerListInitialized = false; // Add this line

        if (PhotonNetwork.IsMasterClient)
        {
            Hashtable clearProperties = new Hashtable {
            { "Winner", null },
            { "PlayerList", null } // Also clear the PlayerList property
        };
            PhotonNetwork.CurrentRoom.SetCustomProperties(clearProperties);
        }
    }

    public void CheckIfGameShouldEnd()
    {
        Debug.Log("[CheckIfGameShouldEnd] Function called...");

        // Don't check if the game is already over
        if (CurrentState == GameState.GameOver)
        {
            Debug.Log("[CheckIfGameShouldEnd] Game is already over, skipping check.");
            return;
        }

        object playerListObj;
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("PlayerList", out playerListObj))
        {
            int[] playerList = (int[])playerListObj;
            Debug.Log($"[CheckIfGameShouldEnd] PlayerList retrieved: {string.Join(", ", playerList)}");
            Debug.Log($"[CheckIfGameShouldEnd] Current active players: {playerList.Length} / Initial players: {initialPlayerCount}");

            // Game ends when only one player remains from the initial set
            if (playerList.Length == 1 && !(CurrentState == GameState.GameOver))
            {
                int winnerActorNumber = playerList[0];
                Debug.Log($"[CheckIfGameShouldEnd] One player remains. Declaring winner: ActorNumber {winnerActorNumber}");
                OnGameEnd?.Invoke(winnerActorNumber);

            }
            else
            {
                Debug.Log($"[CheckIfGameShouldEnd] Game is not ending yet. More than one player is active.");
            }
        }
        else
        {
            Debug.LogError("[CheckIfGameShouldEnd] Failed to retrieve PlayerList from room properties.");
        }
    }

    // For starting a rematch after game over
    public void StartRematch()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            // Reset game state before changing to Loading
            TriggerGameReset();
            ChangeState(GameState.Loading);
        }
    }

    public void SetLives()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            int livesValue = 10;
            if (int.TryParse(livesInputField.text, out int result))
            {
                livesValue = result;
            }

            Hashtable roomProperties = new Hashtable
            {
                { "StartingLives", livesValue }
            };
            PhotonNetwork.CurrentRoom.SetCustomProperties(roomProperties);
        }
    }

    public void DiableLivesInputField()
    {
        if (!PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            livesDisplay.SetActive(false);
        }
    }

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.ContainsKey("StartRematch"))
        {
            StartRematch();
        }
    }
}