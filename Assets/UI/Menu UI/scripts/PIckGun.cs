using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PIckGun : MonoBehaviour
{

    public Image container;
    public Image buttonDis;

    public bool isShowing = false;

    public Sprite[] guns;

    // Start is called before the first frame update

    public void ToggleContainer()
    {
        if (isShowing == false)
        {
            container.gameObject.SetActive(true);
            isShowing = true;
        }

        else
        {
            container.gameObject.SetActive(false);
            isShowing = false;
        }
    }

    public void CloseContainer()
    {
        if(isShowing)
        {
            container.gameObject.SetActive(false);
            isShowing = false;
        }
    }

    public void SelectButton(int index)
    { 

    }
}
