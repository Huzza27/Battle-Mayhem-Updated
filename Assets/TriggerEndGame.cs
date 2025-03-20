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
        view.RPC("CelebrateVictory", RpcTarget.All, ActorNumber);
    }

    [PunRPC]
    void CelebrateVictory(int actorNumber)
    {
        GameManager.Instance.gameOver = true;
        // Find the winning player
        Player winningPlayer = PhotonNetwork.PlayerList.FirstOrDefault(p => p.ActorNumber == actorNumber);
        if (winningPlayer == null)
        {
            Debug.LogError($"Could not find player with ActorNumber: {actorNumber}");
            return;
        }

        Debug.Log($"Found winning player: {winningPlayer.NickName}");

        // Get the player's color
        int spriteIndex = 0; // Default blue
        if (winningPlayer.CustomProperties.TryGetValue("PlayerColor", out object colorChoice))
        {
            spriteIndex = (int)colorChoice;
            Debug.Log($"Winner color index from properties: {spriteIndex}");
        }
        else
        {
            // Fallback color assignment
            spriteIndex = winningPlayer.IsMasterClient ? 0 : 2;
            Debug.Log($"Using fallback color index: {spriteIndex}");
        }

        // Verify sprite index is within bounds
        if (spriteIndex >= 0 && spriteIndex < colorSprites.Length)
        {
            WinningAnimation(spriteIndex);

            // After a short delay, show the rematch UI
            StartCoroutine(ShowRematchUIAfterDelay(2.0f));
        }
        else
        {
            Debug.LogError($"Invalid sprite index: {spriteIndex}. Array length: {colorSprites.Length}");
        }
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

    public void WinningAnimation(int index)
    {
        Debug.Log($"Playing winning animation with sprite index: {index}");
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