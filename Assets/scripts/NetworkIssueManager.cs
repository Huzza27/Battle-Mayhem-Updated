using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class NetworkIssueManager : MonoBehaviour
{
    public GameObject networkIssueUI; // Assign your UI element in the Inspector
    public float checkInterval = 5f; // Time between checks
    private int pingThreshold = 100; // Define acceptable ping threshold in milliseconds

    void Start()
    {
        InvokeRepeating("CheckNetworkStatus", 0, checkInterval);
        networkIssueUI.SetActive(false);
    }

    void CheckNetworkStatus()
    {
        if (PhotonNetwork.IsConnected && PhotonNetwork.IsMasterClient)
        {
            bool hasNetworkIssue = CheckPing();
            networkIssueUI.SetActive(hasNetworkIssue);
        }
        else
        {
            // Show UI when completely disconnected from Photon
            networkIssueUI.SetActive(true);
        }
    }

    bool CheckPing()
    {
        int ping = PhotonNetwork.GetPing(); // Get current ping
        return ping > pingThreshold; // Return true if ping exceeds threshold
    }
}

