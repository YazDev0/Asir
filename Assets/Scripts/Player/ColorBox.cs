using UnityEngine;
using UnityEngine.Tilemaps;

public class ColorBox : MonoBehaviour
{
    [Header("Box Settings")]
    public BoxColor boxColor = BoxColor.Red;
    public bool startActive = false; // ✅ إذا كان true يبدأ مفعل من البداية

    [Header("HDR Glow Settings")]
    [ColorUsage(true, true)]
    public Color hdrGlowColor = Color.red;
    public float glowExposure = 2f;
    public float pulseSpeed = 1.5f;
    public float pulseIntensity = 0.5f;

    [Header("Timer Settings")]
    public float activeDuration = 5f;
    private float activeTimer = 0f;
    private bool isActive = false;
    private bool isDeactivated = false; // ✅ لتتبع إذا كان الفخ معطل

    private SpriteRenderer spriteRenderer;
    private TilemapRenderer tilemapRenderer;
    private Collider2D boxCollider;
    private TilemapCollider2D tilemapCollider;
    private CompositeCollider2D compositeCollider;
    private bool hasGlow = false;
    private bool isSolid = true;

    // مواد الـHDR Glow
    private Material hdrGlowMaterial;
    private Material originalMaterial;

    // ✅ متغير ثابت لتتبع اللون النشط حاليًا
    private static BoxColor currentActiveColor = BoxColor.Red;

    public enum BoxColor
    {
        Red,
        Green
    }

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        tilemapRenderer = GetComponent<TilemapRenderer>();
        boxCollider = GetComponent<Collider2D>();
        tilemapCollider = GetComponent<TilemapCollider2D>();
        compositeCollider = GetComponent<CompositeCollider2D>();

        // ✅ إنشاء مواد الـHDR Glow
        CreateHDRGlowMaterial();

        if (boxCollider == null && tilemapCollider == null && compositeCollider == null)
        {
            boxCollider = gameObject.AddComponent<BoxCollider2D>();
        }

