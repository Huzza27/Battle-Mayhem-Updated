using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReloadAnimEvent : MonoBehaviour
{
    public GunMechanicManager manager;
    public BoxCollider2D collider;
    
    //THIS SCRIPT IS BASICALLY FOR ALL ARM RELATED ANIMATION EVENTS!!!!
    public void OnReloadAnimationEnter()
    {
        manager.OnReloadEnter();
    }

    public void OnReloadAnimationExit()
    {
        manager.OnReloadExit();
    }
    
    public void OnKatanaAttackEnter()
    {
        collider.enabled = true;
    }

    public void OnKatanaAttackExit()
    {
        collider.enabled = false;
    }
}
