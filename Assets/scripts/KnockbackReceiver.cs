using Photon.Pun;
using UnityEngine;

public class KnockbackReceiver : MonoBehaviourPun, IPunObservable
{
    Rigidbody2D rb;
    private bool knockbackPending = false;
    private Vector2 pendingKnockbackForce;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void QueueKnockback(Vector2 force)
    {
        if (!photonView.IsMine) return;
        knockbackPending = true;
        pendingKnockbackForce = force;
    }

    void Update()
    {
        if (knockbackPending)
        {
            rb.velocity = Vector2.zero;
            rb.AddForce(pendingKnockbackForce, ForceMode2D.Impulse);
            knockbackPending = false;
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(knockbackPending);
            if (knockbackPending)
                stream.SendNext(pendingKnockbackForce);
        }
        else
        {
            bool receivedKnockback = (bool)stream.ReceiveNext();
            if (receivedKnockback)
            {
                Vector2 receivedForce = (Vector2)stream.ReceiveNext();
                if (photonView.IsMine)
                {
                    QueueKnockback(receivedForce);
                }
            }
        }
    }
}
