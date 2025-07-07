using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EyeFollow : MonoBehaviour
{
    [Header("References")]
    public RectTransform playerDisplayRect;
    public RectTransform eyeL, eyeR;

    [Header("Settings")]
    public float eyeMoveSpeed = 5f;
    public float maxDistance = 10f;

    private RectTransform thisRect;

    private Vector2 eyeLOrigin;
    private Vector2 eyeROrigin;

    public LayoutElement layout;

    private void Awake()
    {
        layout.ignoreLayout = true;
        thisRect = GetComponent<RectTransform>();

        eyeLOrigin = eyeL.anchoredPosition;
        eyeROrigin = eyeR.anchoredPosition;
    }

    private void Update()
    {
        FollowWithEyes();
    }

    public void MoveEyes()
    {
        if (playerDisplayRect != null)
        {
            thisRect.position = playerDisplayRect.position;
        }
    }


    public void FollowWithEyes()
    {
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(thisRect, Input.mousePosition, null, out Vector2 localMousePos))
            return;

        AnimateEye(eyeL, eyeLOrigin, localMousePos);
        AnimateEye(eyeR, eyeROrigin, localMousePos);
    }

    private void AnimateEye(RectTransform eye, Vector2 origin, Vector2 localMousePos)
    {
        // Calculate offset FROM the eye origin TO the mouse
        Vector2 direction = localMousePos - origin;

        // Clamp how far the eye can move
        direction = Vector2.ClampMagnitude(direction, maxDistance);

        // Final position is origin + clamped offset
        Vector2 targetPos = origin + direction;

        // Smooth movement
        eye.anchoredPosition = Vector2.Lerp(eye.anchoredPosition, targetPos, Time.deltaTime * eyeMoveSpeed);
    }
}
