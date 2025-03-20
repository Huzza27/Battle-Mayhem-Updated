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
    public int StartingLives;
    public TMP_InputField livesInputField;
    public static GameManager Instance;
    public GameObject livesDisplay;
    public int MapSelection;
    public bool gameOver = false;
    public bool gameStarted = false;  // Change from private to public
    public int initialPlayerCount = 0;
    private bool playersInitialized = false;
    static bool gameHasBeenRestartedForRematch = false;

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
        Reset();
    }

    private void Start()
    {
        StartCoroutine(WaitForPlayersToSpawn());
    }

    public void SetReferences(TMP_InputField livesInputField)
    {
        this.livesInputField = livesInputField;
    }

    public static void TriggerGameReset()
    {
        gameHasBeenRestartedForRematch = true;
        OnGameReset?.Invoke();
    }

    private void OnEnable()
    {
        GameManager.OnGameReset += Reset;
    }

    private void OnDisable()
    {
        GameManager.OnGameReset -= Reset;
    }

    private IEnumerator WaitForPlayersToSpawn()
    {
        // Wait for scene to load and network objects to spawn
        yield return new WaitForSeconds(2f);

        // Wait until we detect all players are present in the room
        float timeout = 0f;
        while (!AllPlayersSpawned() && timeout < 10f)
        {
            timeout += Time.deltaTime;
            yield return null;
        }

        if (timeout >= 10f)
        {
            Debug.LogWarning("Timeout waiting for players to spawn!");
        }

        // Initialize player list only after players have spawned
        InitializePlayerList();

        // Start the game over check after initialization
        StartCoroutine(DelayedStartGameOverCheck());
    }

    private bool AllPlayersSpawned()
    {
        // Check if the number of spawned players matches the number of players in the room
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player"); // Ensure your player prefabs have the "Player" tag
        return players.Length == PhotonNetwork.CurrentRoom.PlayerCount;
    }

    private void InitializePlayerList()
    {
        List<int> initialPlayers = new List<int>();
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            initialPlayers.Add(player.ActorNumber);
        }

        initialPlayerCount = initialPlayers.Count;

        Hashtable roomProperties = new Hashtable
        {
            { "PlayerList", initialPlayers.ToArray() }
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(roomProperties);

        playersInitialized = true;
        Debug.Log($"Initialized player list with {initialPlayerCount} players");
    }

    private IEnumerator DelayedStartGameOverCheck()
    {
        yield return new WaitForSeconds(5f); // Reduced delay since we already waited for spawns
        gameStarted = true;
        StartCoroutine(CheckForGameOver());
    }

    private IEnumerator CheckForGameOver()
    {
        while (!gameOver)
        {
            yield return new WaitForSeconds(1f);
            if (PhotonNetwork.IsMasterClient && gameStarted && playersInitialized)
            {
                CheckIfGameShouldEnd();
            }
        }
    }



    private void CheckIfGameShouldEnd()
    {
        Debug.Log("[CheckIfGameShouldEnd] Function called...");

        // Don't check if the game is already over
        if (gameOver)
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
            if (playerList.Length == 1 && initialPlayerCount > 1 && !gameOver)
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


    public void Reset()
    {
        gameOver = false;
        gameStarted = false;
        initialPlayerCount = 0;
        playersInitialized = false;
        //MapSelection = 0;

        if (PhotonNetwork.IsMasterClient)
        {
            // Clear the Winner property
            Hashtable clearProperties = new Hashtable { { "Winner", null } };
            PhotonNetwork.CurrentRoom.SetCustomProperties(clearProperties);

            if (gameHasBeenRestartedForRematch)
            {
                // Delay player list initialization to ensure all players are properly rejoined
                StartCoroutine(DelayedInitializePlayerList());
            }
        }
    }

    private IEnumerator DelayedInitializePlayerList()
    {
        // Wait for players to properly rejoin
        yield return new WaitForSeconds(2f);

        // Initialize player list only after players have spawned
        InitializePlayerList();

        // Start the game over check after initialization
        StartCoroutine(DelayedStartGameOverCheck());
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
            gameOver = false;
        }
    }
}
