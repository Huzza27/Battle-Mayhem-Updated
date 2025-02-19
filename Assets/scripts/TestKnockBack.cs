using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestKnockBack : MonoBehaviour
{
    public PhotonView view;




    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            if(view.IsMine)
            {
                view.RPC("Knockback", RpcTarget.All);
            }
        }
    }

    [PunRPC]
    public void Knockback()
    {
        GetComponent<Rigidbody2D>().AddForce(new Vector2(500f, 0f), ForceMode2D.Impulse);
    }
}
