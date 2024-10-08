using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class CameraMove : MonoBehaviour
{
    public PhotonView view;
    public Transform player;
    public float horizontalFollowRatio = 1f / 12f; // Camera moves 1 unit horizontally for every 12 units the player moves
    public float verticalFollowRatio = 1f / 4f;   // Camera moves 1 unit vertically for every 4 units the player moves
    public float damping = 0.15f;                 // Damping effect for smooth 

    private Vector3 velocity = Vector3.zero;
    private Vector3 lastPlayerPosition;
    private Vector3 cameraOffset;

    void Start()
    {
        if (view.IsMine)
        {
            if (player == null)
            {
                Debug.LogError("Player transform is not assigned to the camera script.");
                return;
            }

            lastPlayerPosition = player.position;
            cameraOffset = transform.position - player.position;
            player.gameObject.GetComponent<Health>().camera = this.gameObject.GetComponent<CameraMove>();
        }
        else 
        {
            Destroy(this.gameObject);
        }
    }
    public IEnumerator Shake(float duration, float magnitude)
    {
        Vector3 originalPosition = transform.localPosition;
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = new Vector3(x, y, originalPosition.z);
            elapsed += Time.deltaTime;

            yield return null;  // Wait until the next frame before continuing the loop
        }

        // Reset the position of the camera after shaking is done
        transform.localPosition = originalPosition;
    }


void FixedUpdate()
    {
        if (view.IsMine)
        {
            Vector3 playerDelta = player.position - lastPlayerPosition;

            // Apply different ratios for horizontal and vertical movement
            Vector3 targetCameraPosition = transform.position + new Vector3(playerDelta.x * horizontalFollowRatio, playerDelta.y * verticalFollowRatio, 0);

            // Apply damping for smooth transition
            transform.position = Vector3.SmoothDamp(transform.position, targetCameraPosition, ref velocity, damping);

            // Update the last player position for the next frame
            lastPlayerPosition = player.position;
        }
    }
}