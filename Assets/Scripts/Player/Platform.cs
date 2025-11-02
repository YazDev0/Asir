using UnityEngine;

public class ColorBox : MonoBehaviour
{
    [Header("Box Settings")]
    public BoxColor boxColor = BoxColor.Red;

    [Header("Bloom Effect")]
    public Material bloomMaterial;
    public Material normalMaterial;

    private SpriteRenderer spriteRenderer;
    private Collider2D boxCollider;
    private bool hasBloom = false;
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
        ApplyNormalAppearance();
        MakeSolid();
    }

    public void UpdateBoxState(PlayerController player)
    {
        PlayerController.PlayerColor playerColor = player.GetPlayerColorType();

        Debug.Log($"🎨 Player: {playerColor} | Box: {boxColor}");

        // ✅ إذا كان لون اللاعب يطابق لون الصندوق - بلوم + صلب
        if (playerColor == PlayerController.PlayerColor.Red && boxColor == BoxColor.Red)
        {
            ApplyBloomEffect();
            MakeSolid();
            Debug.Log("🔴 Red player on RED box - SOLID + BLOOM");
        }
        else if (playerColor == PlayerController.PlayerColor.Green && boxColor == BoxColor.Green)
        {
            ApplyBloomEffect();
            MakeSolid();
            Debug.Log("🟢 Green player on GREEN box - SOLID + BLOOM");
        }
        else if (playerColor == PlayerController.PlayerColor.Blue && boxColor == BoxColor.Blue)
        {
            ApplyBloomEffect();
            MakeSolid();
            Debug.Log("🔵 Blue player on BLUE box - SOLID + BLOOM");
        }
        // ❌ إذا كانت الألوان مختلفة - لا بلوم + اختراق
        else
        {
            ApplyNormalAppearance();
            MakePassThrough();
            Debug.Log($"🚫 Color mismatch - PASS THROUGH");
        }
    }

    void ApplyBloomEffect()
    {
        if (hasBloom) return;

        hasBloom = true;

        // تطبيق مادة البلوم إذا موجودة
        if (bloomMaterial != null)
        {
            spriteRenderer.material = bloomMaterial;
        }

        // جعل اللون مضيء
        spriteRenderer.color = GetBoxColor() * 1.8f;
    }

    void ApplyNormalAppearance()
    {
        if (!hasBloom) return;

        hasBloom = false;

        // العودة للمادة العادية
        if (normalMaterial != null)
        {
            spriteRenderer.material = normalMaterial;
        }

        // العودة للون الطبيعي
        spriteRenderer.color = GetBoxColor();
    }

    void MakeSolid()
    {
        if (isSolid) return;

        isSolid = true;
        boxCollider.isTrigger = false;

        // إعادة اللون الكامل
        spriteRenderer.color = GetBoxColor();

        Debug.Log("🛑 Box is now SOLID");
    }

    void MakePassThrough()
    {
        if (!isSolid) return;

        isSolid = false;
        boxCollider.isTrigger = true;

        // جعل الصندوق شفاف للإشارة أنه قابل للاختراق
        Color transparentColor = GetBoxColor();
        transparentColor.a = 0.3f;
        spriteRenderer.color = transparentColor;

        Debug.Log("👻 Box is now PASS-THROUGH");
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
            if (player != null)
            {
                UpdateBoxState(player);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                UpdateBoxState(player);
            }
        }
    }

    // ✅ رسم معلومات التصحيح
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        string status = isSolid ? "SOLID" : "PASS-THROUGH";
        string bloom = hasBloom ? "BLOOM" : "NORMAL";

        GUIStyle style = new GUIStyle();
        style.normal.textColor = isSolid ? Color.green : Color.red;

        // UnityEditor.Handles.Label(transform.position + Vector3.up * 1f, $"{boxColor}\n{status}\n{bloom}", style);
    }
}