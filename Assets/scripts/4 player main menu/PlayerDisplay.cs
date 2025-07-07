using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Networking.Types;
using UnityEngine.UI;

public class PlayerDisplay : MonoBehaviour
{
    private Player player;
    public EyeFollow eyeFollow;
    public Image body, hat;
    CosmeticDatabase database;

    // UI-specific variables
    private Canvas parentCanvas;
    private RectTransform canvasRect;



    public void SetPlayer(int newPlayerActorNumber)
    {
        foreach (Player newPlayer in PhotonNetwork.PlayerList)
        {
            if (newPlayer.ActorNumber == newPlayerActorNumber)
            {
                player = newPlayer; break;
            }
            else
            {
                Debug.Log("No Player found for actor number " + newPlayerActorNumber);
            }
        }
    }

    private void Start()
    {
        SetCosmetic();
        eyeFollow.gameObject.SetActive(true);
        eyeFollow.MoveEyes();
    }
    
    public void SetCosmetic()
    {
        object hatID, bodyID;
        database = DBRetrieval.instance.CosmeticDatabase;
        if (player != null && player.CustomProperties.TryGetValue("HatID", out hatID) && player.CustomProperties.TryGetValue("BodyID", out bodyID))
        {
            hat.sprite = database.GetItem(ItemType.Hat, (int)hatID).inGameSprite;
            body.sprite = database.GetItem(ItemType.Body, (int)bodyID).inGameSprite;
        }
    }    
}