using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class CreateAndJoinRooms : MonoBehaviourPunCallbacks
{
    public InputField createInput;
    public InputField joinInput;

    private void Awake()
    {
        PhotonNetwork.SendRate = 30;
        PhotonNetwork.SerializationRate = 30;

    }

    public void CreateRoom()
    {
        if(createInput.text == string.Empty)
        {

        }
        RoomOptions options = new RoomOptions
        {
            MaxPlayers = 2,
            IsVisible = true,
            IsOpen = true,
            CleanupCacheOnLeave = true, // Clear network objects and cache
            EmptyRoomTtl = 0 // Delete the room immediately when empty
        };

        PhotonNetwork.CreateRoom(createInput.text, options, TypedLobby.Default);
    }

    public void TriggerError()
    {
        
    }

    public void JoinRoom()
    {
        PhotonNetwork.JoinRoom(joinInput.text);
    }

    public override void OnJoinedRoom()
    {
        PhotonNetwork.LoadLevel("CharacterSelect");
    }
}
