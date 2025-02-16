using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

public class BulletPool : MonoBehaviour
{
    public List<Bullet> bulletPool = new List<Bullet>();
    private Bullet currentBullet;
    public PhotonView view;
    private int bulletIndex = 0;

    private void Awake()
    {
        // Initialize all bullets in pool, but only on master client
        if (PhotonNetwork.IsMasterClient)
        {
            foreach (Bullet bullet in bulletPool)
            {
                bullet.pool = this;
                bullet.gameObject.SetActive(true);
                bullet.gameObject.SetActive(false);
            }
        }
    }

    // Called by the gun when requesting to fire
    public void RequestBulletFire(Vector3 spawnPosition, Vector2 fireDirection, int shooterViewID)
    {
        // Send fire request to master client
        view.RPC("FireBulletRPC", RpcTarget.MasterClient, spawnPosition, fireDirection, shooterViewID);
    }

    [PunRPC]
    private void FireBulletRPC(Vector3 spawnPosition, Vector2 fireDirection, int shooterViewID)
    {
        // Only master client handles bullet management
        if (!PhotonNetwork.IsMasterClient) return;

        currentBullet = GetNextBullet();
        if (currentBullet != null)
        {
            MoveBulletToFireLocation(currentBullet, spawnPosition);
            // Tell all clients about this bullet
            view.RPC("EnableBulletNetwork", RpcTarget.All,
                currentBullet.view.ViewID,
                fireDirection,
                shooterViewID);
            bulletIndex = (bulletIndex + 1) % bulletPool.Count;
        }
    }

    [PunRPC]
    void EnableBulletNetwork(int bulletViewID, Vector2 fireDir, int shooterViewID)
    {
        PhotonView bulletView = PhotonView.Find(bulletViewID);
        if (bulletView != null)
        {
            Bullet bullet = bulletView.GetComponent<Bullet>();

            // For non-master clients, slightly backtrack the bullet position
            // to create smoother visual transition
            if (!PhotonNetwork.IsMasterClient)
            {
                bullet.transform.position -= (Vector3)(fireDir * 0.1f);
            }

            bullet.gameObject.SetActive(true);
            bullet.ResetBullet();
            bullet.SetDirection(fireDir);
            bullet.shooterViewID = shooterViewID;
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