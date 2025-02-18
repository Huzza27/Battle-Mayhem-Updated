using System;
using System.Collections;
using UnityEngine;
using Photon.Pun;

public class AnimController : MonoBehaviourPunCallbacks
{
    public GunMechanicManager manager;
    public Animator bodyController, legController, armController, eyesController;

    private PhotonView view;
    private float direction;
    private string currentState;
    private bool gameOver = false;
    private const string DEFAULT_RUN = "run_arms";
    private const string DEFAULT_IDLE = "idel_arms";

    private void Start()
    {
        view = GetComponent<PhotonView>();
        manager = GetComponent<GunMechanicManager>();
    }


    private void Awake()
    {
        ResetAnimControllerState();
    }


    public void ResetAnimControllerState()
    {
        // Reset Photon View
        view = GetComponent<PhotonView>();

        // Reset manager reference
        manager = GetComponent<GunMechanicManager>();

        // Reset directional input and game over state
        direction = 0f;
        gameOver = false;

        // Reset current animation state
        currentState = null;

        // Reset all animator parameters
        ResetAnimator(armController, DEFAULT_IDLE);
        ResetAnimator(legController);
        ResetAnimator(bodyController);
        ResetAnimator(eyesController);

        // Ensure ground flags and speeds are reset
        SetAnimatorGroundFlag(true);
        SetAnimatorSpeed(0f);
    }

    private void ResetAnimator(Animator animator, string defaultAnimation = null)
    {
        if (animator != null)
        {
            animator.Rebind(); // Reset the animator to its default state
            animator.Update(0); // Ensure it is refreshed
            if (!string.IsNullOrEmpty(defaultAnimation))
            {
                animator.Play(defaultAnimation); // Set to a default animation if specified
            }
        }
    }

    private void Update()
    {
        if (view.IsMine)
        {
            if (ESCMenuListener.isPaused || IsLoading() || GameManager.Instance.gameOver) //Check for the game being pasued, disable shooting if true;
            {
                return;
            }
            // Process player input for direction
            direction = Input.GetAxis("Horizontal");

            // Manage animation based on direction
            view.RPC("ManageAnimationBasedOnDirection", RpcTarget.AllBuffered, direction);
        }
    }

    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.ContainsKey("Winner"))
        {
            gameOver = true;
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
    public bool IsLoading()
    {
        // Check if the room has the "IsLoading" property
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("IsLoading", out object isLoadingValue))
        {
            // Return the value cast as a boolean
            return (bool)isLoadingValue;
        }

        // Default to false if the property is not set
        return false;
    }

}
