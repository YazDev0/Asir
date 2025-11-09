using UnityEngine;

public class ColorBox : MonoBehaviour
{
    [Header("Box Settings")]
    public BoxColor boxColor = BoxColor.Red;

    private SpriteRenderer spriteRenderer;
    private Collider2D boxCollider;
    private bool hasGlow = false;
    private bool isSolid = true;

    public enum BoxColor
    {
        Red,
        Green,
        Blue
    }

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<Collider2D>();

        if (boxCollider == null)
        {
            Debug.LogWarning($"⚠️ No Collider2D found on {gameObject.name}. Adding BoxCollider2D.");
            boxCollider = gameObject.AddComponent<BoxCollider2D>();
        }

        // ✅ تحديث الحالة مباشرة عند البدء
        UpdateBoxStateImmediately();
    }

    void UpdateBoxStateImmediately()
    {
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            UpdateBoxState(player);
        }
        else
        {
            // ✅ إذا ما فيه لاعب، خلي الصندوق شفاف
            ApplyNormalAppearance();
            MakePassThrough();
        }
    }

    public void UpdateBoxState(PlayerController player)
    {
        if (boxCollider == null) return;

        PlayerController.PlayerColor playerColor = player.GetPlayerColorType();

        Debug.Log($"🎨 Player: {playerColor} | Box: {boxColor}");

        // ✅ إذا كان لون اللاعب يطابق لون الصندوق - جلوه + صلب
        if (playerColor.ToString() == boxColor.ToString())
        {
            ApplyGlowEffect();
            MakeSolid();
            Debug.Log($"🎯 Color match - SOLID + GLOW");
        }
        // ❌ إذا كانت الألوان مختلفة - لا جلوه + اختراق
        else
        {
            ApplyNormalAppearance();
            MakePassThrough();
            Debug.Log($"🚫 Color mismatch - PASS THROUGH");
        }
    }

    void ApplyGlowEffect()
    {
        if (hasGlow) return;
        hasGlow = true;

        // جعل اللون مضيء (جلوه)
        Color glowColor = GetBoxColor() * 2f;
        spriteRenderer.color = glowColor;

        // ✅ إظهار الكولايدر عندما يكون هناك تطابق ألوان
        if (boxCollider != null)
        {
            boxCollider.enabled = true;
        }
    }

    void ApplyNormalAppearance()
    {
        if (!hasGlow) return;
        hasGlow = false;

        // العودة للون الطبيعي
        spriteRenderer.color = GetBoxColor();
    }

    void MakeSolid()
    {
        if (isSolid || boxCollider == null) return;

        isSolid = true;
        boxCollider.isTrigger = false;
        boxCollider.enabled = true;
    }

    void MakePassThrough()
    {
        if (!isSolid || boxCollider == null) return;

        isSolid = false;
        boxCollider.isTrigger = true;
        boxCollider.enabled = false;

        // جعل الصندوق شفاف
        Color transparentColor = GetBoxColor();
        transparentColor.a = 0.3f;
        spriteRenderer.color = transparentColor;
    }

    Color GetBoxColor()
    {
        switch (boxColor)
        {
            case BoxColor.Red: return Color.red;
            case BoxColor.Green: return Color.green;
            case BoxColor.Blue: return Color.blue;
            default: return Color.white;
        }
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

    // ✅ تحديث الحالة عندما يصبح الكائن نشط في السين
    void OnEnable()
    {
        UpdateBoxStateImmediately();
    }
}