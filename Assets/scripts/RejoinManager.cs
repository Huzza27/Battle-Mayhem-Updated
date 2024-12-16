using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RejoinManager : MonoBehaviour
{
    public static TextMeshProUGUI errorText;
    private static string masterClientText = "A player has been unexpectedly disconnected, waiting for them to rejoin...";
    private static string secondClientText = "Connection Lost! /n Attempting to recconect..";
    static bool isOpen = false;
    [SerializeField] static Canvas canvas;
    public static void ToggleRejoinUI()
    {
        errorText.text = PhotonNetwork.IsMasterClient ? masterClientText : secondClientText;
        if(isOpen)
        {
            canvas.enabled = false;
            isOpen = false;
        }
        else
        {
            canvas.enabled = true;
            isOpen = true;
        }
    }
}
