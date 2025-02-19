using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class MaintainBulletCountFacing : MonoBehaviour
{
    public Image image;
    public TextMeshProUGUI usernameText;
   // Update is called once per frame
    void Update()
    {
        image.GetComponent<RectTransform>().rotation = Quaternion.identity;
        usernameText.GetComponent<RectTransform>().rotation = Quaternion.identity;
    }
}
