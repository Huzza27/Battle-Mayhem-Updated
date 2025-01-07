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

    public override void Use(bool isRight, Transform gunTip, PhotonView view, Vector2 shootDirection)
    {
        view.RPC("PlayThrowSound", RpcTarget.All);
        Debug.Log("Using " + this.itemName);
        PlayThrowSound(view);
        obj = PhotonNetwork.Instantiate(throwablePrefab.name, gunTip.transform.position, Quaternion.identity, 0);
        
        if (obj.GetComponent<Bomb>() != null)
        {
            obj.GetComponent<Bomb>().dir = shootDirection;
            obj.GetComponent<Bomb>().thrower_view = view;
            obj.GetComponent<Bomb>().source = playerAudioSource;
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
