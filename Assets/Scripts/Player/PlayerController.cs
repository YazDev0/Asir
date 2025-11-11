using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 8f;
    public float jumpForce = 13f;

    [Header("Wall Jump")]
    public float wallSlideSpeed = 2f;
    public float wallJumpForce = 15f;
    public Vector2 wallJumpDirection = new Vector2(1.5f, 1.2f);
    public float wallCheckDistance = 0.6f;
    public LayerMask wallLayer;

    [Header("Color System")]
    public PlayerColor currentColor = PlayerColor.Red;
    public float colorDuration = 5f;
    private float colorTimer = 0f;
    private bool isColorActive = false;

    [Header("Particle Systems")]
    public ParticleSystem redParticle;
    public ParticleSystem greenParticle;
    public ParticleSystem dustParticle;

    [Header("Sound Effects")]
    public AudioClip footstepSound;
    public AudioClip jumpSound;
    public AudioClip colorChangeSound;
    public AudioClip landingSound;

    private AudioSource audioSource;
    private float footstepTimer = 0f;
    private float footstepDelay = 0.3f;

    [Header("Ground Detection")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.3f;
    public LayerMask groundLayer;

    [Header("Animation")]
    public Animator animator;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private bool isGrounded = false;
    private bool isTouchingWall = false;
    private bool isWallSliding = false;
    private int wallDirection = 0;
    private float wallJumpCooldown = 0f;
    private bool canWallJump = true;
    private float lastXDirection = 1f;
    private bool wasFalling = false;

    public enum PlayerColor
    {
        Red,
        Green
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        if (animator == null)
            animator = GetComponent<Animator>();

        CreateGroundCheck();
        SetupRigidbody();

        StopAllColorParticles();
        UpdateAllBoxesImmediately();

        if (wallLayer == 0) wallLayer = groundLayer;
        Debug.Log("🎮 Player Ready with Wall Jump & Particles!");
    }

    void SetupRigidbody()
    {
        rb.gravityScale = 3f;
        rb.freezeRotation = true;
    }

    void Update()
    {
        CheckGrounded();
        CheckWall();
        HandleWallSlide();
        HandleMovement();
        HandleJump();
        HandleWallJump();
        HandleColorChange();
        HandleColorTimer();
        HandleCooldowns();
        HandleFootsteps();
        UpdateAnimations();
        CheckFallState();
        CheckLanding();
    }

    void CreateGroundCheck()
    {
        if (groundCheck == null)
        {
            GameObject groundObj = new GameObject("GroundCheck");
            groundObj.transform.SetParent(transform);
            groundObj.transform.localPosition = new Vector3(0, -0.6f, 0);
            groundCheck = groundObj.transform;
        }
    }

    void CheckGrounded()
    {
        bool wasGrounded = isGrounded;
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (isGrounded)
        {
            canWallJump = true;

            if (!wasGrounded && dustParticle != null)
            {
                dustParticle.Play();
            }
        }
    }

    void CheckWall()
    {
        Vector2 rayOrigin = (Vector2)transform.position + new Vector2(0, 0.2f);
        float rayLength = wallCheckDistance;

        RaycastHit2D rightHit = Physics2D.Raycast(rayOrigin, Vector2.right, rayLength, wallLayer);
        RaycastHit2D leftHit = Physics2D.Raycast(rayOrigin, Vector2.left, rayLength, wallLayer);

        RaycastHit2D rightHit2 = Physics2D.Raycast(rayOrigin + new Vector2(0, 0.5f), Vector2.right, rayLength, wallLayer);
        RaycastHit2D leftHit2 = Physics2D.Raycast(rayOrigin + new Vector2(0, 0.5f), Vector2.left, rayLength, wallLayer);

        isTouchingWall = (rightHit.collider != null || leftHit.collider != null ||
                         rightHit2.collider != null || leftHit2.collider != null) && !isGrounded;

        if (rightHit.collider != null || rightHit2.collider != null)
        {
            wallDirection = 1;
        }
        else if (leftHit.collider != null || leftHit2.collider != null)
        {
            wallDirection = -1;
        }
        else
        {
            wallDirection = 0;
        }
    }

    void HandleWallSlide()
    {
        if (isTouchingWall && !isGrounded && wallDirection != 0)
        {
            float moveInput = Input.GetAxisRaw("Horizontal");

            if (Mathf.Sign(moveInput) == wallDirection && rb.linearVelocity.y < 0)
            {
                isWallSliding = true;
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Clamp(rb.linearVelocity.y, -wallSlideSpeed, float.MaxValue));
            }
            else
            {
                isWallSliding = false;
            }
        }
        else
        {
            isWallSliding = false;
        }
    }

    void HandleMovement()
    {
        if (wallJumpCooldown > 0) return;

        float moveX = Input.GetAxisRaw("Horizontal");

        if (!isWallSliding)
        {
            rb.linearVelocity = new Vector2(moveX * moveSpeed, rb.linearVelocity.y);
        }

        if (moveX != 0)
        {
            lastXDirection = Mathf.Sign(moveX);
            if (animator != null)
                animator.SetFloat("LastDirection", lastXDirection);
        }

        if (moveX > 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
        else if (moveX < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
    }

    void HandleFootsteps()
    {
        if (isGrounded && Mathf.Abs(rb.linearVelocity.x) > 0.1f)
        {
            footstepTimer -= Time.deltaTime;
            if (footstepTimer <= 0f)
            {
                PlaySound(footstepSound, 0.3f);
                footstepTimer = footstepDelay;

                if (dustParticle != null && !dustParticle.isPlaying)
                {
                    dustParticle.Play();
                }
            }
        }
        else
        {
            footstepTimer = 0f;
        }
    }

    void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && wallJumpCooldown <= 0)
        {
            PerformJump(jumpForce);
            PlaySound(jumpSound, 0.5f);

            if (animator != null)
            {
                animator.SetTrigger("Jump");
                animator.SetBool("IsJumping", true);
            }
        }
    }

    void HandleWallJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isWallSliding && canWallJump && wallJumpCooldown <= 0)
        {
            PerformWallJump();
            PlaySound(jumpSound, 0.5f);
        }
    }

    void PerformWallJump()
    {
        Vector2 jumpDirection = new Vector2(-wallDirection * wallJumpDirection.x, wallJumpDirection.y).normalized;
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(jumpDirection * wallJumpForce, ForceMode2D.Impulse);

        wallJumpCooldown = 0.4f;
        canWallJump = false;

        if (animator != null)
        {
            animator.SetTrigger("WallJump");
            animator.SetBool("IsJumping", true);
        }
    }

    void PerformJump(float force)
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, force);
    }

    void HandleCooldowns()
    {
        if (wallJumpCooldown > 0)
        {
            wallJumpCooldown -= Time.deltaTime;
        }
    }

    void HandleColorChange()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            ActivateRedColor();
            // ✅ إضافة تغيير اللون النشط
            ColorBox.ChangeActiveColor(ColorBox.BoxColor.Red);
        }
        else if (Input.GetKeyDown(KeyCode.Q))
        {
            ActivateGreenColor();
            // ✅ إضافة تغيير اللون النشط
            ColorBox.ChangeActiveColor(ColorBox.BoxColor.Green);
        }
    }

    void ActivateRedColor()
    {
        currentColor = PlayerColor.Red;
        ActivateColor();
    }

    void ActivateGreenColor()
    {
        currentColor = PlayerColor.Green;
        ActivateColor();
    }

    void ActivateColor()
    {
        PlaySound(colorChangeSound, 0.4f);

        StartColorParticle();

        colorTimer = colorDuration;
        isColorActive = true;

        UpdateAllBoxesImmediately();

        if (animator != null)
            animator.SetTrigger("ColorChange");

        Debug.Log($"🎨 {currentColor} activated for {colorDuration} seconds");
    }

    void HandleColorTimer()
    {
        if (isColorActive)
        {
            colorTimer -= Time.deltaTime;

            if (colorTimer <= 0f)
            {
                isColorActive = false;
                StopAllColorParticles();
                UpdateAllBoxesImmediately();
                Debug.Log("⏰ Color effect ended");
            }
        }
    }

    void StartColorParticle()
    {
        StopAllColorParticles();

        switch (currentColor)
        {
            case PlayerColor.Red:
                if (redParticle != null)
                {
                    redParticle.Play();
                }
                break;
            case PlayerColor.Green:
                if (greenParticle != null)
                {
                    greenParticle.Play();
                }
                break;
        }
    }

    void StopAllColorParticles()
    {
        if (redParticle != null)
        {
            redParticle.Stop();
            redParticle.Clear();
        }
        if (greenParticle != null)
        {
            greenParticle.Stop();
            greenParticle.Clear();
        }
    }

    void UpdateAllBoxesImmediately()
    {
        ColorBox[] allBoxes = FindObjectsOfType<ColorBox>();
        foreach (ColorBox box in allBoxes)
        {
            box.UpdateBoxState(this);
        }
    }

    void PlaySound(AudioClip clip, float volume = 1f)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip, volume);
        }
    }

    void UpdateAnimations()
    {
        if (animator == null) return;

        float verticalVelocity = rb.linearVelocity.y;

        animator.SetBool("IsGrounded", isGrounded);
        animator.SetBool("IsWallSliding", isWallSliding);
        animator.SetFloat("MoveSpeed", Mathf.Abs(rb.linearVelocity.x));
        animator.SetFloat("VerticalVelocity", verticalVelocity);
        animator.SetBool("IsTouchingWall", isTouchingWall);
        animator.SetInteger("PlayerColor", (int)currentColor);

        bool isJumping = verticalVelocity > 0.5f && !isGrounded && !isWallSliding;
        bool isFalling = verticalVelocity < -0.5f && !isGrounded && !isWallSliding;

        animator.SetBool("IsJumping", isJumping);
        animator.SetBool("IsFalling", isFalling);

        if (isGrounded && (isJumping || isFalling))
        {
            animator.ResetTrigger("Jump");
            animator.ResetTrigger("Fall");
            animator.SetBool("IsJumping", false);
            animator.SetBool("IsFalling", false);
        }
    }

    void CheckFallState()
    {
        if (isWallSliding)
        {
            animator.SetBool("IsFalling", false);
            return;
        }

        if (!isGrounded && rb.linearVelocity.y < -2f)
        {
            if (animator != null && !animator.GetBool("IsFalling"))
            {
                animator.SetTrigger("Fall");
                animator.SetBool("IsFalling", true);
                animator.SetBool("IsJumping", false);
            }
        }
    }

    void CheckLanding()
    {
        if (wasFalling && isGrounded)
        {
            PlaySound(landingSound, 0.3f);

            if (animator != null)
            {
                animator.SetTrigger("Land");
                animator.SetBool("IsFalling", false);
                animator.SetBool("IsJumping", false);
            }
        }

        wasFalling = !isGrounded && rb.linearVelocity.y < -0.1f;
    }

    public PlayerColor GetPlayerColorType()
    {
        return currentColor;
    }

    public bool IsColorActive()
    {
        return isColorActive;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (IsGroundObject(collision.gameObject))
        {
            isGrounded = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (IsGroundObject(collision.gameObject))
        {
            isGrounded = false;
        }
    }

    bool IsGroundObject(GameObject obj)
    {
        return obj.CompareTag("Ground") || obj.CompareTag("Platform") || obj.CompareTag("ColorBox");
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        Gizmos.color = Color.red;
        Vector2 rayOrigin = (Vector2)transform.position + new Vector2(0, 0.2f);
        Gizmos.DrawRay(rayOrigin, Vector2.right * wallCheckDistance);
        Gizmos.DrawRay(rayOrigin, Vector2.left * wallCheckDistance);

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(rayOrigin + new Vector2(0, 0.5f), Vector2.right * wallCheckDistance);
        Gizmos.DrawRay(rayOrigin + new Vector2(0, 0.5f), Vector2.left * wallCheckDistance);
    }

    public bool IsWallSliding => isWallSliding;
    public bool IsTouchingWall => isTouchingWall;
}