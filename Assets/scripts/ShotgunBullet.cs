using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Pun.Demo.Asteroids;

public class ShotgunBullet : MonoBehaviour
{
    public PhotonView view;


    public bool MoveDir = false;

    public float moveSpeed;

    public float destroyTime;

    public SpriteRenderer spriteRenderer;

    public PhotonView shooterView;
    public float damage;
    public float hitkb;
    public Vector2 direction;

    private void Awake()
    {
        StartCoroutine("DestroyTimer");
        StartCoroutine("changeSprite");
    }

    IEnumerator DestroyTimer()
    {
        yield return new WaitForSeconds(destroyTime);
        GetComponent<PhotonView>().RPC("DestroyObject", RpcTarget.AllBuffered);
    }

    [PunRPC]
    public void changeDir_Left()
    {
        //Debug.Log("Changing Direction");
        transform.Rotate(0f, 180f, 0f);
        spriteRenderer.flipX = true;
        MoveDir = true;
    }

    [PunRPC]
    public void DestroyObject()
    {
        PhotonNetwork.Destroy(this.gameObject);
    }

    IEnumerator changeSprite()
    {
        yield return new WaitForSeconds(0.05f);
        spriteRenderer.sprite = null;
    }

    public void SetDirection(Vector2 shootDirection)
    {
        // Normalize and store the direction
        direction = shootDirection.normalized;

        // Calculate the angle in degrees for the particle system rotation
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Apply the rotation to the particle system to match its direction
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

}
