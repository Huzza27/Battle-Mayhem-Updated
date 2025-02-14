using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class EnableOrDisableCreateRoomBUttons : MonoBehaviour
{
    public Button button;
    public InputField input;

    private void Update()
    {
        if(input.text == string.Empty)
        {
            button.interactable = false;
        }
        else
        {
            button.interactable = true;
        }
    }
}
