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
   }

    public void LoadSceneNetworked(int num)
    {
        PhotonNetwork.LoadLevel(num);
    }
}
