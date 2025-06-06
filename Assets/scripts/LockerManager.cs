using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.UI;

enum Tab
{ 
    Hat,
    Body
}

public class LockerManager : MonoBehaviour
{
    public Image hatDisplay, bodyDisplay;
    private Image currentCosmeticdisplayToChange;
    private Transform currentSpawnParent;
    public GameObject displayItemPrefab;
    public Transform hatDisplayItemParent, bodyDisplayItemParent;

    public CosmeticDatabase db;

    public List<GameObject> displayItems;

    GameObject tempDisplayItem;

    
    


    private Tab currentTab;

    #region UI LOADING
    public void LoadContent(GameObject itemConatiner)
    {
        currentCosmeticdisplayToChange = null;
        List<CosmeticItem> items = itemConatiner.GetComponent<CosmeticTypeInformationContainer>().items;

        if(displayItems.Count > 1)
        {
            foreach (GameObject item in displayItems)
            {
                Destroy(item);
            }
            displayItems.Clear();

        }

        foreach (CosmeticItem item in items) 
        {
           if(currentCosmeticdisplayToChange == null)
           {
                switch (item.type) 
                {
                    case ItemType.Hat:
                        currentTab = Tab.Hat;
                        currentCosmeticdisplayToChange = hatDisplay;
                        currentSpawnParent = hatDisplayItemParent;
                        
                        break;
                    case ItemType.Body:
                        currentTab = Tab.Body;
                        currentCosmeticdisplayToChange = bodyDisplay;
                        currentSpawnParent = bodyDisplayItemParent;
                        break;
                }
           }
           InitializeObjectWithExternalReferences(currentCosmeticdisplayToChange, currentSpawnParent, item);
        }
    }

    void InitializeObjectWithExternalReferences(Image display, Transform parent, CosmeticItem item)
    {
        item.SetUIToChange(display);
        tempDisplayItem = Instantiate(displayItemPrefab, parent);
        tempDisplayItem.transform.SetParent(parent);
        tempDisplayItem.GetComponent<CosmeticItemDisplay>().InitializeDisplay(item);
        displayItems.Add(tempDisplayItem);
    }
    #endregion
    #region Saving Outfit
    public void SaveOutfit()
    {
        PhotonNetwork.LocalPlayer.SetCustomProperties(LockerData.WholeOutfit);
        Debug.Log(PhotonNetwork.LocalPlayer.CustomProperties);
    }
    #endregion
}
