using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialWeaponColliders : MonoBehaviour
{
    public Collider2D ShotgunCollider;
    public void SetColliderEnabled(Collider2D collider)
    {
        collider.enabled = true;
    }
}
