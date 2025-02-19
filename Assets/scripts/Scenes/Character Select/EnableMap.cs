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
        view.RPC("SetMap", RpcTarget.All);
    }

    [PunRPC]
    private void SetMap()
    {
        maps[GameManager.Instance.MapSelection].SetActive(true);
    }
}
