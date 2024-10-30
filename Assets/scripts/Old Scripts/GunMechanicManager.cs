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

public class GunMechanicManager : MonoBehaviour
{
    public GameSetup setup;
    public SpawnCrate crateSpawner;
    [SerializeField] public Item[] items;
    [SerializeField] public Item[] reusableItems;
    public Item heldItem;
    public int currentItemIndex;
    public GameObject hand;
    public Transform gunTip;

    public PhotonView view;
    public GameObject bulletPrefab;
    SpriteRenderer bulletSpriteRenderer;
    public Sprite bullet;
    public float speed = 10f;
    public bool isRight;
    public Movement movement;

    public float forceAmount = 10f;

    public bool canUseItem = true;

    public Animator gunController;
    public Animator armController;

    public float resetHandDelay = 0.75f;
    float pistolRecoilAnimationtimer;
    bool timerActive = false;

    int bulletCount;
    public int originalItemIndex;

    public bool isReloading = false;
    public AnimController animController;
    public GameObject HitMarker;

    public ParticleSystem bulletCasingParticle;

    public float timeBetweenShots = 0.1f;  // Time player has to press again to be considered spamming
    private float lastShotTime;

    public float hitmarkerDuration = 0.3f;

    bool isKnockbackActive;




    private void Awake()
    {
        DisableKatanaCollider();
        crateSpawner = GameObject.FindGameObjectWithTag("Crate Spawner").GetComponent<SpawnCrate>();
        populateItemList();
        pistolRecoilAnimationtimer = 0f;
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
            if (Input.GetKeyDown(KeyCode.T) && canUseItem && !isReloading)
            {
                if(heldItem.getType().Equals("Katana"))
                {
                    UseKatana();
                    return;
                }
                float timeSinceLastShot = Time.time - lastShotTime;
                Shoot();
                if (timeSinceLastShot <= timeBetweenShots)
                {
                    // If the key is pressed quickly enough, consider it spamming
                    armController.SetBool("IsShooting", true);
                }
                else
                {
                    // If pressed slowly, do not play the spamming shooting animation
                    armController.SetBool("IsShooting", false);
                }
                lastShotTime = Time.time;
                if (heldItem.hasBulletCasings)
                {
                    PlayerBulletCasingParticle();
                }
                StartTimer();
            }


            if (!isReloading)
            {
                CheckForReload();
            }

            if (Input.GetKey(KeyCode.T) && canUseItem && !timerActive && heldItem.isAutomatic() && !isReloading)
            {
                Shoot();
                if (heldItem.hasBulletCasings)
                {
                    PlayerBulletCasingParticle();
                }
                StartTimer();
            }

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
        }
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
            ReloadWeapon();
        }
    }

    private void populateItemList()
    {
        items = crateSpawner.items;
    }

    [PunRPC]
    public void SwapItems(int itemIndex)
    {
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

    IEnumerator weaponDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        canUseItem = true;
    }

    void Shoot()
    {
        if (isReloading)
            return;

        if (bulletCount > 0 && canUseItem)
        {


            //Make this a part of its own method
            if (heldItem.fireAnimation != null)
            {
                view.RPC("PlayFireAnim", RpcTarget.AllBuffered);
            }



            heldItem.Use(movement.facingRight, gunTip, view);
            bulletCount--;
            canUseItem = false;
            view.RPC("updateBulletCount", RpcTarget.AllBuffered, bulletCount);
            if(view.IsMine)
            {
                HandleRecoil();
            }

            if (heldItem.useDelay > 0)
            {
                StartCoroutine(weaponDelay(heldItem.useDelay));
            }

        }
    }
    public void HandleRecoil()
    {
        movement.enabled = false;
        if (movement.facingRight)
        {
            forceAmount *= -1;
            GetComponent<Rigidbody2D>().AddForce(new Vector2(-heldItem.GetRecoilKb() * 100, 0f));
        }
        else
        {
            GetComponent<Rigidbody2D>().AddForce(new Vector2(heldItem.GetRecoilKb() * 100, 0f));
        }
        StartCoroutine("toggleMovementTimer");
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
        gunController.Play(heldItem.fireAnimation);
        armController.Play(heldItem.fireAnimation);
    }

    private IEnumerator toggleMovementTimer()
    {
        yield return new WaitForSeconds(0.05f);
        movement.enabled = true;
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

    [PunRPC]
    public void HitMarkerAnimation()
    {
        // Scale up quickly, then scale back down rapidly
        LeanTween.scale(HitMarker.gameObject, new Vector2(0.639f, 0.639f), hitmarkerDuration)
                 .setEaseOutQuad()
                 .setOnComplete(() =>
                 {
                     // Scale back down to the original size
                     LeanTween.scale(HitMarker.gameObject, Vector2.zero, hitmarkerDuration)
                              .setEaseInQuad();
                 });
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
        //Play Animation
        string reloadAnim = heldItem.getReloadAnim();
        Debug.Log("Reload animation name: " + reloadAnim);
        armController.Play(reloadAnim);
        UpdateWeaponUI();
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
        
        bulletCount = heldItem.getBulletCount();
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
        animController.SetAnimatorSpeed(Input.GetAxis("Horizontal"));
    }
}
