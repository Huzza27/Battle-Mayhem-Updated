using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Throwable", menuName = "Throwable")]
public class Throwable : Item
{
    public GameObject throwablePrefab;
    GameObject obj;
    public float hitKb;
    public float damage;
    public float throwDelay;
    public bool customAnim = true;
    public int throwableAmount;
    AudioSource playerAudioSource;
    public AudioClip THROW_FX;

    public override void Use(bool isRight, Transform gunTip, PhotonView view, Vector2 shootDirection, BulletPool pool)
    {
        view.RPC("PlayThrowSound", RpcTarget.All);
        Debug.Log("Using " + this.itemName);
        PlayThrowSound(view);
        Debug.Log("Spawning Knife");
        obj = PhotonNetwork.Instantiate(throwablePrefab.name, gunTip.transform.position, Quaternion.identity, 0);
        
        if (obj.GetComponent<Bomb>() != null)
        {
            obj.GetComponent<Bomb>().dir = shootDirection;
            obj.GetComponent<Bomb>().thrower_view = view;
            obj.GetComponent<Bomb>().source = playerAudioSource;
        }
        else if(obj.GetComponent<ThrowingKnife>() != null)
        {
            obj.GetComponent<ThrowingKnife>().dir = shootDirection;
            obj.GetComponent<ThrowingKnife>().thrower_view = view;
            //obj.GetComponent<ThrowingKnife>().source = playerAudioSource;
        }
    }

    private void PlayThrowSound(PhotonView view)
    {
        playerAudioSource = view.gameObject.GetComponent<AudioSource>();
        playerAudioSource.PlayOneShot(THROW_FX);
    }

    public override float GetDamage()
    {
        return damage;
    }

    public override float GetHitKB()
    {
        return hitKb;
    }

    public override bool CustomAnim()
    {
        return customAnim;
    }

    public override int getBulletCount()
    {
        return throwableAmount;
    }
}
