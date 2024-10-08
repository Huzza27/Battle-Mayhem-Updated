using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using UnityEngine.UI;

public class TriggerEndGame : MonoBehaviourPunCallbacks
{
    public Sprite[] colorSprites;
    public Animator bgController, textController;
    public Image WinnerDisplay;

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
        if (propertiesThatChanged.ContainsKey("Winner"))
        {
            int winnerActorNumber = (int)propertiesThatChanged["Winner"];
            CelebrateVictory(winnerActorNumber);
        }
    }

    private void CelebrateVictory(int actorNumber)
        {
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

            WinnerDisplay.sprite = colorSprites[spriteIndex];
            bgController.Play("BackgroundFadeIn");
            textController.Play("WinnerUiAnnimatiojn");
    }
    }
