using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerState
{
    Alive,
    Dead
}
public class PlayerStateManager : MonoBehaviour
{
    public PlayerState playerState;
    public PhotonView view;
    public void ChangePlayerState(PlayerState state)
    {
        playerState = state;
        switch (playerState)
        {
            case PlayerState.Dead:
                view.RPC("RequestRemovePlayerFromRoomList", RpcTarget.MasterClient, PhotonNetwork.LocalPlayer.ActorNumber);
                break;
            case PlayerState.Alive:
                view.RPC("AddPlayerToRoomProperties", RpcTarget.MasterClient, PhotonNetwork.LocalPlayer.ActorNumber);
                break;

        }
    }
}
