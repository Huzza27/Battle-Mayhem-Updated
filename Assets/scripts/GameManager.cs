using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Dynamic;
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
    public PhotonView view;
    public int MapSelection;
    public bool gameOver = false;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            DontDestroyOnLoad(gameObject); // Prevent the manager from being destroyed on load
        }
        else
        {
            Destroy(gameObject); // Ensures only one instance exists
        }

        Reset();
    }

    public void Reset()
    {
        gameOver = false;
    }

    public void SetLives()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            int livesValue = 10; // Default value
            if (int.TryParse(livesInputField.text, out int result))
            {
                livesValue = result;
            }

            // Store the value in room properties
            Hashtable roomProperties = new Hashtable
        {
            { "StartingLives", livesValue }
        };
            PhotonNetwork.CurrentRoom.SetCustomProperties(roomProperties);
        }
    }





    public void SetPlayerColorChoice(int choice)
    {
        if (PhotonNetwork.LocalPlayer != null)
        {
            Hashtable props = new Hashtable { { "PlayerColor", choice } };
            PhotonNetwork.LocalPlayer.SetCustomProperties(props);
            Debug.Log("Set PlayerColor to for player " + PhotonNetwork.LocalPlayer + choice);
        }
        else
        {
            Debug.LogError("PhotonNetwork.LocalPlayer is null");
        }
    }

    public void SetPlayerGunChoice(int choice)
    {
        Hashtable props = new Hashtable { { "PlayerMainGunChoice", choice } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
    }

    public void DiableLivesInputField()
    {
        if(!PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            livesDisplay.SetActive(false);
        }
    }

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        if(propertiesThatChanged.ContainsKey("Winner"))
        {
            gameOver = true;
        }
        if(propertiesThatChanged.ContainsKey("StartRematch"))
        {
             gameOver = false;
        }
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();

        // Reset game state when joining a new room
        Reset();

        Debug.Log("Joined a new room. Game state has been reset.");
    }
}


