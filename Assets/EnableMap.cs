using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableMap : MonoBehaviour
{
    public GameObject[] maps;
    public PhotonView view;
    // Start is called before the first frame update
    void Start()
    {
        if(PhotonNetwork.IsMasterClient)
        {
            // Set the map based on the selected index
            view.RPC("SetMap", RpcTarget.All, GameManager.Instance.MapSelection);
        }
    }

    [PunRPC]
    private void SetMap(int mapIndex)
    {
        maps[mapIndex].SetActive(true);
    }
}
