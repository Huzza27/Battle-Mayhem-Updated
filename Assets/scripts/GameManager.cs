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
    private bool gameStarted = false; // Flag to prevent early game-over checks

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
            StartCoroutine(DelayedStartGameOverCheck()); // Delay the game-over check
        }
    }

    public void Reset()
    {
        gameOver = false;
        gameStarted = false; // Reset this flag when starting a new game
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

    private IEnumerator DelayedStartGameOverCheck()
    {
        yield return new WaitForSeconds(10f); // Wait 5 seconds before starting game-over checks
        gameStarted = true; // Now allow game-over checks
        StartCoroutine(CheckForGameOver());
    }

    private IEnumerator CheckForGameOver()
    {
        while (!gameOver)
        {
            yield return new WaitForSeconds(1f);

            if (PhotonNetwork.IsMasterClient && gameStarted) // Only check if the game has started
            {
                CheckIfGameShouldEnd();
            }
        }
    }

    private void CheckIfGameShouldEnd()
    {
        object playerListObj;
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("PlayerList", out playerListObj))
        {
            int[] playerList = (int[])playerListObj;

            if (playerList.Length == 1 && gameStarted) // Ensure the game has started before checking
            {
                int winnerActorNumber = playerList[0];

                Hashtable winnerProperties = new Hashtable { { "Winner", winnerActorNumber } };
                PhotonNetwork.CurrentRoom.SetCustomProperties(winnerProperties);

                Debug.Log($"Game Over! Winner: {PhotonNetwork.CurrentRoom.GetPlayer(winnerActorNumber)?.NickName}");
            }
        }
    }
}
