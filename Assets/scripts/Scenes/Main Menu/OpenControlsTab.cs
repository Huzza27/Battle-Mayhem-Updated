using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;   

public class OpenControlsTab : MonoBehaviour
{
    public Image controlsTab;
    public Image credits;

    public void ToggleImage()
    {
        controlsTab.gameObject.SetActive(!controlsTab.gameObject.activeSelf);
    }

    public void ToggleCredits()
    {
        credits.gameObject.SetActive(!credits.gameObject.activeSelf);
    }
}
