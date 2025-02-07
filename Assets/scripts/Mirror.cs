using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Mirror : MonoBehaviour
{
    public BoxCollider2D collider;
    public GunMechanicManager gunManager;
    bool hasMirror = false;
    bool hasDeflected = false;

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
            ToggleShooting(false);
        }
        else
        {
            hasMirror = false;
            collider.enabled = false;
            ToggleShooting(true);
        }
    }
    void ToggleShooting(bool toggle)
    {
        gunManager.canUseItem = toggle;
    }
}
