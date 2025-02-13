using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowingKnifeCollider : MonoBehaviour
{
    public ThrowingKnife throwingKnife;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        throwingKnife.ExternalCollisionCheck(collision);
    }
}
