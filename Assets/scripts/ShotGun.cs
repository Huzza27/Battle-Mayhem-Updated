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
    public override void Use(bool isRight, Transform gunTip, PhotonView view)
    {
        shooterView = view;

        //Debug.Log("Using " + this.itemName);
        if (isRight)
        {
            GameObject obj = PhotonNetwork.Instantiate(particles.name, gunTip.position, Quaternion.identity);
            obj.GetComponent<ShotgunBullet>().shooterView = view;
            obj.GetComponent<ShotgunBullet>().damage = damage;
            obj.GetComponent<ShotgunBullet>().hitkb = hitkb;
        }

        else
        {
            hitkb = -hitkb;
            GameObject obj = PhotonNetwork.Instantiate(particles.name, gunTip.position, Quaternion.identity);
            obj.GetComponent<ShotgunBullet>().shooterView = view;
            obj.GetComponent<ShotgunBullet>().damage = damage;
            obj.GetComponent<ShotgunBullet>().hitkb = hitkb;
            obj.GetComponent<PhotonView>().RPC("changeDir_Left", RpcTarget.AllBuffered);
            
        }
        
    }

    public override float GetRecoilKb()
    {
        return knockbackAmount;
    }
}
