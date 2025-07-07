using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using System.Linq;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;

public class TriggerEndGame : MonoBehaviourPunCallbacks
{
    public Sprite[] colorSprites;
    public EndGameHighlightUI[] statDisplays;
    public Animator bgController, textController;
    public Image WinnerDisplay;
    public Toggle p1Toggle, p2Toggle;
    public PhotonView view;

    // Reference to the RematchManager
    public RematchManager rematchManager;

    private void Start()
    {
        PhotonNetwork.AddCallbackTarget(this);

        // Find RematchManager if not assigned
        if (rematchManager == null)
        {
            rematchManager = FindObjectOfType<RematchManager>();
        }
    }

    private void OnEnable()
    {
        GameManager.OnGameReset += ResetAnimators;
        GameManager.OnGameEnd += EndGame;
    }

    private void OnDisable()
    {
        GameManager.OnGameReset -= ResetAnimators;
        GameManager.OnGameEnd -= EndGame;

    }

    private void OnDestroy()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }


    
    public void EndGame(int ActorNumber)
    {
        GameManager.Instance.ChangeState(GameManager.GameState.GameOver);
        ManageStatUI(ActorNumber);
        view.RPC("CelebrateVictory", RpcTarget.All, ActorNumber);
    }


    public void ManageStatUI(int winnerActorNumber)
    {
        // Prepare arrays of serializable data
        List<int> playerIds = new List<int>();
        List<string> titleNames = new List<string>();
        List<string> titleDescriptions = new List<string>();

        if (PhotonNetwork.IsMasterClient)
        {
            foreach (Player player in PhotonNetwork.PlayerList)
            {
                PlayerStats stat = MatchStatsManager.Instance.GetTitleEarnedByPlayer(player.ActorNumber);
                if (stat != null && stat.title != null)
                {
                    playerIds.Add(stat.playerId);
                    titleNames.Add(stat.title.name);
                    titleDescriptions.Add(stat.title.description);
                }
            }

            // Send each list as an array (Photon supports arrays of built-in types)
            view.RPC(
                "UpdateStatUIOverNetwork",
                RpcTarget.All,
                playerIds.ToArray(),
                titleNames.ToArray(),
                titleDescriptions.ToArray(),
                winnerActorNumber
            );
        }
    }



    [PunRPC]
    void UpdateStatUIOverNetwork(int[] playerIds, string[] titleNames, string[] titleDescriptions, int winnerActorNumber)
    {
        for (int i = 0; i < playerIds.Length; i++)
        {
            Player currentPlayer = PhotonNetwork.PlayerList.FirstOrDefault(p => p.ActorNumber == playerIds[i]);
            if (currentPlayer == null) continue;

            EndGameHighlightUI currentPanel = statDisplays[i];
            currentPanel.gameObject.SetActive(true);
            currentPanel.SetHighLightContent(titleNames[i], titleDescriptions[i], currentPlayer, winnerActorNumber == playerIds[i]);
        }
    }


    [PunRPC]
    void CelebrateVictory(int actorNumber)
    {
        
        // Find the winning player
        Player winningPlayer = PhotonNetwork.PlayerList.FirstOrDefault(p => p.ActorNumber == actorNumber);
        if (winningPlayer == null)
        {
            Debug.LogError($"Could not find player with ActorNumber: {actorNumber}");
            return;
        }

        Debug.Log($"Found winning player: {winningPlayer.NickName}");

        // Get the player's color
        

        // Verify sprite index is within bounds

            WinningAnimation();
            // After a short delay, show the rematch UI
            StartCoroutine(ShowRematchUIAfterDelay(2.0f));
    }



    private IEnumerator ShowRematchUIAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Show the rematch UI if we have a RematchManager
        if (rematchManager != null)
        {
            rematchManager.ShowRematchUI();
        }
    }

    public void WinningAnimation()
    {
        bgController.Play("BackgroundFadeIn");
        textController.Play("WinnerUiAnnimatiojn");
    }

    public void ResetAnimators()
    {
        textController.Play("ResetEndScreen");
        bgController.Play("BackgorunDFadeOut");
    }
}


[System.Serializable]
public struct PlayerTitleInfo
{
    public int playerId;
    public string titleName;
    public string titleDescription;

    public PlayerTitleInfo(int id, string t, string d)
    {
        playerId = id;
        titleName = t;
        titleDescription = d;
    }
}
