using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
using TMPro;
using UnityEngine;
using UnityEngine.Video;
using Hashtable = ExitGames.Client.Photon.Hashtable;

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
    public TextMeshProUGUI username;

    // Start is called before the first frame update
    private void Awake()
    {
        ResetGameSetupState();
    }

    public void ResetGameSetupState()
    {
        // Reset gun data
        gunData = gunManager.heldItem;
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


        // Ensure collider is reinitialized
        if (collider != null)
        {
            collider.offset = Vector2.zero; // Reset to default
            collider.size = Vector2.one; // Default size
        }
    }

    private void OnEnable()
    {
        GameManager.OnGameReset += ResetGameSetupState;
    }

    private void OnDisable()
    {
        GameManager.OnGameReset -= ResetGameSetupState;
    }

    private void Start()
    {
        gunData = gunManager.heldItem;
        renderer = hand.GetComponent<SpriteRenderer>();
        renderer.sprite = gunData.icon;

        if (view.IsMine)
        {
            //view.RPC("SetPlayerColorForAllClients", RpcTarget.All, view.ViewID); For old color system

            // Set username for this player
            SyncPlayerUsername();
        }

        if (gunData.gunTipYOffset != 0.0)
        {
            AdjustGunTipPosition(gunData.gunTipYOffset, gunData);
        }
    }

    /// <summary>
    /// Syncs the player's username across the network
    /// </summary>
    public void SyncPlayerUsername()
    {
        if (view.IsMine)
        {
            // Get the player's username from SteamManager
            string playerName = SteamManager.GetSteamUserName();

            // Store it as a custom property for persistence across reconnects
            Hashtable props = new Hashtable();
            props.Add("PlayerName", playerName);
            PhotonNetwork.LocalPlayer.SetCustomProperties(props);

            // Send RPC to update username display on all clients
            view.RPC("UpdatePlayerUsername", RpcTarget.AllBuffered, view.ViewID, playerName);
        }
    }

    [PunRPC]
    public void UpdatePlayerUsername(int viewID, string playerName)
    {
        // Find the PhotonView for this player
        PhotonView playerView = PhotonView.Find(viewID);

        if (playerView != null && playerView.gameObject == gameObject)
        {
            // Update the TextMeshProUGUI component with the player's name
            if (username != null)
            {
                username.text = playerName;

                // Optional: You can add styling or formatting here
                // For example, if it's the local player, you might want to highlight it
                if (playerView.IsMine)
                {
                    username.color = Color.yellow; // Highlight local player name
                }
                else
                {
                    username.color = Color.white; // Normal color for other players
                }
            }
            else
            {
                Debug.LogError("Username TextMeshProUGUI component is not assigned!");
            }
        }
    }

    public void AdjustGunTipPosition(float yOffset, Item heldItem)
    {
        // Calculate offset
        // Assuming the gun tip is at the right edge of the sprite
        float xOffset;
        if (heldItem.getType() == "Throwable")
        {
            return;
        }
        if (hand.GetComponent<SpriteRenderer>() != null)
        {
            xOffset = hand.GetComponent<SpriteRenderer>().sprite.bounds.extents.x;
            gunTip.transform.localPosition = new Vector2(xOffset, yOffset);
            MoveGunCollider(heldItem);

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

    // Handle reconnect scenario - update username when player properties are updated
    public void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (view.Owner != null && view.Owner.ActorNumber == targetPlayer.ActorNumber)
        {
            if (changedProps.ContainsKey("PlayerName") && username != null)
            {
                username.text = (string)changedProps["PlayerName"];
            }
        }
    }
}