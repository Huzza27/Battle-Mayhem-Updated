using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CloseDialougeOnClickOut : MonoBehaviour
{
    public GameObject boxToClose;

    public void CloseBox()
    {
        if(boxToClose.activeSelf)
        {
            boxToClose.SetActive(false);
        }
    }
}
