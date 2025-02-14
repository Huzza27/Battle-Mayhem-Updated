using Steamworks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteamManager : MonoBehaviour
{
    public static SteamManager instance;
    private uint appID = 3441600;
    public bool connectedToSteam = false;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);

        }
        else
        {
            Destroy(this.gameObject);
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

    // Update is called once per frame
    void Update()
    {
        if(connectedToSteam)
        {
            Steamworks.SteamClient.RunCallbacks();
        }
    }

    public void DissconnectFromSteam()
    {
        if(connectedToSteam)
        {
            Steamworks.SteamClient.Shutdown();
        }
    }

    public string GetSteamUsername()
    {
        if (connectedToSteam)
        {
            return SteamClient.Name;
        }
        return "Guest";
    }
}
