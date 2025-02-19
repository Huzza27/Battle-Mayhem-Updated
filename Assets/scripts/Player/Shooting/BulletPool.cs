using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

public class BulletPool : MonoBehaviour
{
    public List<Bullet> bulletPool = new List<Bullet>();
    public PhotonView view;
    private int bulletIndex = 0;



    public void RequestBulletFire(Vector3 spawnPosition, Vector2 fireDirection, int shooterViewID)
    {
        // Tell all clients to fire a bullet
        view.RPC("FireBulletClientSideRPC", RpcTarget.All, spawnPosition, fireDirection, shooterViewID);
    }

    [PunRPC]
    private void FireBulletClientSideRPC(Vector3 spawnPosition, Vector2 fireDirection, int shooterViewID)
    {
        // Each client handles their own bullet independently
        Bullet bullet = GetNextBullet();
        if (bullet != null)
        {
            // Set position and activate
            bullet.transform.position = spawnPosition;
            bullet.gameObject.SetActive(true);
            bullet.ResetBullet();
            bullet.SetDirection(fireDirection);
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
                Bullet result = bulletPool[bulletIndex];
                bulletIndex = (bulletIndex + 1) % bulletPool.Count;
                return result;
            }
            bulletIndex = (bulletIndex + 1) % bulletPool.Count;
        } while (bulletIndex != startIndex);

        Debug.LogWarning("No available bullets in pool!");
        return null;
    }
}