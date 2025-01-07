using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class MaintainBulletCountFacing : MonoBehaviour
{
    public Image image;
   // Update is called once per frame
    void Update()
    {
        image.GetComponent<RectTransform>().rotation = Quaternion.identity;
    }
}
