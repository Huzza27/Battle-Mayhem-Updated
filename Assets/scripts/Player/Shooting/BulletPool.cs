using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletPool : MonoBehaviour
{
    public List<Bullet> bulletPool = new List<Bullet>();
    Bullet currentBullet;
    public PhotonView view;

    private void Awake()
    {
        foreach (Bullet bullet in bulletPool) 
        { 
            bullet.gameObject.SetActive(true);
            bullet.gameObject.SetActive(false);
        }
    }

    [PunRPC]
    public void FireNextBullet(Transform spawnPoint, Vector2 fireDirection, PhotonView shooterView)
    {
        if (PhotonNetwork.IsMasterClient) //Check for master client
        {
            currentBullet = GetNextBullet(); //Get the reference to the next bullet
            MoveBulletToFireLocation(currentBullet, spawnPoint); //Move the current bullet to the players hand/spawnpoint
            EnableBulletAndSetUpDependencies(fireDirection, shooterView); //Enable the bullet, and make it move the right way
            RemoveBulletFromList(currentBullet);
        }
    }


    void EnableBulletAndSetUpDependencies(Vector2 fireDir, PhotonView shooterView)
    {
        view.RPC("EnableBulletNetwork", RpcTarget.All, currentBullet.view.ViewID);
        currentBullet.SetDirection(fireDir); //Set the direction & rotation of the bullet
        currentBullet.shooterViewID = shooterView.ViewID;
        currentBullet.pool = this;

    }

    [PunRPC]
    void EnableBulletNetwork(int ViewID)
    {
        GameObject bullet = PhotonView.Find(ViewID).gameObject;
        bullet.gameObject.SetActive(true); //Enable bullet
        currentBullet = bullet.GetComponent<Bullet>();

    }
    public Bullet GetNextBullet()
    {
        return bulletPool[0];
    }

    public void MoveBulletToFireLocation(Bullet bullet, Transform spawnPoint)
    {
        bullet.gameObject.transform.position = spawnPoint.position;
    }

    public void RemoveBulletFromList(Bullet bullet)
    {
        bulletPool.Remove(bullet); //Remove bullet from pool
    }

    public void AddBulletToEndOfLine(Bullet bullet)
    {
        bulletPool.Add(bullet);
    }
}
