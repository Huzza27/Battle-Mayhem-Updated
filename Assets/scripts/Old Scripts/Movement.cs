using System.Collections;
using UnityEngine;
using Photon.Pun;

public class Movement : MonoBehaviour
{
    PhotonView view;
    public float acceleration = 15.0f;
    public float maxSpeed = 5.0f;
    public float jumpForce = 7.0f;
    public float gravityScale = 3.0f;
    public float linearDrag = 4.0f;
    private Rigidbody2D rb;
    public bool isGrounded;
    private float direction = 0f;
    public bool facingRight;
    public bool canDoubleJump = false;
    private SpriteRenderer spriteRenderer;
    public GameObject currentPlatform;
    public GameObject playerBottomCollider;
    [SerializeField] float droppingTime = 0.5f;
    public ParticleSystem walkingTrail;
    private bool currentlyPlayingParticles = false; // To keep track of particle state

    void Start()
    {

        view = GetComponent<PhotonView>();
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = gravityScale;
        rb.drag = linearDrag;
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer not found on the player object.");
        }
    }

    void Update()
    {
        if (view.IsMine)
        {
            direction = Input.GetAxis("Horizontal");
            bool isMoving = Mathf.Abs(direction) > 0.3;

            if ((direction > 0 && !facingRight) || (direction < 0 && facingRight))
            {
                view.RPC("FlipCharacterBasedOnDirection", RpcTarget.AllBuffered, direction);
            }

            if (Input.GetKeyDown(KeyCode.W))
            {
                if (isGrounded)
                {
                    view.RPC("ManageWalkingParticles", RpcTarget.All, false);
                    Jump();
                    canDoubleJump = true;
                }
                else if (canDoubleJump)
                {
                    view.RPC("ManageWalkingParticles", RpcTarget.All, false);
                    Jump();
                    canDoubleJump = false;
                }
            }

            // Manage particle effects
            if (isMoving && isGrounded && !currentlyPlayingParticles)
            {
                view.RPC("ManageWalkingParticles", RpcTarget.All, true);
                currentlyPlayingParticles = true;
            }
            else if ((!isMoving || !isGrounded) && currentlyPlayingParticles)
            {
                view.RPC("ManageWalkingParticles", RpcTarget.All, false);
                currentlyPlayingParticles = false;
            }

            // Sync transform in Update for smoother updates

            HandleDroppingThroughPlatforms();
        }
    }


    public void HandleDroppingThroughPlatforms()
    {
        if (Input.GetKeyDown(KeyCode.S) && isGrounded && currentPlatform != null)
        {
            view.RPC("DropThroughPlatform", RpcTarget.All); // Sync platform drop across all clients
        }
    }

    [PunRPC]
    void DropThroughPlatform()
    {
        StartCoroutine(EnableDroppedCollider()); // Sync platform drop behavior across clients
    }

    [PunRPC]
    void ManageWalkingParticles(bool play)
    {
        if (play)
        {
            walkingTrail.Play();
        }
        else
        {
            walkingTrail.Stop();
        }
    }

    IEnumerator EnableDroppedCollider()
    {
        gameObject.layer = LayerMask.NameToLayer("DropThrough");
        playerBottomCollider.layer = LayerMask.NameToLayer("DropThrough");
        yield return new WaitForSeconds(droppingTime);
        gameObject.layer = LayerMask.NameToLayer("Player");
        playerBottomCollider.layer = LayerMask.NameToLayer("Player");
    }

    public void Jump()
    {
        if (isGrounded || canDoubleJump)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            isGrounded = false;
            if (!canDoubleJump)
            {
                // Prevent further jumps if double jump has been used
                canDoubleJump = false;
            }
            else
            {
                // Allow double jump if still available
                canDoubleJump = true;
            }
        }
    }

    void FixedUpdate()
    {
        if (view.IsMine)
        {
            MoveCharacter(direction);
        }
    }

    [PunRPC]
    public void FlipCharacterBasedOnDirection(float horizontalInput)
    {
        facingRight = horizontalInput > 0;
        transform.rotation = Quaternion.Euler(0, facingRight ? 180 : 0, 0);
    }

    public void MoveCharacter(float horizontal)
    {
        rb.AddForce(Vector2.right * horizontal * acceleration);
        if (Mathf.Abs(rb.velocity.x) > maxSpeed)
        {
            rb.velocity = new Vector2(Mathf.Sign(rb.velocity.x) * maxSpeed, rb.velocity.y);
        }
    }
}
