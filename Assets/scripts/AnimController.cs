using System;
using UnityEngine;
using Photon.Pun;

public class AnimController : MonoBehaviourPunCallbacks, IPunObservable
{
    public GunMechanicManager manager;
    public Animator bodyController, legController, armController, eyesController;

    private PhotonView view;
    private float direction;
    private float networkedDirection;
    private string currentState;
    private string networkedState;
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
        view = GetComponent<PhotonView>();
        manager = GetComponent<GunMechanicManager>();
        direction = 0f;
        gameOver = false;
        currentState = null;

        ResetAnimator(armController, DEFAULT_IDLE);
        ResetAnimator(legController);
        ResetAnimator(bodyController);
        ResetAnimator(eyesController);

        SetAnimatorGroundFlag(true);
        SetAnimatorSpeed(0f);
    }

    private void ResetAnimator(Animator animator, string defaultAnimation = null)
    {
        if (animator != null)
        {
            animator.Rebind();
            animator.Update(0);
            if (!string.IsNullOrEmpty(defaultAnimation))
            {
                animator.Play(defaultAnimation);
            }
        }
    }

    private void Update()
    {
        if (ESCMenuListener.isPaused || IsLoading() || GameManager.Instance.CurrentState == GameManager.GameState.GameOver)
            return;

        if (view.IsMine)
        {
            direction = Input.GetAxis("Horizontal");

            if (Mathf.Abs(direction) > 0.1f)
            {
                SetAnimation(manager.heldItem.getRun(), DEFAULT_RUN);
            }
            else
            {
                SetAnimation(manager.heldItem.getIdle(), DEFAULT_IDLE);
            }

            SetAnimatorSpeed(Mathf.Abs(direction));
        }
        else
        {
            SetAnimatorSpeed(Mathf.Abs(networkedDirection));

            if (!string.IsNullOrEmpty(networkedState) && networkedState != currentState)
            {
                currentState = networkedState;
                armController.Play(currentState);
            }
        }
    }

    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.ContainsKey("Winner"))
        {
            gameOver = true;
        }
    }

    public void SetAnimation(string newAnimation, string defaultAnimation)
    {
        if (manager.isReloading || newAnimation == currentState)
            return;

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
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("IsLoading", out object isLoadingValue))
        {
            return (bool)isLoadingValue;
        }
        return false;
    }

    // Photon sync
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(direction);
            stream.SendNext(currentState);
        }
        else
        {
            networkedDirection = (float)stream.ReceiveNext();
            networkedState = (string)stream.ReceiveNext();
        }
    }
}
