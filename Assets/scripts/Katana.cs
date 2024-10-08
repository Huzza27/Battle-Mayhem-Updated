using Photon.Pun;
using UnityEngine;

[CreateAssetMenu(fileName = "Katana", menuName = "Other/Katana")]
public class Katana : Item

{
    public float knockbackForce;
    private Movement playerMovement;
    public float verticalBoost;
    public override void Use(bool isRight, Transform gunTip, PhotonView view)
    {

        GameObject player = view.gameObject;
        player.GetComponent<GunMechanicManager>().UseKatana();
    }

    public override float GetRecoilKb()
    {
        return 0f;
    }

    public override float GetHitKB()
    {
        return knockbackForce;
    }

    public override float GetVerticalBoost()
    {
        return verticalBoost;
    }
}
