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

    private void Start()
    {
        gunData = gunManager.heldItem;
        renderer = hand.GetComponent<SpriteRenderer>();
        renderer.sprite = gunData.icon;
        if (view.IsMine)
        {
            view.RPC("SetPlayerColorForAllClients", RpcTarget.All);
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
        collider = view.gameObject.transform.GetChild(7).GetComponent<BoxCollider2D>();
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
                if(PhotonNetwork.IsMasterClient)
                {
                    colorChoice = 0;
                }

                else
                {
                    colorChoice = 2;
                }
                Debug.LogError("Failed to find 'PlayerColor' in custom properties.");
            }
            bodyObject.GetComponent<SpriteRenderer>().sprite = colors[(int)colorChoice];
        }
        else
        {
            Debug.LogError("PhotonView or Owner not found.");
        }
    }

}
