using Photon.Pun;
using UnityEngine;

public class SwitchMap : MonoBehaviour
{
    public Sprite[] maps;
    private int currentIndex = 0;
    public SpriteRenderer renderer;
    public PhotonView view;
    
    public void Left()
    {
        view.RPC("CycleLeftForAllClients", RpcTarget.All);
    }
    
    public void Right()
    {
        view.RPC("CycleRightForAllClients", RpcTarget.All);
    }


    [PunRPC]
    private void CycleLeftForAllClients()
    {
        if(currentIndex != 0) 
        {
            currentIndex -= 1;
            renderer.sprite = maps[currentIndex];
        }
        else
        {
            currentIndex = maps.Length-1;
            renderer.sprite = maps[currentIndex];
        }
        GameManager.Instance.MapSelection = currentIndex;
    }

    [PunRPC]
    public void CycleRightForAllClients()
    {
        if (currentIndex != maps.Length-1)
        {
            currentIndex += 1;
            renderer.sprite = maps[currentIndex];
        }
        else
        {
            currentIndex = 0;
            renderer.sprite = maps[currentIndex];
        }
        GameManager.Instance.MapSelection = currentIndex;
    }
    
}
