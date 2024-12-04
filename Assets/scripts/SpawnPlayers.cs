using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;
public class SpawnPlayers : MonoBehaviour
{
    public GameObject playerPrefab;
    public GameObject cameraPrefab;

    public GameObject[] healthBarList;

    public Transform canvas;
    public Transform camSpawn;

    [Header("Paralax")]
    public Parallax nearest, farthest, Extras;
    public float minX;
    public float maxX;
    public float minY;
    public float maxY;
    GameObject player;

    [Header("Spawn Points")]
    public Transform p1StartSpawn, p2StartSpawn;

    GameObject playerCam;

    //Quaternion rotation;
    public static int playerCount = 0;
    // Static dictionary to track player GameObjects
    private void Start()
    {
        SpawnPlayerAtStart();
        SetPlayerReadyFlag();
    }

    private void SetPlayerReadyFlag()
    {
        Hashtable properties = new Hashtable
    {
        { "IsReady", true }
    };
        PhotonNetwork.LocalPlayer.SetCustomProperties(properties);
    }

    private void SpawnPlayerAtStart()
    {
        Transform spawn = PhotonNetwork.LocalPlayer.ActorNumber == 1 ? p1StartSpawn : p2StartSpawn;
        player = PhotonNetwork.Instantiate(playerPrefab.name, spawn.position, Quaternion.identity);

        PhotonView view = player.GetComponent<PhotonView>();

        SetBodyColor(view);
        EquipDefaultGun(view);

        playerCount++;

        if (view.IsMine)
        {
            AssignCamera(spawn);

            SetHealthBar(view);


            if (playerCount > 1)
            {
                SwapHealthBarPositions();
            }
        }
    }

    private void AssignCamera(Transform spawnPos)
    {
        if (spawnPos != null)
        {
            playerCam = PhotonNetwork.Instantiate(cameraPrefab.name, spawnPos.position, Quaternion.identity);
            CameraMove cameraMoveScript = playerCam.GetComponent<CameraMove>();
            cameraMoveScript.player = player.transform;


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
        // Pass the view ID and actor number of the player being spawned
        view.RPC("AssignHealthBar", RpcTarget.AllBuffered, view.ViewID, view.Owner.ActorNumber);
    }
    public void SwapHealthBarPositions()
    {
        Transform currentPos;
        switch(playerCount)
        {
            case 2:
                currentPos = healthBarList[playerCount - 1].transform;
                healthBarList[playerCount - 1].transform.position = healthBarList[playerCount - 2].transform.position;
                healthBarList[playerCount - 2].transform.position = currentPos.position;
                break;
            case 3:
                currentPos = healthBarList[playerCount - 2].transform;
                healthBarList[playerCount - 2].transform.position = healthBarList[playerCount - 2].transform.position;
                healthBarList[playerCount - 3].transform.position = currentPos.position;
                break;
            case 4:
                currentPos = healthBarList[playerCount - 3].transform;
                healthBarList[playerCount - 3].transform.position = healthBarList[playerCount - 2].transform.position;
                healthBarList[playerCount - 4].transform.position = currentPos.position;
                break;
        }
    }
}

