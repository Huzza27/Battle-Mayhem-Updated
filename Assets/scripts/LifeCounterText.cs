using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LifeCounterText : MonoBehaviour
{
    [SerializeField] PhotonView view;
    [SerializeField] TMP_Text textBox;

    [PunRPC]
    public void SetLives(int num)
    {
        textBox.text = num.ToString();
    }
}
