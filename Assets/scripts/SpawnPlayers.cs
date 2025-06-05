using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using System.Linq;
using Photon.Realtime;

public class SpawnPlayers : MonoBehaviour
{
    public GameObject playerPrefab;
    public GameObject cameraPrefab;

    public GameObject[] healthBarList;
    public GunTesetingScript testingScript;

    public Transform canvas;
    public Transform camSpawn;

    [Header("Parallax")]
    public Parallax nearest, farthest, Extras;
    public float minX;
    public float maxX;
    public float minY;
    public float maxY;
    GameObject player;

    [Header("Spawn Points")]
    public Transform[] spawnPoints; // Supports multiple spawn points dynamically

    GameObject playerCam;

    [Header("ScriptReferences")]
    private Movement movement;

    [Header("EndGameUI")]
    public Toggle[] rematchToggleList;

    private void Awake()
    {
        ResetSpawnPlayersState();
    }

    public void ResetSpawnPlayersState()
    {
        // Re-enable disabled players
        foreach (GameObject p in GameObject.FindGameObjectsWithTag("Player"))
        {
            GameObject player = p.transform.root.gameObject;
            if (!player.activeSelf) // If the player was disabled (eliminated)
            {
                player.SetActive(true);
                player.GetComponent<Respawn>().ResetPlayerState();
            }
        }

        // Reset health bars
        if (healthBarList != null)
        {
            foreach (var healthBar in healthBarList)
            {
                if (healthBar != null)
                {
                    healthBar.SetActive(false);
                }
            }
        }

        // Reset other game state
        if (nearest != null) nearest.cameraTransform = null;
        if (farthest != null) farthest.cameraTransform = null;
        if (Extras != null) Extras.cameraTransform = null;

        movement = null;

        Hashtable properties = new Hashtable
    {
        { "IsReady", false }
    };
        PhotonNetwork.LocalPlayer.SetCustomProperties(properties);

        foreach (var toggle in rematchToggleList)
        {
            if (toggle != null)
            {
                toggle.isOn = false;
                toggle.interactable = false;
            }
        }

        Debug.Log("Players re-enabled and rematch state reset.");
    }


    private void Start()
    {
        SpawnPlayerAtStart();
        SetPlayerFacingOnStart();
        SetPlayerReadyFlag();
    }

    private void SetPlayerFacingOnStart()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            player.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            movement.facingRight = true;
        }
    }

    private void SetPlayerReadyFlag()
    {
        if (player == null)
        {
            Debug.LogError("Player object is null. Cannot set ready flag.");
            return;
        }

        Hashtable properties = new Hashtable
        {
            { "IsReady", true }
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(properties);
        Debug.Log($"Player {PhotonNetwork.LocalPlayer.NickName} is ready.");
    }

    private void SpawnPlayerAtStart()
    {
        if (spawnPoints.Length == 0)
        {
            Debug.LogError("No spawn points assigned! Ensure the spawnPoints array is populated.");
            return;
        }

        int spawnIndex = PhotonNetwork.LocalPlayer.ActorNumber - 1;
        Transform spawn = spawnPoints[spawnIndex];

        player = PhotonNetwork.Instantiate(playerPrefab.name, spawn.position, Quaternion.identity);
        player.GetComponent<ResetMatch>().spawnPoint = spawn;
        player.GetComponent<ResetMatch>().camSpawn = camSpawn;
        PhotonView view = player.GetComponent<PhotonView>();

        // Ask the MasterClient to update the PlayerList
        view.RPC("RequestAddPlayerToRoomList", RpcTarget.MasterClient, PhotonNetwork.LocalPlayer.ActorNumber);
        if(PhotonNetwork.IsMasterClient)
        {
            SetStatsForPlayer();
        }

        SetBodyColor(view);
        EquipDefaultGun(view);

        if (view.IsMine)
        {
            AssignCamera(camSpawn);
            SetHealthBar(view);
            movement = player.GetComponent<Movement>();
        }

        testingScript.playerView = view;
    }



    void SetStatsForPlayer()
    {
        foreach(Player player in PhotonNetwork.PlayerList)
        {
            Debug.Log("Registering Player " + player.ActorNumber);
            MatchStatsManager.Instance.RegisterPlayer(player.ActorNumber, SteamManager.GetSteamUserName());
        }
        
    }

    private int GetRemainingPlayerCount()
    {
        object playerListObj;
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("PlayerList", out playerListObj))
        {
            int[] playerList = (int[])playerListObj;
            return playerList.Length; // Returns number of active players
        }
        return 0;
    }

    private Player FindLastAlivePlayer()
    {
        if (!PhotonNetwork.IsMasterClient) return null;

        object playerListObj;
        if (!PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("PlayerList", out playerListObj))
        {
            Debug.LogWarning("PlayerList property not found.");
            return null;
        }

        int[] playerList = (int[])playerListObj;

        if (playerList.Length == 1) // Only return the winner if exactly ONE player remains
        {
            return PhotonNetwork.CurrentRoom.GetPlayer(playerList[0]);
        }

        return null;
    }

    private void AssignCamera(Transform spawnPos)
    {
        if (spawnPos != null)
        {
            playerCam = PhotonNetwork.Instantiate(cameraPrefab.name, spawnPos.position, Quaternion.identity);
            CameraMove cameraMoveScript = playerCam.GetComponent<CameraMove>();
            cameraMoveScript.player = player.transform;

            player.GetComponent<ResetMatch>().playerCamera = playerCam;

            nearest.cameraTransform = playerCam.transform;
            farthest.cameraTransform = playerCam.transform;
            Extras.cameraTransform = playerCam.transform;
        }
    }

    private void SetBodyColor(PhotonView view)
    {
        view.RPC("SetPlayerColorForAllClients", RpcTarget.AllBuffered, view.ViewID);
    }

    private void EquipDefaultGun(PhotonView view)
    {
        view.RPC("EquipMainWeapon", RpcTarget.AllBuffered, view.ViewID);
    }

    private void SetHealthBar(PhotonView view)
    {
        if (!view.gameObject.GetComponent<Health>().enabled)
        {
            view.gameObject.GetComponent<Health>().enabled = true;
        }
        view.RPC("AssignHealthBar", RpcTarget.AllBuffered, view.ViewID, view.Owner.ActorNumber);
    }
}
