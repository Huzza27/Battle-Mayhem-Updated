using System.Collections;
using UnityEngine;
using Photon.Pun;
using TMPro;
using Photon.Realtime;
using Unity.VisualScripting.Antlr3.Runtime;

public class ESCMenuListener : MonoBehaviourPunCallbacks
{
    [Header("Primary Pause Menu")]
    public GameObject ESCMenuCanvas; // Local player's pause menu
    private bool isEnabled = false;
    public static bool isPaused; // Tracks if the game is paused locally
    public PhotonView view;

    [Header("Secondary Pause Menu")]
    public GameObject SecondaryPauseMenu; // Shown to other players
    public TextMeshProUGUI SecondaryPauseTextObject; // Text indicating who paused the game

    private void Update()
    {
        if (view.IsMine && Input.GetKeyUp(KeyCode.Escape))
        {
            // Toggle the pause menu for the local player
            TogglePauseMenu(!isEnabled);
        }
    }

    public void TogglePauseMenu(bool enabled)
    {
        if (view.IsMine) // Only the local player can toggle the pause
        {
            ESCMenuCanvas.SetActive(enabled);
            isPaused = enabled;
            isEnabled = enabled;
        }
    }

    public void LeavingConfirmation(GameObject window)
    {
        // Toggle the confirmation window
        bool isActive = window.activeSelf;
        window.SetActive(!isActive);
    }
}