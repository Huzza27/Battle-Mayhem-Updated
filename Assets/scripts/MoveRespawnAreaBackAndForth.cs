using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveRespawnAreaBackAndForth : MonoBehaviour
{
    public Transform pointA;  // First position
    public Transform pointB;  // Second position
    public float moveDuration = 5f; // Time to move from A to B (slower movement)

    void Start()
    {
        MoveToB();
    }

    void MoveToB()
    {
        LeanTween.move(gameObject, pointB.position, moveDuration)
            .setEase(LeanTweenType.linear) // Keeps it at a constant speed
            .setOnComplete(MoveToA); // When it reaches B, move back to A
    }

    void MoveToA()
    {
        LeanTween.move(gameObject, pointA.position, moveDuration)
            .setEase(LeanTweenType.linear)
            .setOnComplete(MoveToB); // When it reaches A, move back to B
    }
}
