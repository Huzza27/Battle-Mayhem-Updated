using ExitGames.Client.Photon;
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


    public override void Use(bool isRight, Transform gunTip, PhotonView view, Vector2 shootDirection, BulletPool pool)
    {
        view.RPC("PlayWeaponSounds", RpcTarget.All);
        pool.RequestBulletFire(gunTip.position, shootDirection, view.ViewID);

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
