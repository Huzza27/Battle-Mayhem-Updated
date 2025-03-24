using System.Collections;
using UnityEngine;
using Photon.Pun;
using Unity.VisualScripting;
using Photon.Realtime;

public class Movement : MonoBehaviourPunCallbacks
{
    public PhotonView view;
    public float acceleration = 15.0f;
    const float MAX_SPEED_FINAL = 10.0f;
    public float maxSpeed = 10.0f;
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
    private bool isSlow = false;


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
    public int dashCooldown;   // Time between dashes
    private Health healthScript;
    private bool isDashing = false;
    private bool canDash = true;
    private float dashDirection;
    public TrailRenderer trail;
    private bool gameOver = false;
    public AudioSource audio;
    public AudioClip dashAudio;

    private bool isInitialized = false;
    private const string INIT_PROPERTY = "IsInitialized";


    void Awake()
    {
        // Get references first in Awake
        view = GetComponent<PhotonView>();
        rb = GetComponent<Rigidbody2D>();
        healthScript = GetComponent<Health>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        ResetMovementState();
    }


    public void ResetMovementState()
    {
        // Reset core movement variables
        direction = 0f;
        isGrounded = true;
        facingRight = true;
        isWallSliding = false;
        isWallJumping = false;
        isDashing = false;
        canDash = true;
        canDoubleJump = false;
        isSlow = false;
        maxSpeed = MAX_SPEED_FINAL;

        // Reset Rigidbody properties
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.gravityScale = gravityScale;
            rb.drag = linearDrag;
        }

