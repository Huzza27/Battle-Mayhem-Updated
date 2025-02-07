using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Mirror : MonoBehaviour
{
    public BoxCollider2D collider;
    private PhotonView view;
    public PhotonView playerView;
    public GunMechanicManager gunManager;
    bool hasMirror = false;
    bool hasDeflected = false;

    private void Start()
    {
        view = this.GetComponent<PhotonView>();
    }

    private void Update()
    {
        if (!hasMirror)
        {
            CheckForMirror();
        }
    }
    void CheckForMirror()
    {
        if (gunManager.heldItem.name == "Mirror")
        {          
            hasMirror = true;
            collider.enabled = true;       
        }
        else
        {
            hasMirror = false;
            collider.enabled = false;      
        }
    }

    public void OnHitMirror()
    {
        view.RPC("LowerDurability", RpcTarget.All);
    }

    [PunRPC]
    public void LowerDurability()
    {
        gunManager.bulletCount--;
        playerView.RPC("updateBulletCount", RpcTarget.AllBuffered, gunManager.bulletCount);
    }
}
