using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkedCosmeticHandler : MonoBehaviour
{
    public PhotonView view;
    public SpriteRenderer body, hat;
    CosmeticDatabase database = DBRetrieval.instance.CosmeticDatabase;

    int currentHatID, currentBodyID;

    [PunRPC]
    public void SetPlayerOutfit(int viewID)
    {
        PhotonView view = PhotonView.Find(viewID);
        if(view != null)
        {
            object hatID, bodyID;
            if(view.Owner.CustomProperties.TryGetValue("HatID", out hatID) && view.Owner.CustomProperties.TryGetValue("BodyID", out bodyID))
            {
                CosmeticItem bodyCosmetic = database.GetItem(ItemType.Body, (int)bodyID);
                CosmeticItem hatCosmetic = database.GetItem(ItemType.Hat,  (int)hatID);

                body.sprite = bodyCosmetic.inGameSprite;
                hat.sprite = hatCosmetic.inGameSprite;

                currentBodyID = bodyCosmetic.ItemID;
                currentHatID = hatCosmetic.ItemID;
            }
        }
    }

    public int GetID(ItemType type)
    {
        if(type == ItemType.Body)
        {
            return currentBodyID;
        }
        else
        {
            return currentHatID;
        }
    }
}
