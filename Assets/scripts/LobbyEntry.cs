using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyEntry : MonoBehaviour
{
    public TextMeshProUGUI roomNameText;
    public TextMeshProUGUI playerCountText;
    public Button joinButton;
    private string roomName;

    public void SetRoomInfo(string name, int currentPlayers, int maxPlayers, string roomCode, System.Action<string> onJoinCallback)
    {
        roomName = name;
        roomNameText.text = name;  // This line is correct now
        playerCountText.text = $"{currentPlayers}/{maxPlayers}";

        // Fix button listener to pass room name to callback
        joinButton.onClick.RemoveAllListeners();
        joinButton.onClick.AddListener(() => onJoinCallback(roomCode));
    }
}