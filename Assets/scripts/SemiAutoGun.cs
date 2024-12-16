using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SemiAuto", menuName = "Gun/SemiAuto")]
public class SemiAutoGun : Item
{
    public GameObject bulletPrefab;
    GameObject obj;
    public float recoilKB;
    public float hitKb;
    public float damage;
    public float shotDelay;
    public bool automatic;
    AudioSource playerAudioSource;


    public override void Use(bool isRight, Transform gunTip, PhotonView view, Vector2 shootDirection)
    {
        PlayGunShot(view);
        // Instantiate the bullet at the gunTip position with the gun's current rotation
        obj = PhotonNetwork.Instantiate(bulletPrefab.name, gunTip.position, Quaternion.identity, 0);

        // Set the bullet's direction
        Bullet bulletScript = obj.GetComponent<Bullet>();
        bulletScript.gun = this;
        bulletScript.shooterViewID = view.ViewID;
        bulletScript.SetDirection(shootDirection); // New method to set bullet direction
    }

    private void PlayGunShot(PhotonView view)
    {
        playerAudioSource = view.gameObject.GetComponent<AudioSource>();
        playerAudioSource.PlayOneShot(FIRE_SFX);
    }

    public override float GetRecoilKb()
    {
        return recoilKB;
    }
    public override float GetDamage()
    {
        return damage;
    }

    public override float GetHitKB()
    {
        return hitKb;
    }

    public override bool isAutomatic()
    {
        return automatic;
    }
}
