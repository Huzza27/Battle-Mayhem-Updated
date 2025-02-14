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
        networkIssueUI.SetActive(false);    
        InvokeRepeating("CheckNetworkStatus", 0, checkInterval);
    }


    void CheckNetworkStatus()
    {
        if (PhotonNetwork.IsConnected)
        {
            bool hasNetworkIssue = CheckPing();
            networkIssueUI.SetActive(hasNetworkIssue);
        }
    }

    bool CheckPing()
    {
        int ping = PhotonNetwork.GetPing(); // Get current ping
        return ping > pingThreshold; // Return true if ping exceeds threshold
    }
}

