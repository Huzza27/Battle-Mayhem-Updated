using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class GameManager : MonoBehaviour
{
    public int StartingLives;
    public TMP_InputField livesInputField;
    public static GameManager Instance;
    public GameObject livesDisplay;
    public PhotonView view;
    public int MapSelection;
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
        };
    }

    public void SetLives()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (int.TryParse(livesInputField.text, out int result))
            {
                view.RPC("SynchronizeStartingLives", RpcTarget.AllBuffered, result);
            }
            else
            {
                Debug.LogError("Input is not a valid integer");
            }
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
}


