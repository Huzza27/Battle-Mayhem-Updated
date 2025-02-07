using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class KeepTrackOfEliminatedPlayers : MonoBehaviour
{
    public PhotonView view;

    [PunRPC]
    private void RequestAddPlayerToRoomList(int actorNumber)
    {
        if (!PhotonNetwork.IsMasterClient) return; // Ensure only the MasterClient runs this

        AddPlayerToRoomProperties(actorNumber);
    }



    private void AddPlayerToRoomProperties(int actorNumber)
    {
        if (!PhotonNetwork.IsMasterClient) return; // Only MasterClient modifies properties

        Hashtable roomProperties = PhotonNetwork.CurrentRoom.CustomProperties;
        object playerListObj;

        List<int> playerList = new List<int>();

        // Check if the property exists; if not, create it
        if (roomProperties.TryGetValue("PlayerList", out playerListObj))
        {
            playerList = ((int[])playerListObj).ToList();
        }
        else
        {
            Debug.Log("PlayerList property does not exist. Creating a new one...");
        }

        if (!playerList.Contains(actorNumber))
        {
            playerList.Add(actorNumber);
            roomProperties["PlayerList"] = playerList.ToArray(); // Convert list to array
            PhotonNetwork.CurrentRoom.SetCustomProperties(roomProperties);

            Debug.Log($"MasterClient added player {actorNumber} to PlayerList. Total players now: {playerList.Count}");
        }
    }


    [PunRPC]
    private void RequestRemovePlayerFromRoomList(int actorNumber)
    {
        if (!PhotonNetwork.IsMasterClient) return; // Ensure only the MasterClient processes this request

        RemovePlayerFromRoomProperties(actorNumber);
    }



    private void RemovePlayerFromRoomProperties(int actorNumber)
    {
        if (!PhotonNetwork.IsMasterClient) return; // Ensure only the MasterClient modifies properties

        Hashtable roomProperties = PhotonNetwork.CurrentRoom.CustomProperties;
        object playerListObj;
        List<int> playerList = new List<int>();

        if (roomProperties.TryGetValue("PlayerList", out playerListObj))
        {
            playerList = ((int[])playerListObj).ToList();
        }

        if (playerList.Contains(actorNumber))
        {
            playerList.Remove(actorNumber);
            roomProperties["PlayerList"] = playerList.ToArray();
            PhotonNetwork.CurrentRoom.SetCustomProperties(roomProperties);

            Debug.Log($"MasterClient removed player {actorNumber} from PlayerList. Remaining players: {playerList.Count}");
        }
    }
}
