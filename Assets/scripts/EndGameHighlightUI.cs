using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class EndGameHighlightUI : MonoBehaviour
{
    public TextMeshProUGUI highlightTitle;
    public TextMeshProUGUI highlightDescription;
    public Image body, crown;

    public void SetHighLightContent(string title, string description, Player player, bool isWinner)
    {

        highlightTitle.text = title;
        highlightDescription.text = description;
        body.sprite = GetBodySprite(player);
        if (isWinner)
        {
            crown.gameObject.SetActive(true);
        }
    }

    public Sprite GetBodySprite(Player player)
    {

        object bodyID;
        if (player.CustomProperties.TryGetValue("BodyID", out bodyID))
        {
            return DBRetrieval.instance.CosmeticDatabase.GetItem(ItemType.Body, (int)bodyID).itemSprite;
        }
        return null;
    }
}
