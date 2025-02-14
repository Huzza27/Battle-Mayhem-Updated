using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;

public class SetPlayerUserName : MonoBehaviour
{
    public TextMeshProUGUI usernameField;

    [PunRPC]
    public void SetUsername(string name)
    {
        usernameField.text = name;
    }
}
