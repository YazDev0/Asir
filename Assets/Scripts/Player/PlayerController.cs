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

    [Header("Particle Systems")]
    public ParticleSystem redParticle;
    public ParticleSystem greenParticle;
    public ParticleSystem blueParticle;

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

    public enum PlayerColor
    {
        Red,
        Green,
        Blue
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (animator == null)
            animator = GetComponent<Animator>();

        CreateGroundCheck();
        SetupRigidbody();
        UpdatePlayerParticles();
        UpdateAllBoxes();

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
        HandleCooldowns();
        UpdateAnimations();
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
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (isGrounded)
        {
            canWallJump = true;
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

    void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && wallJumpCooldown <= 0)
        {
            PerformJump(jumpForce);
            if (animator != null)
                animator.SetTrigger("Jump");
        }
    }

    void HandleWallJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isWallSliding && canWallJump && wallJumpCooldown <= 0)
        {
            PerformWallJump();
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
            animator.SetTrigger("WallJump");

        Debug.Log($"🦎 Wall Jump! Direction: {jumpDirection}");
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
        if (Input.GetKeyDown(KeyCode.Alpha1)) ChangeColor(PlayerColor.Red);
        if (Input.GetKeyDown(KeyCode.Alpha2)) ChangeColor(PlayerColor.Green);
        if (Input.GetKeyDown(KeyCode.Alpha3)) ChangeColor(PlayerColor.Blue);
        if (Input.GetKeyDown(KeyCode.E)) ChangeToNextColor();
    }

    void ChangeToNextColor()
    {
        if (currentColor == PlayerColor.Red)
            currentColor = PlayerColor.Green;
        else if (currentColor == PlayerColor.Green)
            currentColor = PlayerColor.Blue;
        else if (currentColor == PlayerColor.Blue)
            currentColor = PlayerColor.Red;

        UpdatePlayerParticles();
        UpdateAllBoxes();

        if (animator != null)
            animator.SetTrigger("ColorChange");
    }

    void ChangeColor(PlayerColor newColor)
    {
        if (currentColor == newColor) return;
        currentColor = newColor;
        UpdatePlayerParticles();
        UpdateAllBoxes();

        if (animator != null)
            animator.SetTrigger("ColorChange");
    }

    void UpdatePlayerParticles()
    {
        // إيقاف كل البارتيكلز أولاً
        if (redParticle != null) redParticle.Stop();
        if (greenParticle != null) greenParticle.Stop();
        if (blueParticle != null) blueParticle.Stop();

        // تشغيل البارتيكل المناسب للون الحالي
        switch (currentColor)
        {
            case PlayerColor.Red:
                if (redParticle != null) redParticle.Play();
                break;
            case PlayerColor.Green:
                if (greenParticle != null) greenParticle.Play();
                break;
            case PlayerColor.Blue:
                if (blueParticle != null) blueParticle.Play();
                break;
        }
    }

    void UpdateAllBoxes()
    {
        ColorBox[] allBoxes = FindObjectsOfType<ColorBox>();
        foreach (ColorBox box in allBoxes)
        {
            box.UpdateBoxState(this);
        }
    }

    void UpdateAnimations()
    {
        if (animator == null) return;

        animator.SetBool("IsGrounded", isGrounded);
        animator.SetBool("IsWallSliding", isWallSliding);
        animator.SetFloat("MoveSpeed", Mathf.Abs(rb.linearVelocity.x));
        animator.SetFloat("VerticalVelocity", rb.linearVelocity.y);
        animator.SetBool("IsTouchingWall", isTouchingWall);
        animator.SetInteger("PlayerColor", (int)currentColor);
    }

    public PlayerColor GetPlayerColorType()
    {
        return currentColor;
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