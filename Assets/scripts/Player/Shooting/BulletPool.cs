using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

public class BulletPool : MonoBehaviour
{
    public List<Bullet> bulletPool = new List<Bullet>();
    private Bullet currentBullet;
    public PhotonView view;
    private int bulletIndex = 0;
    public GameObject dummyBullet;

    private void Awake()
    {
        foreach (Bullet bullet in bulletPool)
        {
            bullet.pool = this;
            bullet.gameObject.SetActive(true);
            bullet.gameObject.SetActive(false);
        }
    }

    public void RequestBulletFire(Vector3 spawnPosition, Vector2 fireDirection, int shooterViewID)
    {
        // If we're the shooter but not the master, spawn dummy bullet
        if (!PhotonNetwork.IsMasterClient &&
            PhotonView.Find(shooterViewID).IsMine)
        {
            FireDummyBullet(fireDirection, spawnPosition);
        }

        // Always send fire request to master client
        view.RPC("FireBulletRPC", RpcTarget.MasterClient, spawnPosition, fireDirection, shooterViewID);
    }

    [PunRPC]
    private void FireBulletRPC(Vector3 spawnPosition, Vector2 fireDirection, int shooterViewID)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        currentBullet = GetNextBullet();
        if (currentBullet != null)
        {
            MoveBulletToFireLocation(currentBullet, spawnPosition);
            view.RPC("EnableBulletNetwork", RpcTarget.All,
                currentBullet.view.ViewID,
                fireDirection,
                shooterViewID);
            bulletIndex = (bulletIndex + 1) % bulletPool.Count;
        }
    }

    void FireDummyBullet(Vector2 direction, Vector3 spawn)
    {
        GameObject bullet = Instantiate(dummyBullet, spawn, Quaternion.identity);
        if (bullet != null)
        {
            DummyBullet dBullet = bullet.GetComponent<DummyBullet>();
            dBullet.SetDirection(direction);
            // Ensure sprite and other properties are properly set
            dBullet.Initialize();
        }
    }

    [PunRPC]
    void EnableBulletNetwork(int bulletViewID, Vector2 fireDir, int shooterViewID)
    {
        PhotonView bulletView = PhotonView.Find(bulletViewID);
        if (bulletView != null)
        {
            Bullet bullet = bulletView.GetComponent<Bullet>();

            // Show networked bullet to everyone except the shooter
            if ((PhotonView.Find(shooterViewID).OwnerActorNr != PhotonNetwork.LocalPlayer.ActorNumber) || PhotonNetwork.IsMasterClient)
            {
                bullet.transform.position -= (Vector3)(fireDir * 0.1f);
                bullet.gameObject.SetActive(true);
                bullet.ResetBullet();
                bullet.SetDirection(fireDir);
                bullet.shooterViewID = shooterViewID;
            }
        }
    }

    private Bullet GetNextBullet()
    {
        int startIndex = bulletIndex;
        do
        {
            if (!bulletPool[bulletIndex].gameObject.activeSelf)
            {
                return bulletPool[bulletIndex];
            }
            bulletIndex = (bulletIndex + 1) % bulletPool.Count;
        } while (bulletIndex != startIndex);
        Debug.LogWarning("No available bullets in pool!");
        return null;
    }

    public void MoveBulletToFireLocation(Bullet bullet, Vector3 spawnPosition)
    {
        bullet.transform.position = spawnPosition;
    }
}