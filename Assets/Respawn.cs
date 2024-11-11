using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class Respawn : MonoBehaviour
{
    // Components to disable player------------
    public Rigidbody2D rb;
    public SpriteRenderer[] renderers;
    public GunMechanicManager manager;
    public CapsuleCollider2D[] colliders;
    public Canvas ammoCount;
    public GameObject hand;

    public float shakeDuration;
    public float shakeMagnitude;
    
    //---------------------------------------

    public Health health;
    public PhotonView view;
    public float respawnDelay;
    public Transform respawnArea;
    private void Awake()
    {
        respawnArea = GameObject.FindGameObjectWithTag("RespawnArea").transform;
    }

    /* PUN RPC METHOD --> Called from DeathCollider
     * Runs Death Logic
     */
    [PunRPC]
    public void Death()
    {
        health.healthAmount = 0; // Explicitly set health to zero
        health.fillImage.fillAmount = 0; // Update health bar UI
        health.isDead = true; // Prevent multiple death triggers
        view.RPC("Toggle", RpcTarget.All, false); // Disable local player's components
        if(view.IsMine)
        {
            health.camera.Shake(shakeDuration, shakeMagnitude);
            StartCoroutine(RespawnTimer()); // Trigger respawn
        }
    }



    IEnumerator RespawnTimer()
    {
        yield return new WaitForSeconds(respawnDelay); // Wait for respawn delay
            // Set up respawn logic
            transform.position = respawnArea.position; // Teleport to respawn area
            view.RPC("Toggle", RpcTarget.All, true);


        // Reset health
        health.healthAmount = 100; // Reset health to full locally
            health.fillImage.fillAmount = health.healthAmount / 100; // Update the local health bar UI

            // Broadcast the updated health amount to all clients
            view.RPC("UpdateHealthUI", RpcTarget.All, view.ViewID, health.healthAmount);
            health.isDead = false;
    }



    /* Use: On Death, Disables/Enables Player Mechanics
     * Params: bool
     * 1) Toggle Colliders 
     * 2) Reset Velocity
     * 3) Toggle rb simulation
     * 4) Toggle gun mechanics
     * 5) Toggle visuals (sprites, ammo count, hand)
     */
    [PunRPC]
    private void Toggle(bool toggle)
    {
        foreach (var c in colliders) // Cycle through colliders
        {
            c.enabled = toggle;
        }

        rb.velocity = Vector3.zero; // Reset velocity
        rb.simulated = toggle;  // Stop/Enable physics
        manager.enabled = toggle;   // Stop/Enable shooting

        foreach (var r in renderers) // Cycle through sprite renderers
        {
            r.enabled = toggle;
        }

        hand.SetActive(toggle); // Toggle hand
        ammoCount.gameObject.SetActive(toggle); // Toggle ammoCount
    }
}
