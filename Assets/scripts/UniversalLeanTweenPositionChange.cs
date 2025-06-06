using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UniversalLeanTweenPositionChange : MonoBehaviour
{
    public LeanTweenType type;
    public float duration;


    public void AnimatePosition(Transform target)
    {
        LeanTween.moveLocalX(gameObject, target.position.x, duration).setEase(type);
    }
}
