using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;   

public class OpenControlsTab : MonoBehaviour
{
    public Image controlsTab;

    public void ToggleImage()
    {
        controlsTab.gameObject.SetActive(!controlsTab.gameObject.activeSelf);
    }
}
