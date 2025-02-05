using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class SetPlayerPreferences : MonoBehaviour
{
    public static void SetPlayerColorChoice(int choice)
    {
        if (PhotonNetwork.LocalPlayer != null)
        {
            Hashtable props = new Hashtable { { "PlayerColor", choice } };
            PhotonNetwork.LocalPlayer.SetCustomProperties(props);
            Debug.Log("Set PlayerColor to for player " + PhotonNetwork.LocalPlayer + choice);
        }
        else
        {
            Debug.LogError("PhotonNetwork.LocalPlayer is null");
        }
    }

    public void SetPlayerGunChoice(int choice)
    {
        Hashtable props = new Hashtable { { "PlayerMainGunChoice", choice } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
    }


}
