using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SwitchMap : MonoBehaviour
{
    public Sprite[] maps;
    private int currentIndex = 0;
    public Image renderer;
    public PhotonView view;
    public TextMeshProUGUI mapName;
    public string[] mapNames;
    public Image temporaryFix;
    
    public void Left()
    {
        view.RPC("CycleLeftForAllClients", RpcTarget.AllBuffered);
    }
    
    public void Right()
    {
        view.RPC("CycleRightForAllClients", RpcTarget.AllBuffered);
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
        mapName.text = mapNames[currentIndex];
        GameManager.Instance.MapSelection = currentIndex;

        if(currentIndex == 0)
        {
            temporaryFix.gameObject.SetActive(true);
        }
        else
        {
            temporaryFix.gameObject.SetActive(false);
        }
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
        mapName.text = mapNames[currentIndex];
        GameManager.Instance.MapSelection = currentIndex;

        if (currentIndex == 0)
        {
            temporaryFix.gameObject.SetActive(true);
        }
        else
        {
            temporaryFix.gameObject.SetActive(false);
        }
    }
    
}
