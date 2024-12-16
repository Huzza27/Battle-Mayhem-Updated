using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
[CreateAssetMenu(fileName = "Shotgun", menuName = "Gun/Shotgun")]
public class ShotGun : Item

{
    public float damage;
    public ParticleSystem particles;
    public float knockbackAmount;
    public float shotRange = 2f;
    private RaycastHit2D hit;
    public float hitkb;
    PhotonView shooterView;
    AudioSource playerAudioSource;
    public override void Use(bool isRight, Transform gunTip, PhotonView view, Vector2 targetPosition)
    {

        PlayGunShot(view);
        shooterView = view;
        SpecialWeaponColliders colliders = gunTip.transform.parent.gameObject.GetComponent<SpecialWeaponColliders>();

        // Calculate the direction to the target position
        Vector2 direction = (targetPosition - (Vector2)gunTip.position).normalized;

        // Calculate the angle in degrees for the particle system rotation
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Instantiate the particle system with the correct rotation
        Quaternion rotation = Quaternion.Euler(0, 0, angle);
        GameObject obj = PhotonNetwork.Instantiate(particles.name, gunTip.position, gunTip.transform.parent.gameObject.transform.rotation);

        // Set up the ShotgunBullet properties
        ShotgunBullet bullet = obj.GetComponent<ShotgunBullet>();
        bullet.shooterView = view;
        bullet.damage = damage;
        bullet.hitkb = hitkb;
        bullet.direction = direction; // Store the normalized direction
        CheckForCollision(colliders);

    }

    private void PlayGunShot(PhotonView view)
    {
        playerAudioSource = view.gameObject.GetComponent<AudioSource>();
        playerAudioSource.PlayOneShot(FIRE_SFX);
    }

    public void CheckForCollision(SpecialWeaponColliders collider)
    {
        collider.SetColliderEnabled(collider.ShotgunCollider);
    }



    public override float GetRecoilKb()
    {
        return knockbackAmount;
    }
}
