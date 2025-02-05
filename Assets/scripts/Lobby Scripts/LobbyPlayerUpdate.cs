using JetBrains.Annotations;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyPlayerUpdate : MonoBehaviourPunCallbacks
{
    //public TextMeshProUGUI playersCountText; // Assign this in the inspector with your UI Text

    public Image p1Image; 

    public Image firstPlayerOtherClientDisplayIMage, secondPlayerOtherClientDisplayImage;
    public GameObject movingPoint;

    //public RectTransform player1BannerLoc;
    public RectTransform player2BannerLoc;


    public Image redImage; // Array of colors represented as Sprites
    private bool isPlayer1 = false;
    public GameObject Container;

    //public TMP_InputField livesInput;
    private int lives;
    [SerializeField] PhotonView view;

    public Sprite redSprite, blueSprite;

    private void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        AssignImages();
    }

    public override void OnJoinedRoom()
    {
        AssignImages();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        AssignImages();
    }


    void AssignImages()
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount > 1)
        {
            if (PhotonNetwork.LocalPlayer.ActorNumber == 2)
            {
                SwapImages();
            }

            if (PhotonNetwork.LocalPlayer.ActorNumber == 1)
            {
                secondPlayerOtherClientDisplayImage.gameObject.SetActive(true);
                firstPlayerOtherClientDisplayIMage.gameObject.SetActive(false);
                secondPlayerOtherClientDisplayImage.gameObject.GetComponent<Button>().enabled = false;
            }
        }
    }

    public void SwapImages()
    {

        p1Image.sprite = redSprite;
        firstPlayerOtherClientDisplayIMage.gameObject.SetActive(true);
        secondPlayerOtherClientDisplayImage.gameObject.SetActive(false);
        redImage.sprite = blueSprite;
    }




    public void LoadGameScene()
    {
        if (PhotonNetwork.IsMasterClient) // Only the Master Client can initiate the scene change
        {
            PhotonNetwork.LoadLevel("Game");
        }
    }

}
