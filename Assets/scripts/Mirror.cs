using Photon.Pun;
using UnityEngine;

public class Mirror : MonoBehaviour
{
    public BoxCollider2D collider;
    private PhotonView view;
    public PhotonView playerView;
    public GunMechanicManager gunManager;
    bool hasMirror = false;

    private void Start()
    {
        view = this.GetComponent<PhotonView>();
    }

    private void Update()
    {
        // Check for mirror state changes
        bool shouldHaveMirror = (gunManager.heldItem.name == "Mirror");

        // Only update if the state has changed
        if (hasMirror != shouldHaveMirror)
        {
            hasMirror = shouldHaveMirror;
            collider.enabled = hasMirror;
        }
    }

    public void OnHitMirror()
    {
        // Only the owner of the player/mirror should decrease durability
        if (playerView.IsMine)
        {
            LowerDurability();
        }
    }

    // Make this a local method instead of RPC
    public void LowerDurability()
    {
        // Directly modify the bulletCount in gunManager
        gunManager.bulletCount--;

        // Update the bullet count across the network
        playerView.RPC("updateBulletCount", RpcTarget.AllBuffered, gunManager.bulletCount);

        // Only swap to original weapon when durability reaches zero
        if (gunManager.bulletCount <= 0)
        {
            playerView.RPC("SwapItemsToOriginal", RpcTarget.All);
        }
    }
}