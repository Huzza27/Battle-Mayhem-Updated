using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using System.Linq;

public class TriggerEndGame : MonoBehaviourPunCallbacks
{
    public Sprite[] colorSprites;
    public Animator bgController, textController;
    public Image WinnerDisplay;
    public Toggle p1Toggle, p2Toggle;

        private void Start()
        {
            PhotonNetwork.AddCallbackTarget(this);
        }

        private void OnDestroy()
        {
            PhotonNetwork.RemoveCallbackTarget(this);
        }

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.ContainsKey("Winner") )
        {
            int winnerActorNumber = (int)propertiesThatChanged["Winner"];
            CelebrateVictory(winnerActorNumber);
        }
        if(propertiesThatChanged.ContainsKey("StartRematch"))
        {
            if ((bool)propertiesThatChanged["StartRematch"])
            {
               ResetAnimators();
               p1Toggle.isOn = false;
               p2Toggle.isOn = false;
            }
        }
    }


    private void CelebrateVictory(int actorNumber)
        {
            GameManager.Instance.gameOver = true;
            Player winningPlayer = null;
        int spriteIndex = 0; //Default for blue if no color is picked
            foreach(Player player in PhotonNetwork.PlayerList)
            {
                if(player.ActorNumber == actorNumber)
                {
                    winningPlayer = player;
                }
            }    
            //Set Image Color
            object colorIndex;
        if (winningPlayer.CustomProperties.TryGetValue("PlayerColor", out colorIndex))
        {
            spriteIndex = (int)colorIndex;
        }
        else
        {
            colorIndex = PhotonNetwork.IsMasterClient ? 0 : 2;
        }

        WinningAnimation((int)colorIndex);
    }

    public void WinningAnimation(int index)
    {
        WinnerDisplay.sprite = colorSprites[index];
        bgController.Play("BackgroundFadeIn");
        textController.Play("WinnerUiAnnimatiojn");
    }

     public void ResetAnimators()
     {
        textController.Play("ResetEndScreen");
        bgController.Play("BackgorunDFadeOut");
     }
}
