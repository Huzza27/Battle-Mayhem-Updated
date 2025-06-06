using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.ReorderableList;
using UnityEngine;
using UnityEngine.UI;

public class CosmeticItemDisplay : MonoBehaviour
{
    public Image cosmeticDisplay;
    private CosmeticItem item;
    
    public void InitializeDisplay(CosmeticItem item)
    {
        this.item = item;
        cosmeticDisplay.sprite = item.itemSprite;
    }

    public void EquipCosmetic()
    {
            item.cosmeticToChange.sprite = item.itemSprite;
    }

    public void UpdateOutfitData()
    {
        switch (item.type)
        {
            case ItemType.Hat:
                LockerData.HatID = item.ItemID;
                break;
            case ItemType.Body:
                LockerData.BodyID = item.ItemID;
                break;
        }
    }

}
