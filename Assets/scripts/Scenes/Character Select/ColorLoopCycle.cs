using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorLoopCycle : MonoBehaviour
{
    // The Image component attached to this GameObject
    private RectTransform myRectTransform;

    // Optional: Assign the images to check for overlap, or you can find them dynamically.
    public Image[] otherImages;
    public RectTransform LeftMostDetector;
    public RectTransform RightMostDetector;
    public RectTransform LeftBeginning;
    public RectTransform RightBeginning;

    void Start()
    {
        // Get the RectTransform of the current object (which has the script)
        myRectTransform = GetComponent<RectTransform>();

        // If you want to detect other images dynamically, uncomment the line below
        // otherImages = FindObjectsOfType<Image>();
    }

    void Update()
    {
        // Check for overlaps with each other image in the list
        foreach (Image otherImage in otherImages)
        {
            // Get the RectTransform of the other image
            RectTransform otherRectTransform = otherImage.GetComponent<RectTransform>();

            // Check if the two RectTransforms overlap
            if (IsOverlapping(RightMostDetector, otherRectTransform))
            {
                otherRectTransform.position = LeftBeginning.position;
                return;
            }
            else if(IsOverlapping(LeftMostDetector, otherRectTransform))
            {
                otherRectTransform.position = RightBeginning.position;
            }
        }
    }

    // Method to check if two UI RectTransforms are overlapping
    private bool IsOverlapping(RectTransform rect1, RectTransform rect2)
    {
        Rect rectA = GetWorldRect(rect1);
        Rect rectB = GetWorldRect(rect2);
        return rectA.Overlaps(rectB);
    }

    // Convert a RectTransform to a world space Rect
    private Rect GetWorldRect(RectTransform rt)
    {
        Vector3[] corners = new Vector3[4];
        rt.GetWorldCorners(corners);

        Vector3 bottomLeft = corners[0];
        Vector3 topRight = corners[2];

        return new Rect(bottomLeft.x, bottomLeft.y, topRight.x - bottomLeft.x, topRight.y - bottomLeft.y);
    }
}
