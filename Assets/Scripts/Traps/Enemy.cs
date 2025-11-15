using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [Header("Movement Points")]
    public Transform leftPoint;
    public Transform rightPoint;

    [Header("Movement Settings")]
    public float moveSpeed = 2f;
    public bool startFromRight = false;

    private bool movingRight;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        movingRight = !startFromRight; // تحديد نقطة البداية

        if (leftPoint == null || rightPoint == null)
        {
            Debug.LogError("❌ Please assign left and right points!");
            return;
        }

        // البدء من النقطة المحددة
        transform.position = startFromRight ? rightPoint.position : leftPoint.position;
    }

    void Update()
    {
        if (leftPoint != null && rightPoint != null)
        {
            MoveEnemy();
        }
    }

    void MoveEnemy()
    {
        Vector3 targetPosition = movingRight ? rightPoint.position : leftPoint.position;
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        FlipEnemy();

        if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
        {
            movingRight = !movingRight;
        }
    }

    void FlipEnemy()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = !movingRight;
        }
        else
        {
            Vector3 scale = transform.localScale;
            scale.x = movingRight ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
    }

    void OnDrawGizmos()
    {
        if (leftPoint != null && rightPoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(leftPoint.position, 0.3f);
            Gizmos.DrawWireSphere(rightPoint.position, 0.3f);
            Gizmos.DrawLine(leftPoint.position, rightPoint.position);

            // إظهار الاتجاه الحالي
            Gizmos.color = movingRight ? Color.green : Color.yellow;
            Gizmos.DrawLine(transform.position, movingRight ? rightPoint.position : leftPoint.position);
        }
    }
}