        // ✅ تحديد الحالة الأولية بناء على الإعداد
        if (startActive)
        {
            SetInitialActive();
        }
        else
        {
            SetInitialAppearance();
        }
    }

    void CreateHDRGlowMaterial()
    {
        // ✅ إنشاء شادر يدعم HDR
        hdrGlowMaterial = new Material(Shader.Find("Universal Render Pipeline/2D/Sprite-Lit-Default"));

        if (hdrGlowMaterial == null)
            hdrGlowMaterial = new Material(Shader.Find("Sprites/Default"));

        // ✅ حفظ المادة الأصلية
        if (spriteRenderer != null)
            originalMaterial = spriteRenderer.material;
        else if (tilemapRenderer != null)
            originalMaterial = tilemapRenderer.material;
    }

    void SetInitialAppearance()
    {
        // ✅ تطبيق المظهر الأولي الشفاف
        Color transparentColor = GetNormalColor();
        transparentColor.a = 0.3f;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = transparentColor;
        }
        else if (tilemapRenderer != null && tilemapRenderer.material != null)
        {
            tilemapRenderer.material.color = transparentColor;
        }

        // ✅ تعطيل الكولايدر في البداية
        SetAllCollidersTrigger(true);
        EnableAllColliders(false);
        isSolid = false;
        isActive = false;
        hasGlow = false;
        isDeactivated = false;
    }

    void SetInitialActive()
    {
        // ✅ يبدأ مفعل من البداية
        isActive = true;
        isSolid = true;
        hasGlow = true;
        isDeactivated = false;

        // ✅ تطبيق مادة الـHDR Glow
        if (spriteRenderer != null)
        {
            spriteRenderer.material = hdrGlowMaterial;
        }

        if (tilemapRenderer != null)
        {
            tilemapRenderer.material = hdrGlowMaterial;
        }

        // ✅ الإعدادات الأساسية للـHDR Glow
        SetupHDRGlowProperties();

        // ✅ تفعيل الكولايدر
        SetAllCollidersTrigger(false);
        EnableAllColliders(true);

        // ✅ تطبيق اللون الكامل
        Color solidColor = GetNormalColor();
        solidColor.a = 1f;
        ApplyColorToRenderer(solidColor);
    }

    void Update()
    {
        // ✅ إدارة المؤقت للفخوش المعطلة
        if (startActive && isDeactivated && activeTimer > 0f)
        {
            activeTimer -= Time.deltaTime;
            if (activeTimer <= 0f)
            {
                SetActiveAgain();
            }
        }
        // ✅ إدارة المؤقت للصناديق العادية
        else if (!startActive && isActive && activeTimer > 0f)
        {
            activeTimer -= Time.deltaTime;
            if (activeTimer <= 0f)
            {
                SetInactive();
            }
        }

        // ✅ التموج عندما يكون الـGlow مفعل
        if (hasGlow && hdrGlowMaterial != null)
        {
            ApplyPulseEffect();
        }
    }

    void ApplyPulseEffect()
    {
        // ✅ حساب التموج باستخدام Sine wave
        float pulse = Mathf.Sin(Time.time * pulseSpeed) * pulseIntensity + 1f;

        // ✅ لون HDR مع التموج
        Color pulsedColor = GetHDRColor() * pulse;

        // ✅ تطبيق اللون المتموج
        hdrGlowMaterial.color = pulsedColor;

        // ✅ تطبيق التموج على الـEmission أيضاً
        if (hdrGlowMaterial.HasProperty("_EmissionColor"))
        {
            hdrGlowMaterial.SetColor("_EmissionColor", pulsedColor * glowExposure);
        }
    }

    public void UpdateBoxState(PlayerController player)
    {
        if (!HasCollider()) return;

        PlayerController.PlayerColor playerColor = player.GetPlayerColorType();
        bool isColorActive = player.IsColorActive();

        // ✅ إذا كان startActive = true، يتعطل عندما يكون اللون متطابق
        if (startActive)
        {
            // ✅ الفخ المفعل يصبح غير مفعل عندما يكون اللون متطابق
            if (playerColor.ToString() == boxColor.ToString() && isColorActive)
            {
                DeactivateWithTimer();
            }
        }
        else
        {
            // ✅ السلوك العادي: يتفعل عندما يكون اللون متطابق واللون هو النشط حاليًا
            if (playerColor.ToString() == boxColor.ToString() && isColorActive && boxColor == currentActiveColor)
            {
                ActivateWithTimer();
            }
            // ✅ إذا تغير اللون النشط، نعطل الصندوق تلقائيًا
            else if (isActive && boxColor != currentActiveColor)
            {
                SetInactive();
            }
        }
    }

    // ✅ دالة عامة لتغيير اللون النشط
    public static void ChangeActiveColor(BoxColor newColor)
    {
        if (currentActiveColor != newColor)
        {
            currentActiveColor = newColor;
            Debug.Log("🎨 تغيير اللون النشط إلى: " + newColor);

            // ✅ إعادة تحديث كل الصناديق في المشهد
            UpdateAllColorBoxes();
        }
    }

    // ✅ تحديث كل الصناديق في المشهد
    private static void UpdateAllColorBoxes()
    {
        ColorBox[] allColorBoxes = FindObjectsOfType<ColorBox>();
        PlayerController player = FindObjectOfType<PlayerController>();

        foreach (ColorBox colorBox in allColorBoxes)
        {
            if (colorBox != null && player != null)
            {
                colorBox.UpdateBoxState(player);
            }
        }
    }

    void ActivateWithTimer()
    {
        if (isActive)
        {
            activeTimer = activeDuration;
            return;
        }

        isActive = true;
        activeTimer = activeDuration;

        ApplyHDRGlowEffect();
        MakeSolid();

        Debug.Log("✅ تفعيل صندوق: " + boxColor);
    }

    void DeactivateWithTimer()
    {
        if (!isActive || isDeactivated) return;

        isActive = false;
        isDeactivated = true;
        activeTimer = activeDuration;

        ApplyNormalAppearance();
        MakePassThrough();

        Debug.Log("⏳ الفخ تعطل لمدة " + activeDuration + " ثانية: " + gameObject.name);
    }

    void SetActiveAgain()
    {
        isActive = true;
        isDeactivated = false;
        activeTimer = 0f;

        ApplyHDRGlowEffect();
        MakeSolid();

        Debug.Log("🔄 الفخ عاد للتفعيل: " + gameObject.name);
    }

    void SetInactive()
    {
        if (!isActive) return;

        isActive = false;
        isDeactivated = false;
        activeTimer = 0f;

        ApplyNormalAppearance();
        MakePassThrough();

        Debug.Log("❌ تعطيل صندوق: " + boxColor);
    }

    void ApplyHDRGlowEffect()
    {
        if (hasGlow) return;
        hasGlow = true;

        // ✅ تطبيق مادة الـHDR Glow
        if (spriteRenderer != null)
        {
            spriteRenderer.material = hdrGlowMaterial;
        }

        if (tilemapRenderer != null)
        {
            tilemapRenderer.material = hdrGlowMaterial;
        }

        // ✅ الإعدادات الأساسية للـHDR Glow
        SetupHDRGlowProperties();

        EnableAllColliders(true);
    }

    void SetupHDRGlowProperties()
    {
        if (hdrGlowMaterial != null)
        {
            Color hdrColor = GetHDRColor();

            // ✅ تطبيق اللون الأساسي
            hdrGlowMaterial.color = hdrColor;

            // ✅ إضافة Emission للـHDR Glow
            if (hdrGlowMaterial.HasProperty("_EmissionColor"))
            {
                hdrGlowMaterial.SetColor("_EmissionColor", hdrColor * glowExposure);
                hdrGlowMaterial.EnableKeyword("_EMISSION");
            }

            // ✅ إعدادات إضافية للـHDR
            hdrGlowMaterial.SetFloat("_Metallic", 0f);
            hdrGlowMaterial.SetFloat("_Glossiness", 0.8f);
        }
    }

    Color GetHDRColor()
    {
        // ✅ ألوان HDR مكثفة
        switch (boxColor)
        {
            case BoxColor.Red:
                return new Color(2f, 0.5f, 0.5f, 1f);
            case BoxColor.Green:
                return new Color(0.5f, 2f, 0.5f, 1f);
            default:
                return Color.white;
        }
    }

    void ApplyNormalAppearance()
    {
        if (!hasGlow) return;
        hasGlow = false;

        // ✅ العودة للمادة الأصلية
        if (spriteRenderer != null && originalMaterial != null)
        {
            spriteRenderer.material = originalMaterial;
        }

        if (tilemapRenderer != null && originalMaterial != null)
        {
            tilemapRenderer.material = originalMaterial;
        }

        // ✅ تطبيق اللون الشفاف
        Color transparentColor = GetNormalColor();
        transparentColor.a = 0.3f;
        ApplyColorToRenderer(transparentColor);
    }

    void MakeSolid()
    {
        if (isSolid) return;

        isSolid = true;
        SetAllCollidersTrigger(false);
        EnableAllColliders(true);

        Color solidColor = GetNormalColor();
        solidColor.a = 1f;
        ApplyColorToRenderer(solidColor);
    }

    void MakePassThrough()
    {
        if (!isSolid) return;

        isSolid = false;
        SetAllCollidersTrigger(true);
        EnableAllColliders(false);

        Color transparentColor = GetNormalColor();
        transparentColor.a = 0.3f;
        ApplyColorToRenderer(transparentColor);
    }

    void ApplyColorToRenderer(Color color)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = color;
        }

        if (tilemapRenderer != null && tilemapRenderer.material != null)
        {
            tilemapRenderer.material.color = color;
        }
    }

    Color GetNormalColor()
    {
        switch (boxColor)
        {
            case BoxColor.Red: return Color.red;
            case BoxColor.Green: return Color.green;
            default: return Color.white;
        }
    }

    bool HasCollider()
    {
        return boxCollider != null || tilemapCollider != null || compositeCollider != null;
    }

    void EnableAllColliders(bool enabled)
    {
        if (boxCollider != null) boxCollider.enabled = enabled;
        if (tilemapCollider != null) tilemapCollider.enabled = enabled;
        if (compositeCollider != null) compositeCollider.enabled = enabled;
    }

    void SetAllCollidersTrigger(bool isTrigger)
    {
        if (boxCollider != null) boxCollider.isTrigger = isTrigger;
        if (tilemapCollider != null) tilemapCollider.isTrigger = isTrigger;
        if (compositeCollider != null) compositeCollider.isTrigger = isTrigger;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            if (player != null) UpdateBoxState(player);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null) UpdateBoxState(player);
        }
    }

    void OnEnable()
    {
        if (startActive)
        {
            SetInitialActive();
        }
        else
        {
            SetInitialAppearance();
        }
    }

    void OnDestroy()
    {
        if (hdrGlowMaterial != null)
            DestroyImmediate(hdrGlowMaterial);
    }
}