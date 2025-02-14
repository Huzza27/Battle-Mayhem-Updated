using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GunTesetingScript : MonoBehaviour
{
    public PhotonView playerView;
    public TMP_Dropdown dropdown;

    public void TestWeapon()
    {
        playerView.RPC("SwapItems", RpcTarget.All, (int)dropdown.value);
    }
}
