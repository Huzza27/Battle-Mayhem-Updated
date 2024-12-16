using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkTester : MonoBehaviourPunCallbacks
{
    public void SimulateDisconnect()
    {
        Debug.Log("Simulating network disconnect...");
        PhotonNetwork.Disconnect(); // Simulates unexpected disconnection
    }

    // Simulate intentional leaving
    public void SimulateLeaveRoom()
    {
        Debug.Log("Simulating player leaving the room...");
        PhotonNetwork.LeaveRoom(); // Simulates intentional room exit
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log($"Disconnected! Cause: {cause}");
    }
}
