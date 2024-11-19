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


    [Header("Wall Jumping")]
    public float moveSpeed = 5f;
    public float wallJumpForce = 10f;
    public float wallSlideSpeed = 2f;
    public float wallCheckDistance = 0.5f;
    public float spinDuration = 0.5f;
    public float spinSpeed = 5f;
    public LayerMask wallLayer;

    private bool isWallSliding;
    private bool isWallJumping;

    [Header("Dashing")]
    public float dashForce = 15f;     // Force of the dash
    public float dashDuration = 0.5f; // How long the dash lasts
    public float dashCooldown = 1f;   // Time between dashes

    private bool isDashing = false;
    private bool canDash = true;
    private float dashDirection;


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

            HandleJumping();
            HandleWallJump();
            HandleWalkingParticles(isMoving);
            HandleDashInput();
            HandleDroppingThroughPlatforms();
            WallCheck();
        }
    }

    private void HandleDashInput()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash)
        {
            // Set dash direction based on input (right or left)
            if (Input.GetKey(KeyCode.D))
            {
                dashDirection = 1f;
            }
            else if (Input.GetKey(KeyCode.A))
            {
                dashDirection = -1f;
            }
            else
            {
                if(facingRight)
                {
                    dashDirection =  1f;
                }
                else
                {
                    dashDirection = -1f;
                }
            }

            // Start the dash coroutine
            Dash();
        }
    }

    private void HandleWalkingParticles(bool isMoving)
    {

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
    }

    private void HandleJumping()
    {
        

        if (Input.GetKeyDown(KeyCode.W) && !isWallSliding)
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
    }

    private void HandleWallJump()
    {
        if (Input.GetKeyDown(KeyCode.W) && isWallSliding)
        {
            view.RPC("ManageWalkingParticles", RpcTarget.All, false);
            isWallJumping = true;
            float jumpDirection = facingRight ? -1 : 1; // Push off in the opposite direction of the wall
            rb.velocity = new Vector2(jumpDirection * wallJumpForce, jumpForce + wallJumpForce);
            StartCoroutine(WallJumpSpin());
            Invoke("ResetWallJump", 0.2f); // Allow regular jumping after a short delay
        }
    }

    private void WallCheck()
    {
        // Check for wall on the left or right
        bool wallOnLeft = Physics2D.Raycast(transform.position, Vector2.left, wallCheckDistance, wallLayer);
        bool wallOnRight = Physics2D.Raycast(transform.position, Vector2.right, wallCheckDistance, wallLayer);

        // Determine if player is sliding on the wall
        isWallSliding = (wallOnLeft || wallOnRight) && !isGrounded && rb.velocity.y < 0;

        if (isWallSliding)
        {
            // Apply wall slide speed if sliding down
            rb.velocity = new Vector2(rb.velocity.x, -wallSlideSpeed);
        }
    }

    private IEnumerator WallJumpSpin()
    {
        float elapsedTime = 0f;
        while (elapsedTime < spinDuration && !isGrounded)
        {
            // Rotate the player on the Z-axis
            transform.Rotate(0, 0, spinSpeed * 10f * Time.deltaTime, Space.World);

            elapsedTime += Time.deltaTime;
            yield return null; // Wait for the next frame
        }

        // Reset the rotation to zero at the end of the spin
        transform.rotation = Quaternion.Euler(0, facingRight ? 0 : 180, 0);
    }

    private void ResetWallJump()
    {
        isWallJumping = false;
    }

    private void Dash()
    {
        // Start the dash
        isDashing = true;
        canDash = false;

        // Apply dash force in the direction of the dash
        rb.AddForce(new Vector2(dashDirection * dashForce, 0f), ForceMode2D.Impulse);
        StartCoroutine(DashTimer(dashDuration));
    }

    private IEnumerator DashTimer(float dashDuariton)
    {
        yield return new WaitForSeconds(dashDuariton);
        canDash = true;
        isDashing = false;

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
        if (!isDashing)
        {
            rb.AddForce(Vector2.right * horizontal * acceleration);
            if (Mathf.Abs(rb.velocity.x) > maxSpeed)
            {
                rb.velocity = new Vector2(Mathf.Sign(rb.velocity.x) * maxSpeed, rb.velocity.y);
            }
        }
    }
}
