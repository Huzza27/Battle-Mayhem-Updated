using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
using UnityEngine;
using UnityEngine.Video;

public class GameSetup : MonoBehaviour
{
    public PhotonView view;
    public GameObject hand;
    Item gunData;
    new SpriteRenderer renderer;
    public GunMechanicManager gunManager;
    public GameObject gunTip;
    public GameObject bodyObject;
    public Sprite[] colors;
    public BoxCollider2D collider;

    // Start is called before the first frame update

    private void Awake()
    {
        ResetGameSetupState();
    }

    public void ResetGameSetupState()
    {
        // Reset gun data
        gunData = gunManager.heldItem;
        if (renderer == null)
        {
            renderer = hand.GetComponent<SpriteRenderer>();
        }
        if (renderer != null && gunData != null)
        {
            renderer.sprite = gunData.icon; // Reset the hand sprite to the gun's icon
        }

        // Reset gun tip position and collider based on held item
        if (gunData != null)
        {
            if (gunData.gunTipYOffset != 0.0f)
            {
                AdjustGunTipPosition(gunData.gunTipYOffset, gunData);
            }
            else
            {
                gunTip.transform.localPosition = Vector2.zero; // Default position
            }
            MoveGunCollider(gunData);
        }

        // Reset body color to default or based on player properties
        if (view.IsMine)
        {
            view.RPC("SetPlayerColorForAllClients", RpcTarget.All, view.ViewID);
        }

        // Ensure collider is reinitialized
        if (collider != null)
        {
            collider.offset = Vector2.zero; // Reset to default
            collider.size = Vector2.one; // Default size
        }
    }


    private void Start()
    {
        
        gunData = gunManager.heldItem;
        renderer = hand.GetComponent<SpriteRenderer>();
        renderer.sprite = gunData.icon;
        if (view.IsMine)
        {
            view.RPC("SetPlayerColorForAllClients", RpcTarget.All, view.ViewID);
        }

        if(gunData.gunTipYOffset != 0.0)
        {
            AdjustGunTipPosition(gunData.gunTipYOffset, gunData);
        }
        
    }
    

    public void AdjustGunTipPosition(float yOffset, Item heldItem)
    {
        // Calculate offset
        // Assuming the gun tip is at the right edge of the sprite
        float xOffset;


        if (hand.GetComponent<SpriteRenderer>() != null)
        {
            xOffset = hand.GetComponent<SpriteRenderer>().sprite.bounds.extents.x;
            gunTip.transform.localPosition = new Vector2(xOffset, yOffset);
            MoveGunCollider(heldItem);
            
        }
        else
        {
            return;
        }
    }

    public void MoveGunCollider(Item heldItem)
    {
        collider.offset = new Vector2(heldItem.GuncolliderOffsetX, heldItem.GuncolliderOffsetY);
        collider.size = new Vector2(heldItem.GunColliderSizeX, heldItem.GunColliderSizeY);


    }

    public Vector3 GetGunTipPosition()
    {
        return gunTip.transform.position;
    }

    [PunRPC]
    public void SetPlayerColorForAllClients(int viewID)
    {
        PhotonView playerView = PhotonView.Find(viewID);
        if (playerView != null && playerView.Owner != null)
        {
            object colorChoice;
            if (playerView.Owner.CustomProperties.TryGetValue("PlayerColor", out colorChoice))
            {
                Debug.Log($"Setting color for {playerView.Owner.NickName}: {colorChoice}");
            }
            else
            {
                colorChoice = playerView.Owner.ActorNumber == 1 ? 0 : 2;
            }
            bodyObject.GetComponent<SpriteRenderer>().sprite = colors[(int)colorChoice];
        }
        else
        {
            Debug.LogError("PhotonView or Owner not found.");
        }
    }
}
