using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformCollision : MonoBehaviour
{

    public AnimController animController;
    public Movement movementScript;
    private bool isGrounded;
    public GunMechanicManager gunMechManager;
    public AudioSource audioSource;
    public AudioClip softFallAudio;
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Ground")
        {
            if(animController.gameObject.GetComponent<Rigidbody2D>().velocity.y > 0) 
            { return; }
            audioSource.PlayOneShot(softFallAudio);
            isGrounded = true;
            animController.SetAnimatorGroundFlag(isGrounded);
            movementScript.isGrounded = true;
            movementScript.currentPlatform = collision.transform.root.gameObject;
            if(gunMechManager.heldItem.MustBeGrounded())
            {
                gunMechManager.canUseItem = true;
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    { 
        movementScript.currentPlatform = null;
        movementScript.isGrounded = false;
        /*
        if (gunMechManager.heldItem.MustBeGrounded())
        {
            gunMechManager.canUseItem = false;
        }
        */
        animController.SetAnimatorGroundFlag(isGrounded);
        movementScript.canDoubleJump = true;
    }
}
