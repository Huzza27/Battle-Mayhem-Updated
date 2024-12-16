using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Unity.VisualScripting;
using Photon.Pun.UtilityScripts;
using System.ComponentModel;
using Smooth;
using JetBrains.Annotations;
using UnityEngine.UIElements;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class GunMechanicManager : MonoBehaviourPunCallbacks
{
    [Header("FX")]
    public GameObject gunSwapPrefab;

    [Header("Setup & References")]
    public GameSetup setup;
    public SpawnCrate crateSpawner;
    public PhotonView view;
    public Movement movement;
    public AnimController animController;
    public AudioSource playerAudio;

    [Header("Items & Inventory")]
    [SerializeField] public Item[] items;
    [SerializeField] public Item[] reusableItems;
    public Item heldItem;
    public int currentItemIndex;
    public int originalItemIndex;

    [Header("Gun & Shooting")]
    public GameObject hand;
    public Transform gunTip;
    public Transform gunParent;  // Center point for gun rotation
    public GameObject bulletPrefab;
    public ParticleSystem bulletCasingParticle;
    public Sprite bullet;
    public float speed = 10f;
    public float timeBetweenShots = 0.1f;  // Time between shots to prevent spamming
    private float lastShotTime;

    [Header("Force & Recoil")]
    public float forceAmount = 10f;
    public float resetHandDelay = 0.75f;
    float pistolRecoilAnimationtimer;
    bool timerActive = false;

    [Header("Bullet Count & Reload")]
    public int bulletCount;
    public bool canUseItem = true;
    public bool isReloading = false;

    [Header("Hitmarker")]
    public GameObject HitMarker;
    public float hitmarkerDuration = 0.3f;

    [Header("Animation Controllers")]
    public Animator gunController;
    public Animator armController;

    [Header("Knockback")]
    bool isKnockbackActive;

    [Header("Miscellaneous")]
    public bool isRight;
    public bool gameOver = false;

    [Header("Internal")]
    private Vector2 shootDirection;


    SpriteRenderer bulletSpriteRenderer;

    private void Awake()
    {
        ResetGunMechanicManagerState();
        DisableKatanaCollider();
        crateSpawner = GameObject.FindGameObjectWithTag("Crate Spawner").GetComponent<SpawnCrate>();
        populateItemList();
        pistolRecoilAnimationtimer = 0f;
    }

    public void ResetGunMechanicManagerState()
    {
        // Reset held item to the original item
        heldItem = reusableItems[originalItemIndex];
        currentItemIndex = originalItemIndex;

        // Reset hand sprite
        if (hand != null && heldItem.icon != null)
        {
            hand.GetComponent<SpriteRenderer>().sprite = heldItem.icon;
        }

        // Reset bullet count and update across network
        bulletCount = heldItem.getBulletCount();
        view.RPC("updateBulletCount", RpcTarget.AllBuffered, bulletCount);

        // Reset gun tip position and collider
        if (setup != null)
        {
            setup.AdjustGunTipPosition(heldItem.gunTipYOffset, heldItem);
        }

        // Reset shooting-related variables
        canUseItem = true;
        isReloading = false;
        timerActive = false;
        pistolRecoilAnimationtimer = 0f;

        // Reset particle systems
        if (bulletCasingParticle != null)
        {
            bulletCasingParticle.Clear();
            bulletCasingParticle.Stop();
        }

        // Reset arm and gun animations
        if (armController != null)
        {
            armController.Rebind();
            armController.Update(0);
            armController.SetBool("IsShooting", false);
        }
        if (gunController != null)
        {
            gunController.Rebind();
            gunController.Update(0);
        }

        // Reset knockback state
        isKnockbackActive = false;

        // Reset gun parent rotation and shoot direction
        if (gunParent != null)
        {
            gunParent.localRotation = Quaternion.identity;
            shootDirection = Vector2.right; // Default shoot direction
        }

        // Reset movement state
        if (movement != null)
        {
            movement.enabled = true;
        }

        // Reset game-over state
        gameOver = false;

        // Reset item list using crate spawner
        if (crateSpawner != null)
        {
            populateItemList();
        }

        // Disable Katana collider
        DisableKatanaCollider();

        // Sync weapon UI for the local client
        if (view.IsMine)
        {
            UpdateWeaponUI();
        }
    }


    private void Start()
    {
        bulletCount = heldItem.getBulletCount();
        view.RPC("updateBulletCount", RpcTarget.AllBuffered, bulletCount);
        bulletSpriteRenderer = bulletPrefab.GetComponent<SpriteRenderer>();
        isRight = movement.facingRight;
    }

    void Update()
    {
        if (view.IsMine)
        {
            if (ESCMenuListener.isPaused || IsLoading() || GameManager.Instance.gameOver)
            {
                return;
            }

            // Handle single-shot weapons
            if (Input.GetMouseButtonDown(0) && canUseItem && !isReloading)
            {
                HandleFireInput();
            }

            // Handle automatic weapons
            if (Input.GetMouseButton(0) && canUseItem && heldItem.isAutomatic() && !isReloading)
            {
                HandleFireInput();
            }

            // Manage recoil animation timing
            if (timerActive)
            {
                pistolRecoilAnimationtimer -= Time.deltaTime;
                if (pistolRecoilAnimationtimer <= 0f)
                {
                    StopShootingAnimation();
                }
            }

            if (!isReloading)
            {
                CheckForReload();
            }

            HandleAiming();
        }
    }

    private void HandleFireInput()
    {
        float currentTime = Time.time;

        // Ensure the player cannot shoot before the weapon's fire rate allows
        if (currentTime - lastShotTime < heldItem.useDelay)
        {
            return;
        }

        Shoot();
        lastShotTime = currentTime;

        // Play bullet casing particle if applicable
        if (heldItem.hasBulletCasings)
        {
            PlayerBulletCasingParticle();
        }

        StartTimer(); // Start the recoil animation timer
    }



    public bool IsLoading()
    {
        // Check if the room has the "IsLoading" property
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("IsLoading", out object isLoadingValue))
        {
            // Return the value cast as a boolean
            return (bool)isLoadingValue;
        }

        // Default to false if the property is not set
        return false;
    }

    private void HandleAiming()
    {
        // Get the mouse position in world coordinates
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // Check if the mouse is to the left or right of the player
        bool mouseIsToTheRight = mousePosition.x > transform.position.x;

        // Flip the player if the mouse crosses over the player horizontally
        if ((movement.facingRight && !mouseIsToTheRight) || (!movement.facingRight && mouseIsToTheRight))
        {
            // Flip the player on all clients
            view.RPC("FlipCharacterBasedOnDirection", RpcTarget.All, mousePosition.x - transform.position.x);
        }

        // Calculate the direction from the gun pivot to the mouse
        Vector2 aimDirection = mousePosition - (Vector2)gunParent.position;

        // Calculate the angle in degrees based on the aim direction
        float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;

        // Adjust the angle based on facing direction
        if (!movement.facingRight)
        {
            angle = 180 - angle; // Invert angle for left-facing
        }

        // Apply the rotation to the gun pivot based on the adjusted angle
        gunParent.localRotation = Quaternion.Euler(0, 0, -angle);
        shootDirection = gunParent.transform.right;
    }








    [PunRPC]
    public void PlayerBulletCasingParticle()
    {
        bulletCasingParticle.GetComponent<ParticleSystemRenderer>().material = heldItem.GetBulletCasingMaterial();
        bulletCasingParticle.Emit(1);
    }

    [PunRPC]
    public void EquipMainWeapon(int viewID)
    {
        PhotonView targetView = PhotonView.Find(viewID);
        if (targetView != null && targetView.Owner != null)
        {
            // Use targetView.Owner to get the correct player's properties
            object mainWeaponIndex;

            // Check the custom properties of the owner of the PhotonView
            if (!targetView.Owner.CustomProperties.TryGetValue("PlayerMainGunChoice", out mainWeaponIndex))
            {
                // Default to the first item if no weapon choice is found
                this.gameObject.GetComponent<GunMechanicManager>().heldItem = reusableItems[0];
                originalItemIndex = 0;
            }
            else
            {
                // Use the weapon index from the owner's custom properties
                int weaponIndex = (int)mainWeaponIndex;
                this.gameObject.GetComponent<GunMechanicManager>().heldItem = reusableItems[weaponIndex];
                originalItemIndex  = weaponIndex;
            }
        }
        else
        {
            Debug.LogError("PhotonView not found or Owner is null.");
        }
    }

