using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class ESCMenuListener : MonoBehaviourPunCallbacks
{
    [Header("Primary Pause Menu")]
    public GameObject ESCMenuCanvas;
    private bool isEnabled = false;
    public static bool isPaused;
    public PhotonView view;
    public Button settingsButton;

    private void Start()
    {
        AddSettingsListener();
        ResetPauseState();
    }

    private void OnDisable()
    {
        ResetPauseState();
    }

    public void ResetPauseState()
    {
        isEnabled = false;
        isPaused = false;
        if (ESCMenuCanvas != null)
        {
            ESCMenuCanvas.SetActive(false);
        }
    }

    void AddSettingsListener()
    {
        if (settingsButton != null)
        {
            settingsButton.onClick.AddListener(() =>
            {
                SettingsMenuManager.Instance.OpenCanvas();
            });
        }
    }

    public void LeaveRoomButton()
    {
        ResetPauseState();
        view.RPC("ResetGameManager", RpcTarget.All);
        RoomManager.Instance.LeaveRoom();
    }

    [PunRPC]
    void ResetGameManager()
    {
        GameManager.Instance.Reset();
    }

    private void Update()
    {
        if (GameManager.Instance.gameOver)
        {
            return;
        }
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            TogglePauseMenu(!isEnabled);
        }
    }

    public void TogglePauseMenu(bool enabled)
    {
            ESCMenuCanvas.SetActive(enabled);
            isPaused = enabled;
            isEnabled = enabled;

            Debug.Log($"Pause state changed - isPaused: {isPaused}, isEnabled: {isEnabled}");
    }

    public void LeavingConfirmation(GameObject window)
    {
        if (window != null)
        {
            bool isActive = window.activeSelf;
            window.SetActive(!isActive);
        }
    }

    public override void OnLeftRoom()
    {
        ResetPauseState();
    }

    private void OnDestroy()
    {
        ResetPauseState();
    }
}