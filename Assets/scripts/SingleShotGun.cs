using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

[CreateAssetMenu(fileName = "SingleShot", menuName = "Gun/SingleShot")]
public class SingleShotGun : Item
{
    public GameObject bulletPrefab;
    GameObject obj;
    public float recoilKB;
    public float hitKb;
    public float damage;
    public AudioSource playerAudioSource;

    public override void Use(bool isRight, Transform gunTip, PhotonView view, Vector2 shootDirection)
    {
        view.RPC("PlayWeaponSounds", RpcTarget.All);
        // Instantiate the bullet at the gunTip position with the gun's current rotation
        obj = PhotonNetwork.Instantiate(bulletPrefab.name, gunTip.position, Quaternion.identity, 0);

        // Set the bullet's direction
        Bullet bulletScript = obj.GetComponent<Bullet>();
        bulletScript.gun = view.gameObject.GetComponent<GunMechanicManager>().heldItem;
        bulletScript.shooterViewID = view.ViewID;
        bulletScript.SetDirection(shootDirection); // New method to set bullet direction
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
}
