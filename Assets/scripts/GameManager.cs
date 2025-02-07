using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
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
    private bool gameStarted = false;
    private int initialPlayerCount = 0;
    private bool playersInitialized = false;

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
        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(WaitForPlayersToSpawn());
        }
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
        if (!gameStarted || !playersInitialized) return;

        object playerListObj;
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("PlayerList", out playerListObj))
        {
            int[] playerList = (int[])playerListObj;
            Debug.Log($"Current active players: {playerList.Length} / Initial players: {initialPlayerCount}");

            // Game ends when only one player remains from the initial set
            if (playerList.Length == 1 && initialPlayerCount > 1)
            {
                int winnerActorNumber = playerList[0];
                Hashtable winnerProperties = new Hashtable { { "Winner", winnerActorNumber } };
                PhotonNetwork.CurrentRoom.SetCustomProperties(winnerProperties);
                Debug.Log($"Game Over! Winner: {PhotonNetwork.CurrentRoom.GetPlayer(winnerActorNumber)?.NickName}");
            }
        }
    }

    public void Reset()
    {
        gameOver = false;
        gameStarted = false;
        initialPlayerCount = 0;
        playersInitialized = false;
        MapSelection = 0;
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
