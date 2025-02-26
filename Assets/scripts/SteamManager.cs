using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using Photon.Pun;

public class SteamManager : MonoBehaviour
{
    public static SteamManager instance;
    private uint appID = 3441600;
    private static bool connectedToSteam = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        try
        {
            Steamworks.SteamClient.Init(appID);
            connectedToSteam = true;
        }
        catch(System.Exception exception)
        {
            connectedToSteam = false;
        }
    }

    private void Update()
    {
        if(connectedToSteam)
        {
            Steamworks.SteamClient.RunCallbacks();
        }
    }

    public void DisconnectFromSteam()
    {
        {
            if(connectedToSteam)
            {
                Steamworks.SteamClient.Shutdown();
            }
        }
    }

    public static string GetSteamUserName()
    {
        if(connectedToSteam)
        {
            return SteamClient.Name;
        }
        else
        {
            return "Player" + PhotonNetwork.LocalPlayer.ActorNumber;
        }
    }
}
