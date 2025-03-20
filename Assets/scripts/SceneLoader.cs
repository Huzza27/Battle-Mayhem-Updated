using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;



public class SceneLoader : MonoBehaviour
{
    private void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = false;
    }
    public void LoadSceneClientSide(int num)
    {
        SceneManager.LoadScene(num);  
        if(num == 3) //If we go back to the lobby, leave the room
        {
            PhotonNetwork.LeaveRoom();
        }  
    }

    public void LoadSceneNetworked(int num)
    {
        PhotonNetwork.LoadLevel(num);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
