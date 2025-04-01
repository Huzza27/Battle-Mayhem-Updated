using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using UnityEngine.UI;
using JetBrains.Annotations;
using static UnityEngine.ParticleSystem;
using System;
using System.Security.Cryptography;
using UnityEngine.Networking;
using System.Linq;

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
    public AudioSource respawnAudio;
    public AudioClip deathAudio;
    public PlayerStateManager playerState;

    [SerializeField] ParticleSystem deathParticles;

    //events
    public event Action OnPlayerEliminated;

    private void Awake()
    {
        respawnArea = GameObject.FindGameObjectWithTag("RespawnArea").transform;
    }

    private void OnEnable()
    {
        GameManager.OnGameReset += ResetPlayerState;
    }

    private void OnDisable()
    {
        GameManager.OnGameReset -= ResetPlayerState;
    }



    /* PUN RPC METHOD --> Called from DeathCollider
     * Runs Death Logic
     */
    [PunRPC]
    public void Death()
    {
        if (GameManager.Instance.CurrentState == GameManager.GameState.GameOver)
        {
            return;
        }

        health.healthAmount = 0; // Explicitly set health to zero
        health.fillImage.fillAmount = 0; // Update health bar UI
        health.isDead = true; // Prevent multiple death triggers

        view.RPC("Toggle", RpcTarget.All, false); // Disable player components
        ParticleEffects();
        transform.position = respawnArea.position;
        respawnAudio.PlayOneShot(deathAudio);

        if (view.IsMine)
        {
            health.camera.Shake(shakeDuration, shakeMagnitude);
            view.RPC("UpdateLifeCounterOnAllClients", RpcTarget.All, view.ViewID);

            // If the player is out of lives, remove them from the room property list
            if (health.lives <= 0)
            {
                health.canRespawn = false;
                Debug.Log("Player has been eliminated and will not respawn.");
                playerState.ChangePlayerState(PlayerState.Dead);
                // Disable the player instead of destroying them
                view.RPC("Toggle", RpcTarget.All, false);
                return;
            }


            StartCoroutine(RespawnTimer());
        }
    }
   
    public void ResetPlayerState()
    {
        transform.position = respawnArea.position;
        view.RPC("Toggle", RpcTarget.All, true); // Re-enable components
    }

    void ParticleEffects()
    {
        // Instantiate a new particle system
        Instantiate(deathParticles.gameObject, transform.position, Quaternion.identity);
    }

    IEnumerator RespawnTimer()
    {
        if (!health.canRespawn)
        {
            yield break; // Stop the coroutine if player cannot respawn
        }

        yield return new WaitForSeconds(respawnDelay); // Wait for respawn delay

        view.RPC("SwapItemsToOriginal", RpcTarget.All);

        // Reset player state
        view.RPC("Toggle", RpcTarget.All, true); // Re-enable components

        // Reset health and UI
        health.healthAmount = 100; // Reset health locally
        health.fillImage.fillAmount = health.healthAmount / 100; // Update health bar UI

        // Broadcast updated health to all clients
        view.RPC("UpdateHealthUI", RpcTarget.All, view.ViewID, health.healthAmount);

        health.isDead = false; // Ensure death state is reset
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

        hand.GetComponent<Renderer>().enabled = toggle;
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