private void CheckForReload()
    {
        if(bulletCount == 0)
        {
            if (heldItem.isReusable())
            {
                ReloadWeapon();
            }
            else
            {
                view.RPC("SwapItemsToOriginal", RpcTarget.All);
            }
        }

    }

    private void populateItemList()
    {
        items = crateSpawner.items;
    }

    [PunRPC]
    public void SwapItems(int itemIndex)
    {
        StopShootingAnimation();
        SwapItemFX();   
        Item newItem = items[itemIndex];
        heldItem = newItem;

        // if the new item does not have a animation, we just run the idle animation
        //Otherwise, we just make the sprite null for the sprite renderer

        if (heldItem.icon != null) 
        {
            hand.GetComponent<SpriteRenderer>().sprite = heldItem.icon;
            setup.AdjustGunTipPosition(heldItem.gunTipYOffset, heldItem);
            animController.ReturnToArmIdle();
        }
        else
        {
            hand.GetComponent<SpriteRenderer>().sprite = null;
        }
        bulletCount = heldItem.bulletCount;
        view.RPC("updateBulletCount", RpcTarget.AllBuffered, bulletCount);
        canUseItem = true;

    }

    public void SwapItemFX()
    {
        GameObject swappedWeapon = PhotonNetwork.Instantiate(gunSwapPrefab.name, transform.position, Quaternion.identity);
        swappedWeapon.transform.Rotate(0f, 180f, 0f);
        swappedWeapon.GetComponent<SpriteRenderer>().sprite = heldItem.icon;
    }

    IEnumerator weaponDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        canUseItem = true;
    }

    void Shoot()
    {
        if (isReloading || !canUseItem || bulletCount <= 0)
            return;

        canUseItem = false;

        // Handle the firing direction
        Vector2 shootDirection = gunParent.right;

        // Play fire animation
        if (!string.IsNullOrEmpty(heldItem.fireAnimation))
        {
            view.RPC("PlayFireAnim", RpcTarget.AllBuffered);
        }

        // Use the item
        heldItem.Use(movement.facingRight, gunTip, view, shootDirection);

        bulletCount--;
        view.RPC("updateBulletCount", RpcTarget.AllBuffered, bulletCount);

        // Re-enable shooting after the weapon's use delay
        StartCoroutine(weaponDelay(heldItem.useDelay));
    }




    void StartTimer()
    {

        pistolRecoilAnimationtimer = 0.3f;
        timerActive = true;
    }

    void StopShootingAnimation()
    {
        timerActive = false;
        armController.SetBool("IsShooting", false);
        armController.SetTrigger("ReturnToGun");
    }

    [PunRPC]
    void PlayFireAnim()
    {
        gunController.speed = heldItem.animatorSpeed;
        gunController.Play(heldItem.fireAnimation);
        armController.Play(heldItem.fireAnimation);
    }


    [PunRPC]
    public void TakeKnockBackFromBomb(Vector2 dir, float kb)
    {

        //dir.x = -dir.x;
        movement.enabled=false;
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.AddForce(dir * kb, ForceMode2D.Impulse);
        //StartCoroutine(toggleMovementTimer());
        
    }



    public void UseKatana()
    {
        if (movement.isGrounded || movement.canDoubleJump)
        {
            view.RPC("PlayFireAnim", RpcTarget.All);
            movement.Jump();
            // Disable further jumping if katana used in the air
            if (!movement.isGrounded)
            {
                movement.canDoubleJump = false;
            }
            canUseItem = false;
            ActivateKatanaCollider();
        }
    }

    [PunRPC]
    public void ExplosiveKnockback(Vector2 direciton, float force)
    {
        movement.enabled = false;
        direciton.x = -direciton.x;
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.AddForce(direciton * force, ForceMode2D.Impulse);
        //StartCoroutine(toggleMovementTimer());
    }



    public void ActivateKatanaCollider()
    {
        hand.GetComponent<BoxCollider2D>().enabled = true;
        Invoke("DisableKatanaCollider", 0.5f);
    }

    public void DisableKatanaCollider()
    {
        hand.GetComponent<BoxCollider2D>().enabled = false;
    }


    private void ReloadWeapon()
    {
        playerAudio.PlayOneShot(heldItem.RELOAD_SFX);
        // Call an RPC to handle reload across all clients
        view.RPC("NetworkedReload", RpcTarget.All);
    }

    [PunRPC]
    private void NetworkedReload()
    {
        // Disable gun sprite
        view.RPC("DisableGunSpriteForReload", RpcTarget.All, false);

        // Get reload animation
        string reloadAnim = heldItem.getReloadAnim();
        Debug.Log("Reload animation name: " + reloadAnim);

        // Play animation
        armController.Play(reloadAnim);

        // Only update UI on the local client
        if (view.IsMine)
        {
            UpdateWeaponUI();
        }
    }


    [PunRPC]
    private void DisableGunSpriteForReload(bool enable)
    {
        hand.GetComponent<SpriteRenderer>().enabled = enable;
    }

    private void UpdateWeaponUI()
    {
        //update ui
        bulletCount = heldItem.bulletCount;
        view.RPC("updateBulletCount", RpcTarget.AllBuffered, bulletCount);

        //Swap back to original gun
        if(!heldItem.reusable)
        SwapItemsToOriginal();
    }

    [PunRPC]
    public void SwapItemsToOriginal()
    {
        StopShootingAnimation();
        SwapItemFX();
        Item newItem = reusableItems[originalItemIndex];
        
        if (newItem.icon != null)
        {
            hand.GetComponent<SpriteRenderer>().sprite = newItem.icon;
            setup.AdjustGunTipPosition(heldItem.gunTipYOffset, newItem);
            animController.ReturnToArmIdle();
        }
        else
        {
           hand.GetComponent<SpriteRenderer>().sprite = null;
        }
        
        bulletCount = newItem.getBulletCount();
        view.RPC("updateBulletCount", RpcTarget.AllBuffered, bulletCount);
        heldItem = newItem;
        canUseItem = true;
    }

    public void OnReloadEnter()
    {
        canUseItem = false;
        isReloading = true;
    }

    public void OnReloadExit()
    {
        canUseItem = true;
        isReloading = false;
        view.RPC("DisableGunSpriteForReload", RpcTarget.All, true);
        animController.SetAnimatorSpeed(Input.GetAxis("Horizontal"));
    }
}
