using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class ResetMatch : MonoBehaviourPunCallbacks
{
    public Transform spawnPoint;
    public Transform camSpawn;
    public GameObject playerCamera;
    [SerializeField] PhotonView view;

    /* Purpose: To reset all player game states so game can be played again
     * 1) Send player back to spawn
     * 2) Set default weapon for player
     * 3) Set correct starting lives for player
     * 4) Reset all player properties that are needed for game restart
     */

    public void InitiateGameReset()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            ResetCustomProperties();
            view.RPC("ResetGame", RpcTarget.All);
        }
        view.RPC("AddPlayerToRoomProperties", RpcTarget.MasterClient, PhotonNetwork.LocalPlayer.ActorNumber);
    }

    private void OnEnable()
    {
        GameManager.OnGameReset += InitiateGameReset;
    }

    private void OnDisable()
    {
        GameManager.OnGameReset -= InitiateGameReset;
    }

    [PunRPC]
    void ResetGame()
    {
        // Find and reset loading screen
        GameObject loadingScreen = GameObject.FindGameObjectWithTag("LoadingScreen");
        if (loadingScreen != null)
        {
            loadingScreen.GetComponent<GameLoadingManager>().ResetGameLoadingManagerState();
        }

        // Reset player position
        SendPlayerToSpawn();



        
        // Reset weapons
        if (view.IsMine)
        {
            //Reset camera position
            ResetCamera();
            view.RPC("SwapItemsToOriginal", RpcTarget.All);
            view.RPC("SetUpLivesDisplay", RpcTarget.All);
            view.RPC("UpdateHealthUI", RpcTarget.All, view.ViewID, 100f);
        }
        
        // Reset properties (only master client should do this)
        if (PhotonNetwork.IsMasterClient)
        {
            ResetCustomProperties();
        }
    }

    void ResetCustomProperties()
    {
        string[] propertiesToReset = new string[]
        {
            "StartRematch"
        };

        Hashtable resetProperties = new Hashtable();
        foreach (string property in propertiesToReset)
        {
            switch (property)
            {
                case "StartRematch":
                    resetProperties[property] = null; // Reset to false
                    break;
            }
        }

        PhotonNetwork.CurrentRoom.SetCustomProperties(resetProperties);
    }

    [PunRPC]
    void SendPlayerToSpawn()
    {
        if (view.IsMine) // Only move our own player
        {
            transform.position = spawnPoint.position;
        }
    }

    void ResetCamera()
    {
        playerCamera.transform.position = camSpawn.transform.position;
    }

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.ContainsKey("StartRematch"))
        {
            if (propertiesThatChanged.TryGetValue("StartRematch", out object startRematchObj) &&
                startRematchObj is bool startRematch &&
                startRematch)
            {
                InitiateGameReset();
            }
        }
    }
}
