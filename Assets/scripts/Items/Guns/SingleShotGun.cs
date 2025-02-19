using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

[CreateAssetMenu(fileName = "SingleShot", menuName = "Gun/SingleShot")]
public class SingleShotGun : Item
{
    public GameObject bulletPrefab;
    public float recoilKB;
    public float hitKb;
    public float damage;
    public AudioSource playerAudioSource;

    
    public override void Use(bool isRight, Transform gunTip, PhotonView view, Vector2 shootDirection, BulletPool pool)
    {
        pool.RequestBulletFire(gunTip.position, shootDirection, view.ViewID);
    }
    public override float GetDamage()
    {
        return damage;
    }
}
