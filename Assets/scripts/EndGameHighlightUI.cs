using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EndGameHighlightUI : MonoBehaviour
{
    public TextMeshProUGUI highlightTitle;
    public TextMeshProUGUI highlightDescription;
    public Image avatar, crown;

    public void SetHighLightContent(string title, string description, Sprite avatarSprite, bool isWinner)
    {
        highlightTitle.text = title;
        highlightDescription.text = description;
        avatar.sprite = avatarSprite;
        if (isWinner)
        {
            crown.gameObject.SetActive(true);
        }
    }
}
