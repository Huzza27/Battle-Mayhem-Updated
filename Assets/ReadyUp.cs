using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using TMPro;

public class ReadyUp : MonoBehaviour
{
    int numPlayersReady = 0;
    public PhotonView view;
    public Button readyButton;
    public TextMeshProUGUI text;
    public SceneLoader sceneLoader;

    public void OnClick()
    {
        view.RPC("IncrementReadyPlayers", RpcTarget.AllBuffered);
        readyButton.gameObject.SetActive(false);
    }

    [PunRPC]
    private void IncrementReadyPlayers()
    {
        numPlayersReady++;
        text.text = numPlayersReady + "/2";

        // Check the number of players ready after the RPC update
        view.RPC("CheckNumPlayers", RpcTarget.All);
    }

    [PunRPC]
    public void CheckNumPlayers()
    {
        if (numPlayersReady == PhotonNetwork.CurrentRoom.PlayerCount)
        {
            PhotonNetwork.LoadLevel(4);
        }
    }
}
