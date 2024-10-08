using System;
using System.Collections;
using UnityEngine;
using Photon.Pun;

public class AnimController : MonoBehaviour
{
    public GunMechanicManager manager;
    public Animator bodyController, legController, armController, eyesController;

    private PhotonView view;
    private float direction;
    private string currentState;

    private const string DEFAULT_RUN = "run_arms";
    private const string DEFAULT_IDLE = "idel_arms";

    private void Start()
    {
        view = GetComponent<PhotonView>();
        manager = GetComponent<GunMechanicManager>();
    }

    private void Update()
    {
        if (view.IsMine)
        {
            // Process player input for direction
            direction = Input.GetAxis("Horizontal");

            // Manage animation based on direction
            view.RPC("ManageAnimationBasedOnDirection", RpcTarget.AllBuffered, direction);
        }
    }

    [PunRPC]
    public void ManageAnimationBasedOnDirection(float direction)
    {
        if (Mathf.Abs(direction) > 0.1)
        {
            SetAnimation(manager.heldItem.getRun(), DEFAULT_RUN);
        }
        else
        {
            SetAnimation(manager.heldItem.getIdle(), DEFAULT_IDLE);
        }

        SetAnimatorSpeed(Math.Abs(direction));
    }


    public void SetAnimation(string newAnimation, string defaultAnimation)
    {
        if (manager.isReloading || newAnimation == currentState)  // Prevent changing animation during reload
        {
            return;
        }

        currentState = string.IsNullOrEmpty(newAnimation) ? defaultAnimation : newAnimation;
        armController.Play(currentState);
    }

    public void ReturnToArmIdle()
    {
        armController.Play(DEFAULT_IDLE);
    }

    public void SetAnimatorGroundFlag(bool flag)
    {
        if (!manager.isReloading)
        {
            armController.SetBool("IsGrounded", flag);
        }
        legController.SetBool("IsGrounded", flag);
        bodyController.SetBool("IsGrounded", flag);
        eyesController.SetBool("IsGrounded", flag);
    }

    public void SetAnimatorSpeed(float speed)
    {
        if (!manager.isReloading)
        {
            armController.SetFloat("Speed", speed);
        }
        legController.SetFloat("Speed", speed);
        bodyController.SetFloat("Speed", speed);
        eyesController.SetFloat("Speed", speed);
    }
}
