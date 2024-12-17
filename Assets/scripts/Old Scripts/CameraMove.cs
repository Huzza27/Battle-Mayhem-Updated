using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using JetBrains.Annotations;

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

    // Define the horizontal bounds of the map (adjust these values to fit your map size)
    public float leftBound = 4.05f; // Leftmost X value the camera can go
    public float rightBound = 11.05f; // Rightmost X value the camera can go
    public float bottomBound = 1.05f; // Lowest Y value the camera can go (e.g., the floor level)

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

    public void Shake(float duration, float magnitude)
    {
        StartCoroutine(ShakeCoroutiune(duration, magnitude));
    }
    public IEnumerator ShakeCoroutiune(float duration, float magnitude)
    {
        Vector3 originalPosition = transform.localPosition;
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = new Vector3(transform.localPosition.x + x, transform.localPosition.y + y, originalPosition.z);
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
            // Calculate the player's movement delta
            Vector3 playerDelta = player.position - lastPlayerPosition;

            // Calculate the target position for the camera
            Vector3 targetCameraPosition = transform.position;

            // Horizontal follow logic
            float targetX = transform.position.x + playerDelta.x * horizontalFollowRatio;

            // Check if the target X position is within the horizontal bounds
            if (targetX > leftBound && targetX < rightBound)
            {
                targetCameraPosition.x = targetX;
            }
            else
            {
                // Keep the camera's X position fixed if it hits the horizontal bounds
                targetCameraPosition.x = Mathf.Clamp(transform.position.x, leftBound, rightBound);
            }

            // Vertical follow logic
            float targetY = transform.position.y + playerDelta.y * verticalFollowRatio;

            // Check if the target Y position is above the bottom bound
            if (targetY >= bottomBound)
            {
                targetCameraPosition.y = targetY;
            }
            else
            {
                // Keep the camera's Y position fixed at the bottom bound
                targetCameraPosition.y = Mathf.Clamp(transform.position.y, bottomBound, Mathf.Infinity);
            }

            // Smoothly transition to the target position using damping
            transform.position = Vector3.SmoothDamp(transform.position, targetCameraPosition, ref velocity, damping);

            // Update the last player position for the next frame
            lastPlayerPosition = player.position;
        }
    }
}