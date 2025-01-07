using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InputCheck : MonoBehaviour
{
    public TMP_InputField  inputField; // Assign your InputField in the Inspector
    public GameObject error;
    private int maxDigits = 3;

    private bool isLocked = false; // To prevent typing during error display

    public void CheckValidInteger()
    {
        if(inputField.text == string.Empty)
        {
            return;
        }
        if (!int.TryParse(inputField.text, out int result))
        {
            // Clear the input and show the error message
            inputField.text = "";
            ToggleError(true);

            // Lock the input field and invoke hiding the error after 3 seconds
            isLocked = true;
            Invoke("ResetError", 1f);
        }
    }

    public void RestrictToMaxDigits()
    {
        // Restrict input to a maximum number of digits
        if (inputField.text.Length > maxDigits)
        {
            inputField.text = inputField.text.Substring(0, maxDigits); // Truncate extra characters
        }
    }

    void ToggleError(bool show)
    {
        error.SetActive(show);
    }

    void ResetError()
    {
        // Hide the error message and unlock the input field
        ToggleError(false);
        isLocked = false;
    }

    // Optional: Disable typing when locked
    private void Update()
    {
        if (isLocked)
        {
            inputField.interactable = false;
        }
        else
        {
            inputField.interactable = true;
        }
    }
}
