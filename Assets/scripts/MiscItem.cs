using ExitGames.Client.Photon;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Items/Misc", menuName = "MiscItem")]
public class MiscItem : Item
{
    public GameObject itemManager;



    public override void Use(bool isRight, Transform gunTip, PhotonView view, Vector2 shootDirection, BulletPool pool)
    {
        GameObject item = PhotonNetwork.Instantiate(itemManager.name, view.gameObject.transform.position, Quaternion.identity);
        item.GetComponent<MiscItemDependencies>().shooterView = view;
    }
}
