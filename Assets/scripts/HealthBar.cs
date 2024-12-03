using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{

    [SerializeField] private Image fillImage;
    [SerializeField] private TMP_Text livesDisplay;
    [SerializeField] private Image dashUI;
    [SerializeField] private Image icon;

    public Image GetFillImage()
    {
        return fillImage;
    }

    public Image GetDashUI()
    {
        return dashUI;
    }

    public void SetLivesDisplay(string num)
    {
        livesDisplay.text = num;
    }
    public Image GetIcon()
    {
        return icon;
    }

    public TMP_Text GetLivesDisplay()
    {
        return livesDisplay;
    }

    public PhotonView GetLivesDisplayView()
    {
        return livesDisplay.GetComponent<PhotonView>();
    }
}