        // Reset sprite or visual state
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = false;
            transform.rotation = Quaternion.identity; // Ensure the character is upright
        }

        // Reset walking trail particles
        if (walkingTrail != null)
        {
            walkingTrail.Stop();
            currentlyPlayingParticles = false;
        }

        // Reset platform drop layer
        if (playerBottomCollider != null)
        {
            playerBottomCollider.layer = LayerMask.NameToLayer("Player");
        }
        gameObject.layer = LayerMask.NameToLayer("Player");

        // Reset dashing trail
        if (trail != null)
        {
            trail.enabled = false;
        }

        // Ensure any wall jumping spin is stopped
        StopAllCoroutines();

        // Reset platform-related variables
        currentPlatform = null;

        // Update particle colors based on the current map theme
        UpdateParticleColors(GameManager.Instance.MapSelection);
    }
    void Start()
    {
        // Only proceed with initialization if this is our player
        if (view.IsMine)
        {
            InitializeMovement();
        }
    }

    private void InitializeMovement()
    {
        // We already have references from Awake, so just set up the physics
        rb.gravityScale = gravityScale;
        rb.drag = linearDrag;

        UpdateParticleColors(GameManager.Instance.MapSelection);

        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer not found on the player object.");
        }

        isInitialized = true;

        // Set player property to mark as initialized
        var props = new ExitGames.Client.Photon.Hashtable
    {
        { INIT_PROPERTY, true }
    };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (!view.IsMine) return;

        if (changedProps.ContainsKey(INIT_PROPERTY))
        {
            isInitialized = (bool)changedProps[INIT_PROPERTY];
        }
    }


    void Update()
    {
        if (view.IsMine)
        {
            if (ESCMenuListener.isPaused || GameLoadingManager.IsLoading() || GameManager.Instance.gameOver) //Check for the game being pasued, disable movement if true;
            {
                return;
            }
            direction = Input.GetAxis("Horizontal");
            bool isMoving = Mathf.Abs(direction) > 0.3;

            HandleJumping();
            //HandleWallJump();
            HandleWalkingParticles(isMoving);
            HandleDashInput();
            HandleDroppingThroughPlatforms();
            //WallCheck();
        }
    }

    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.ContainsKey("Winner"))
        {
            gameOver = true;
        }
    }


    private void HandleDashInput()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash && !isSlow)
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
            view.RPC("Dash", RpcTarget.All);
        }
    }

    private void HandleWalkingParticles(bool isMoving)
    {
        // Update particle colors based on the map theme
        

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

    private void UpdateParticleColors(int mapTheme)
    {
        if (walkingTrail == null)
        {
            Debug.LogError("No Particle System assigned!");
            return;
        }

        // Access the main module and color over lifetime module
        var mainModule = walkingTrail.main;
        var colorOverLifetime = walkingTrail.colorOverLifetime;
        colorOverLifetime.enabled = true;

        Gradient gradient = new Gradient();

        // Set colors based on the map theme
        if (mapTheme == 0)
        {
            Debug.Log("Setting Green Particles");
            // Green to Brown for Grass
            gradient.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(Color.green, 0.0f),
                    new GradientColorKey(new Color(0.6f, 0.3f, 0.1f), 1.0f) // Brownish
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(1.0f, 0.0f),
                    new GradientAlphaKey(1.0f, 1.0f)
                }
            );
        }
        else if (mapTheme == 1)
        {
            // Yellow to Orange for Desert
            Debug.Log("Setting Orange Particles");

            gradient.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(Color.red, 0.0f),
                    new GradientColorKey(new Color(1.0f, 0.5f, 0.0f), 1.0f) // Orange
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(1.0f, 0.0f),
                    new GradientAlphaKey(1.0f, 1.0f)
                }
            );
        }

            else if (mapTheme == 2)
            {
                // White to Gray to Blue for Ice Map
                Debug.Log("Setting Ice Particles");

                gradient.SetKeys(
                    new GradientColorKey[] {
            new GradientColorKey(Color.white, 0.0f),           // White at the start
            new GradientColorKey(new Color(0.7f, 0.7f, 0.7f), 0.5f), // Light Gray in the middle
            new GradientColorKey(new Color(0.0f, 0.5f, 1.0f), 1.0f)  // Ice Blue at the end
                    },
                    new GradientAlphaKey[] {
            new GradientAlphaKey(1.0f, 0.0f),
            new GradientAlphaKey(1.0f, 1.0f)
                    }
                );
        }
        else
        {
            Debug.LogWarning("Unhandled map theme! Defaulting to Grass theme colors.");
            gradient.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(Color.green, 0.0f),
                    new GradientColorKey(new Color(0.6f, 0.3f, 0.1f), 1.0f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(1.0f, 0.0f),
                    new GradientAlphaKey(1.0f, 1.0f)
                }
            );
        }

        // Apply the gradient to the color over lifetime module
        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);
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

    [PunRPC]
    private void Dash()
    {
        if (!canDash) return;

        // Start the dash
        isDashing = true;
        canDash = false;

        rb.velocity = Vector2.zero;
        // Apply dash force in the direction of the dash
        trail.enabled = true;
        rb.velocity = new Vector2(dashDirection * dashForce, rb.velocity.y); // Set velocity directly for precise control
        audio.PlayOneShot(dashAudio);
        // Start the dash and cooldown timers
        StartCoroutine(DashEndTimer());
        StartCoroutine(DashCooldownTimer());
    }

    private IEnumerator DashEndTimer()
    {
        // Dash lasts only for `dashDuration`
        yield return new WaitForSeconds(dashDuration);
        // End the dash but leave cooldown active
        isDashing = false;
        trail.enabled = false;
    }

    private IEnumerator DashCooldownTimer()
    {
        view.RPC("CallDashUIAnimationForView", view.Owner, dashCooldown);
        // Wait for the cooldown to finish
        yield return new WaitForSeconds(dashCooldown);

        // Allow dashing again
        canDash = true;
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
        if (isGrounded || canDoubleJump && !isSlow)
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
        if (view.IsMine && Mathf.Abs(direction) > 0)
        {
            MoveCharacter(direction);
        }
    }

    [PunRPC]
    public void FlipCharacterBasedOnDirection(float horizontalInput)
    {
        facingRight = horizontalInput > 0;
        Debug.Log("Moving at " + maxSpeed + " speed");
        transform.rotation = Quaternion.Euler(0, facingRight ? 180 : 0, 0);
    }

    public void SlowPlayer(float newSpeed)
    {
        maxSpeed = newSpeed;
        isSlow = true;
    }

    public void ResetSpeed()
    {
        maxSpeed = MAX_SPEED_FINAL;
        isSlow = false;
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
