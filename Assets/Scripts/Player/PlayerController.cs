using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // =============== [HEADER SECTIONS] ===============
    // كل الـHeaders هنا تنظم إعدادات اللاعب في إنسبكتور الوحدة

    [Header("Movement")]
    public float moveSpeed = 8f;          // سرعة حركة اللاعب يمين/يسار
    public float jumpForce = 13f;         // قوة القفز العادي من الأرض

    [Header("Wall Jump")]
    public float wallSlideSpeed = 2f;     // سرعة الانزلاق على الحائط
    public float wallJumpForce = 15f;     // قوة قفز الحائط
    public Vector2 wallJumpDirection = new Vector2(1.5f, 1.2f); // اتجاه قفز الحائط (X,Y)
    public float wallCheckDistance = 0.6f; // مسافة كشف الحائط
    public LayerMask wallLayer;           // طبقات الحائط (يمكن مشاركة groundLayer)

    [Header("Color System")]
    public PlayerColor currentColor = PlayerColor.Red; // اللون الحالي للاعب
    public float colorDuration = 5f;      // مدة تأثير اللون
    private float colorTimer = 0f;        // مؤقت لتأثير اللون
    private bool isColorActive = false;   // هل تأثير اللون مفعل؟

    [Header("Particle Systems")]
    public ParticleSystem redParticle;    // جسيمات تأثير اللون الأحمر
    public ParticleSystem greenParticle;  // جسيمات تأثير اللون الأخضر
    public ParticleSystem dustParticle;   // جسيمات الغبار عند الحركة والهبوط

    [Header("Sound Effects")]
    public AudioClip footstepSound;       // صوت خطوات
    public AudioClip jumpSound;           // صوت قفز
    public AudioClip colorChangeSound;    // صوت تغيير اللون
    public AudioClip landingSound;        // صوت الهبوط

    private AudioSource audioSource;      // مصدر الصوت المرفق باللاعب
    private float footstepTimer = 0f;     // مؤقت لتكرار صوت الخطوات
    private float footstepDelay = 0.3f;   // التأخير بين كل خطوة

    [Header("Ground Detection")]
    public Transform groundCheck;         // نقطة فحص الأرض (أسفل اللاعب)
    public float groundCheckRadius = 0.3f; // نصف دائرة فحص الأرض
    public LayerMask groundLayer;         // طبقات الأرض

    [Header("Animation")]
    public Animator animator;             // المتحكم في أنيميشن اللاعب

    // =============== [PRIVATE VARIABLES] ===============
    private Rigidbody2D rb;               // الجسم الفيزيائي للاعب
    private SpriteRenderer spriteRenderer; // عرض الصورة للاعب
    private bool isGrounded = false;      // هل اللاعب ملامس للأرض؟
    private bool isTouchingWall = false;  // هل اللاعب يلمس حائطًا؟
    private bool isWallSliding = false;   // هل اللاعب ينزلق على الحائط؟
    private int wallDirection = 0;        // اتجاه الحائط (1=يمين, -1=يسار, 0=لا يوجد)
    private float wallJumpCooldown = 0f;  // تأخير قبل إعادة قفز الحائط
    private bool canWallJump = true;      // هل يمكن قفز الحائط؟
    private float lastXDirection = 1f;    // آخر اتجاه حركة أفقي (للأنيميشن)
    private bool wasFalling = false;      // هل كان اللاعب يسقط في الفريم السابق؟

    // =============== [ENUM DEFINITION] ===============
    public enum PlayerColor
    {
        Red,    // اللون الأحمر
        Green   // اللون الأخضر
    }

    // =============== [START METHOD] ===============
    void Start()
    {
        // الحصول على المكونات المرفقة باللاعب
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();

        // إنشاء مصدر صوتي إذا لم يكن موجودًا
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // الحصول على الأنيميتر إذا لم يتم تعيينه
        if (animator == null)
            animator = GetComponent<Animator>();

        // إنشاء نقطة فحص الأرض إذا لم تكن موجودة
        CreateGroundCheck();

        // إعداد خصائص الجسم الفيزيائي
        SetupRigidbody();

        // إيقاف جميع الجسيمات في البداية
        StopAllColorParticles();

        // تحديث حالة كل الصناديق الملونة فورًا
        UpdateAllBoxesImmediately();

        // إذا لم يتم تعيين طبقة الحائط، استخدم طبقة الأرض
        if (wallLayer == 0) wallLayer = groundLayer;

        Debug.Log("🎮 Player Ready with Wall Jump & Particles!");
    }

    // إعداد خصائص الجسم الفيزيائي للاعب
    void SetupRigidbody()
    {
        rb.gravityScale = 3f;     // قوة الجاذبية (تأثير على سرعة السقوط)
        rb.freezeRotation = true; // منع الدوران عند الاصطدام
    }

    // =============== [UPDATE METHOD] ===============
    void Update()
    {
        // ترتيب عمليات التحديث (كل فريم)
        CheckGrounded();      // فحص الأرض
        CheckWall();          // فحص الحائط
        HandleWallSlide();    // معالجة انزلاق الحائط
        HandleMovement();     // معالجة الحركة
        HandleJump();         // معالجة القفز العادي
        HandleWallJump();     // معالجة قفز الحائط
        HandleColorChange();  // معالجة تغيير اللون
        HandleColorTimer();   // معالجة مؤقت اللون
        HandleCooldowns();    // معالجة التأخيرات
        HandleFootsteps();    // معالجة أصوات الخطوات
        UpdateAnimations();   // تحديث الأنيميشن
        CheckFallState();     // فحص حالة السقوط
        CheckLanding();       // فحص الهبوط
    }

    // =============== [GROUND CHECK SYSTEM] ===============
    // إنشاء نقطة فحص الأرض إذا لم تكن موجودة
    void CreateGroundCheck()
    {
        if (groundCheck == null)
        {
            GameObject groundObj = new GameObject("GroundCheck");
            groundObj.transform.SetParent(transform);
            groundObj.transform.localPosition = new Vector3(0, -0.6f, 0); // أسفل اللاعب
            groundCheck = groundObj.transform;
        }
    }

    // فحص إذا كان اللاعب ملامسًا للأرض
    void CheckGrounded()
    {
        bool wasGrounded = isGrounded; // حفظ الحالة السابقة
        // استخدام دائرة لاكتشاف التصادم مع الأرض
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (isGrounded)
        {
            canWallJump = true; // السماح بقفز الحائط عند العودة للأرض

            // تشغيل جسيمات الغبار عند الهبوط
            if (!wasGrounded && dustParticle != null)
            {
                dustParticle.Play();
            }
        }
    }

    // =============== [WALL CHECK SYSTEM] ===============
    // فحص إذا كان اللاعب يلمس حائطًا
    void CheckWall()
    {
        Vector2 rayOrigin = (Vector2)transform.position + new Vector2(0, 0.2f); // نقطة البداية للأشعة
        float rayLength = wallCheckDistance; // طول الأشعة

        // إرسال أشعة من الجانبين (ارتفاعين مختلفين)
        RaycastHit2D rightHit = Physics2D.Raycast(rayOrigin, Vector2.right, rayLength, wallLayer);
        RaycastHit2D leftHit = Physics2D.Raycast(rayOrigin, Vector2.left, rayLength, wallLayer);

        RaycastHit2D rightHit2 = Physics2D.Raycast(rayOrigin + new Vector2(0, 0.5f), Vector2.right, rayLength, wallLayer);
        RaycastHit2D leftHit2 = Physics2D.Raycast(rayOrigin + new Vector2(0, 0.5f), Vector2.left, rayLength, wallLayer);

        // إذا اصطدم أي شعاع بحائط، واللاعب ليس على الأرض
        isTouchingWall = (rightHit.collider != null || leftHit.collider != null ||
                         rightHit2.collider != null || leftHit2.collider != null) && !isGrounded;

        // تحديد اتجاه الحائط
        if (rightHit.collider != null || rightHit2.collider != null)
        {
            wallDirection = 1; // حائط على اليمين
        }
        else if (leftHit.collider != null || leftHit2.collider != null)
        {
            wallDirection = -1; // حائط على اليسار
        }
        else
        {
            wallDirection = 0; // لا يوجد حائط
        }
    }

    // =============== [WALL SLIDE SYSTEM] ===============
    // معالجة انزلاق اللاعب على الحائط
    void HandleWallSlide()
    {
        // شروط الانزلاق: يلمس حائطًا، ليس على الأرض، اتجاه الحائط محدد
        if (isTouchingWall && !isGrounded && wallDirection != 0)
        {
            float moveInput = Input.GetAxisRaw("Horizontal"); // اتجاه الإدخال

            // إذا كان اللاعب يضغط باتجاه الحائط ويتحرك للأسفل
            if (Mathf.Sign(moveInput) == wallDirection && rb.linearVelocity.y < 0)
            {
                isWallSliding = true;
                // التحكم في سرعة السقوط أثناء الانزلاق
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

    // =============== [MOVEMENT SYSTEM] ===============
    // معالجة حركة اللاعب يمين/يسار
    void HandleMovement()
    {
        if (wallJumpCooldown > 0) return; // لا حركة أثناء تأخير قفز الحائط

        float moveX = Input.GetAxisRaw("Horizontal"); // الحصول على الإدخال (-1, 0, 1)

        if (!isWallSliding) // لا حركة أفقية أثناء انزلاق الحائط
        {
            rb.linearVelocity = new Vector2(moveX * moveSpeed, rb.linearVelocity.y);
        }

        // حفظ آخر اتجاه حركة (للأنيميشن)
        if (moveX != 0)
        {
            lastXDirection = Mathf.Sign(moveX);
            if (animator != null)
                animator.SetFloat("LastDirection", lastXDirection);
        }

        // قلب اتجاه الصورة حسب اتجاه الحركة
        if (moveX > 0)
        {
            transform.localScale = new Vector3(1, 1, 1); // الاتجاه الأصلي
        }
        else if (moveX < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1); // قلب الصورة
        }
    }

    // =============== [FOOTSTEP SYSTEM] ===============
    // معالجة أصوات خطوات المشي
    void HandleFootsteps()
    {
        // شروط تشغيل الصوت: على الأرض وتتحرك
        if (isGrounded && Mathf.Abs(rb.linearVelocity.x) > 0.1f)
        {
            footstepTimer -= Time.deltaTime;
            if (footstepTimer <= 0f)
            {
                PlaySound(footstepSound, 0.8f); // تشغيل صوت الخطوة
                footstepTimer = footstepDelay; // إعادة ضبط المؤقت

                // تشغيل جسيمات الغبار
                if (dustParticle != null && !dustParticle.isPlaying)
                {
                    dustParticle.Play();
                }
            }
        }
        else
        {
            footstepTimer = 0f; // إعادة ضبط المؤقت
        }
    }

    // =============== [JUMP SYSTEM] ===============
    // معالجة القفز العادي من الأرض
    void HandleJump()
    {
        // شروط القفز: ضغط Space، على الأرض، لا يوجد تأخير قفز حائط
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && wallJumpCooldown <= 0)
        {
            PerformJump(jumpForce); // تنفيذ القفز
            PlaySound(jumpSound, 0.5f); // تشغيل صوت القفز

            // تحريك الأنيميشن
            if (animator != null)
            {
                animator.SetTrigger("Jump");
                animator.SetBool("IsJumping", true);
            }
        }
    }

    // معالجة قفز الحائط
    void HandleWallJump()
    {
        // شروط قفز الحائط: ضغط Space، ينزلق على الحائط، مسموح بقفز الحائط، لا تأخير
        if (Input.GetKeyDown(KeyCode.Space) && isWallSliding && canWallJump && wallJumpCooldown <= 0)
        {
            PerformWallJump(); // تنفيذ قفز الحائط
            PlaySound(jumpSound, 0.5f); // تشغيل صوت القفز
        }
    }

    // تنفيذ قفز الحائط
    void PerformWallJump()
    {
        // حساب اتجاه القفز (بعيدًا عن الحائط)
        Vector2 jumpDirection = new Vector2(-wallDirection * wallJumpDirection.x, wallJumpDirection.y).normalized;
        rb.linearVelocity = Vector2.zero; // إعادة تعيين السرعة
        rb.AddForce(jumpDirection * wallJumpForce, ForceMode2D.Impulse); // تطبيق القوة

        wallJumpCooldown = 0.4f; // تأخير قبل إعادة القفز
        canWallJump = false; // منع قفز حائط متعدد

        // تحريك الأنيميشن
        if (animator != null)
        {
            animator.SetTrigger("WallJump");
            animator.SetBool("IsJumping", true);
        }
    }

    // تنفيذ القفز العادي
    void PerformJump(float force)
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, force);
    }

    // =============== [COOLDOWN SYSTEM] ===============
    // معالجة التأخيرات الزمنية
    void HandleCooldowns()
    {
        if (wallJumpCooldown > 0)
        {
            wallJumpCooldown -= Time.deltaTime; // تقليل الوقت
        }
    }

    // =============== [COLOR SYSTEM] ===============
    // معالجة تغيير لون اللاعب
    void HandleColorChange()
    {
        if (Input.GetKeyDown(KeyCode.E)) // زر الأحمر
        {
            ActivateRedColor();
            // تحديث الصناديق الملونة للون الأحمر النشط
            ColorBox.ChangeActiveColor(ColorBox.BoxColor.Red);
        }
        else if (Input.GetKeyDown(KeyCode.Q)) // زر الأخضر
        {
            ActivateGreenColor();
            // تحديث الصناديق الملونة للون الأخضر النشط
            ColorBox.ChangeActiveColor(ColorBox.BoxColor.Green);
        }
    }

    // تفعيل اللون الأحمر
    void ActivateRedColor()
    {
        currentColor = PlayerColor.Red;
        ActivateColor(); // تشغيل التأثير العام
    }

    // تفعيل اللون الأخضر
    void ActivateGreenColor()
    {
        currentColor = PlayerColor.Green;
        ActivateColor(); // تشغيل التأثير العام
    }

    // تشغيل تأثير اللون (مشترك بين الأحمر والأخضر)
    void ActivateColor()
    {
        PlaySound(colorChangeSound, 0.1f); // صوت تغيير اللون

        StartColorParticle(); // تشغيل الجسيمات

        colorTimer = colorDuration; // ضبط المؤقت
        isColorActive = true; // تفعيل حالة اللون

        UpdateAllBoxesImmediately(); // تحديث كل الصناديق

        // تحريك أنيميشن تغيير اللون
        if (animator != null)
            animator.SetTrigger("ColorChange");

        Debug.Log($"🎨 {currentColor} activated for {colorDuration} seconds");
    }

    // معالجة مؤقت تأثير اللون
    void HandleColorTimer()
    {
        if (isColorActive)
        {
            colorTimer -= Time.deltaTime; // تقليل الوقت

            if (colorTimer <= 0f) // انتهى الوقت
            {
                isColorActive = false; // إلغاء تفعيل اللون
                StopAllColorParticles(); // إيقاف الجسيمات
                UpdateAllBoxesImmediately(); // تحديث الصناديق
                Debug.Log("⏰ Color effect ended");
            }
        }
    }

    // تشغيل جسيمات اللون المناسب
    void StartColorParticle()
    {
        StopAllColorParticles(); // إيقاف أي جسيمات سابقة

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

    // إيقاف كل جسيمات الألوان
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

    // تحديث كل الصناديق الملونة في المشهد
    void UpdateAllBoxesImmediately()
    {
        ColorBox[] allBoxes = FindObjectsOfType<ColorBox>();
        foreach (ColorBox box in allBoxes)
        {
            box.UpdateBoxState(this); // إرسال حالة اللاعب لكل صندوق
        }
    }

    // =============== [AUDIO SYSTEM] ===============
    // تشغيل صوت مع مستوى صوت محدد
    void PlaySound(AudioClip clip, float volume = 1f)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip, volume);
        }
    }

    // =============== [ANIMATION SYSTEM] ===============
    // تحديث معاملات الأنيميشن
    void UpdateAnimations()
    {
        if (animator == null) return;

        float verticalVelocity = rb.linearVelocity.y;

        // تعيين معاملات الأنيميشن بناءً على حالة اللاعب
        animator.SetBool("IsGrounded", isGrounded);
        animator.SetBool("IsWallSliding", isWallSliding);
        animator.SetFloat("MoveSpeed", Mathf.Abs(rb.linearVelocity.x)); // السرعة المطلقة
        animator.SetFloat("VerticalVelocity", verticalVelocity);
        animator.SetBool("IsTouchingWall", isTouchingWall);
        animator.SetInteger("PlayerColor", (int)currentColor);

        // تحديد حالة القفز والسقوط
        bool isJumping = verticalVelocity > 0.5f && !isGrounded && !isWallSliding;
        bool isFalling = verticalVelocity < -0.5f && !isGrounded && !isWallSliding;

        animator.SetBool("IsJumping", isJumping);
        animator.SetBool("IsFalling", isFalling);

        // إعادة تعيين إذا عاد للأرض
        if (isGrounded && (isJumping || isFalling))
        {
            animator.ResetTrigger("Jump");
            animator.ResetTrigger("Fall");
            animator.SetBool("IsJumping", false);
            animator.SetBool("IsFalling", false);
        }
    }

    // فحص حالة السقوط
    void CheckFallState()
    {
        if (isWallSliding) // لا سقوط أثناء انزلاق الحائط
        {
            animator.SetBool("IsFalling", false);
            return;
        }

        // إذا كان في الهواء ويتحرك للأسفل
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

    // فحص الهبوط على الأرض
    void CheckLanding()
    {
        // إذا كان يسقط والآن على الأرض
        if (wasFalling && isGrounded)
        {
            PlaySound(landingSound, 0.3f); // صوت الهبوط

            // تحريك أنيميشن الهبوط
            if (animator != null)
            {
                animator.SetTrigger("Land");
                animator.SetBool("IsFalling", false);
                animator.SetBool("IsJumping", false);
            }
        }

        // تحديث حالة السقوط السابقة
        wasFalling = !isGrounded && rb.linearVelocity.y < -0.1f;
    }

    // =============== [PUBLIC METHODS] ===============
    // الحصول على لون اللاعب الحالي
    public PlayerColor GetPlayerColorType()
    {
        return currentColor;
    }

    // هل تأثير اللون مفعل؟
    public bool IsColorActive()
    {
        return isColorActive;
    }

    // =============== [COLLISION DETECTION] ===============
    // عند بدء التصادم
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (IsGroundObject(collision.gameObject))
        {
            isGrounded = true;
        }
    }

    // عند انتهاء التصادم
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (IsGroundObject(collision.gameObject))
        {
            isGrounded = false;
        }
    }

    // فحص إذا كان الجسم على الارض
    bool IsGroundObject(GameObject obj)
    {
        return obj.CompareTag("Ground") || obj.CompareTag("Platform") || obj.CompareTag("ColorBox");
    }

    // =============== [GIZMOS FOR DEBUGGING] ===============
    // رسم أدوات التصحيح في محرر Unity
    private void OnDrawGizmosSelected()
    {
        // رسم دائرة فحص الأرض
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        // رسم أشعة كشف الحائط (منخفضة)
        Gizmos.color = Color.red;
        Vector2 rayOrigin = (Vector2)transform.position + new Vector2(0, 0.2f);
        Gizmos.DrawRay(rayOrigin, Vector2.right * wallCheckDistance);
        Gizmos.DrawRay(rayOrigin, Vector2.left * wallCheckDistance);

        // رسم أشعة كشف الحائط (مرتفعة)
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(rayOrigin + new Vector2(0, 0.5f), Vector2.right * wallCheckDistance);
        Gizmos.DrawRay(rayOrigin + new Vector2(0, 0.5f), Vector2.left * wallCheckDistance);
    }

    // =============== [PROPERTIES FOR OTHER SCRIPTS] ===============
    // خصائص للوصول من السكربتات الأخرى (قراءة فقط)
    public bool IsWallSliding => isWallSliding;
    public bool IsTouchingWall => isTouchingWall;
}