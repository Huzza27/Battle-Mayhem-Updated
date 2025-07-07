using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum ItemType
{
    Hat,
    Body
}
[CreateAssetMenu(fileName ="CostmeticStoreItem")]
public class CosmeticItem : ScriptableObject
{
    public ItemType type;
    public Image cosmeticToChange;
    public int ItemID;
    public Sprite itemSprite;
    public Sprite inGameSprite;
    public string itemName;
    public float cost;
    public bool unlocked;
    public void SetUIToChange(Image image)
    {
        cosmeticToChange = image;
    }
}
