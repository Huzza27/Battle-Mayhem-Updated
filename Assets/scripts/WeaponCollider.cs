using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponCollider : MonoBehaviour
{
    PhotonView targetView;
    public Item item;
    public PhotonView view;
    [SerializeField] Collider2D collider;
    public Rigidbody2D rb;

    [Header("AOE")]
    public Transform AOESpawn;
    [SerializeField] GameObject aoePrefab; // Prefab for the AOE effect
    private GameObject currentAOE; // Reference to the current AOE spawned by this player

    [Header("Ground Launching")]
    [SerializeField] float launchSpeed = 10f; // The overall strength of the launch
    [SerializeField] float horizontalInfluence = 1f; // How much the mouse direction affects horizontal velocity
    [SerializeField] float angleThreshold = 30f; // Maximum angle from straight down to allow launching

    [Header("References")]
    [SerializeField] Transform gunParent; // Reference to the parent with rotation logic

    private void OnTriggerEnter2D(Collider2D collision)
    {
        collider.enabled = false;
        if (collision.tag == "Player")
        {
            Debug.Log("Hit a player");
            targetView = collision.gameObject.GetPhotonView();
            targetView.RPC("ReduceHealth", RpcTarget.All, item.GetDamage(), PhotonNetwork.LocalPlayer.ActorNumber);
        }
        else if (collision.tag == "Ground" && view.IsMine)
        {
            if (view.IsMine)
            {
                HandleGroundLaunch();
            }
        }
    }

    private void HandleGroundLaunch()
    {
        if (gunParent != null)
        {
            // Get the rotation of the parent object
            float zRotation = -gunParent.eulerAngles.z;

            // Normalize the rotation to a value between -180 and 180
            if (zRotation > 180f)
            {
                zRotation -= 360f;
            }

            // Check if the rotation is within the angle threshold for launching
            if (Mathf.Abs(zRotation + 90f) <= angleThreshold)
            {
                // Calculate the launch vector based on rotation
                Vector2 launchVector = new Vector2(
                    Mathf.Cos(zRotation * Mathf.Deg2Rad) * horizontalInfluence,
                    Mathf.Sin(zRotation * Mathf.Deg2Rad)
                ).normalized * launchSpeed;

                // Apply the force to the player's Rigidbody
                //SpawnAOE(); // Spawn the AOE before launching 
                rb.AddForce(-launchVector, ForceMode2D.Impulse);
            }
        }
        else
        {
            Debug.LogError("Gun parent reference is missing.");
        }
    }

    private void SpawnAOE()
    {
        if (aoePrefab != null)
        {
            if (currentAOE == null)
            {
                // Instantiate the AOE using PhotonNetwork and track it
                currentAOE = PhotonNetwork.Instantiate(aoePrefab.name, AOESpawn.position, Quaternion.identity);
                currentAOE.GetComponentInChildren<ShotGunAOE>().shooterView = view;
            }
            else
            {
                Debug.Log("Player already has an active AOE.");
            }
        }
        else
        {
            Debug.LogError("AOE prefab reference is missing.");
        }
    }

    [PunRPC]
    private void DestroyCurrentAOE()
    {
        if (currentAOE != null)
        {
            PhotonNetwork.Destroy(currentAOE);
            currentAOE = null;
        }
    }
}
