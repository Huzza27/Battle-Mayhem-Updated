using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Mirror : MonoBehaviour
{
    public BoxCollider2D collider;
    public PhotonView view;
    PhotonView mirrorView;
    public GunMechanicManager gunManager;
    bool hasMirror = false;
    bool hasDeflected = false;

    private void Start()
    {
        mirrorView = GetComponent<PhotonView>();
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
            mirrorView.RPC("ToggleMirror", RpcTarget.All, true);
        }
        else
        {
            mirrorView.RPC("ToggleMirror", RpcTarget.All, false);
        }
    }

    [PunRPC]
    void ToggleMirror(bool toggle)
    {
        hasMirror = toggle;
        collider.enabled = toggle;

    }

    public void OnHitMirror()
    {
        gunManager.bulletCount--;
        view.RPC("updateBulletCount", RpcTarget.AllBuffered, gunManager.bulletCount);
        if(gunManager.bulletCount <= 0)
        {
            view.RPC("SwapItemsToOriginal", RpcTarget.All);
        }
        mirrorView.RPC("ToggleMirror", RpcTarget.All, false);
    }
}
