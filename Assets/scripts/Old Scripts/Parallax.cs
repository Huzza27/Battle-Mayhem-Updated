using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parallax : MonoBehaviour
{
    public float parallaxFactor;
    public Transform cameraTransform;
    private Vector3 lastCameraPosition;


    void LateUpdate()
    {
        Vector3 deltaMovement = cameraTransform.position - lastCameraPosition;
        transform.position += new Vector3(deltaMovement.x * parallaxFactor, 0f);
        lastCameraPosition = cameraTransform.position;
    }
}
