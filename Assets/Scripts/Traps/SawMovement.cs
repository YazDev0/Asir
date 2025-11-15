using UnityEngine;
using UnityEngine.Audio;

public class SawMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public MovementType movementType = MovementType.DistanceBased;
    public float moveDistance = 3f;
    public float moveSpeed = 2f;
    public bool continuousMovement = true;
    public MovementDirection movementDirection = MovementDirection.Horizontal;
    public StartPosition startFrom = StartPosition.CurrentPosition; // نقطة البداية

    [Header("Point Based Movement")]
    public Transform pointA;
    public Transform pointB;

    [Header("Sound Settings")]
    public AudioClip movementSound;
    public AudioClip collisionSound;
    public float soundVolume = 1f;
    public bool soundEnabled = true;

    private Vector3 startPosition;
    private Vector3 positionA;
    private Vector3 positionB;
    private bool movingToB = true;
    private AudioSource audioSource;

    public enum MovementDirection
    {
        Horizontal,
        Vertical
    }

    public enum MovementType
    {
        DistanceBased,
        PointBased
    }

    public enum StartPosition
    {
        CurrentPosition,    // تبدأ من مكانها الحالي
        PointA,            // تبدأ من النقطة A
        PointB,            // تبدأ من النقطة B
        Middle             // تبدأ من المنتصف
    }

    void Start()
    {
        InitializeStartPosition();
        CalculatePositions();

        // إضافة AudioSource تلقائياً
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.volume = soundVolume;
        audioSource.loop = true;

        if (movementSound != null && soundEnabled)
        {
            audioSource.clip = movementSound;
            audioSource.Play();
        }
    }

    void InitializeStartPosition()
    {
        if (movementType == MovementType.DistanceBased)
        {
            // للحركة بالمسافة
            switch (startFrom)
            {
                case StartPosition.CurrentPosition:
                    startPosition = transform.position;
                    break;
                case StartPosition.PointA:
                    startPosition = transform.position;
                    transform.position = positionA;
                    break;
                case StartPosition.PointB:
                    startPosition = transform.position;
                    transform.position = positionB;
                    break;
                case StartPosition.Middle:
                    startPosition = transform.position;
                    // للمسافة بيكون المنتصف هو startPosition نفسه
                    break;
            }
        }
        else
        {
            // للحركة بالنقاط
            if (pointA != null && pointB != null)
            {
                switch (startFrom)
                {
                    case StartPosition.CurrentPosition:
                        startPosition = transform.position;
                        break;
                    case StartPosition.PointA:
                        transform.position = pointA.position;
                        startPosition = transform.position;
                        movingToB = true;
                        break;
                    case StartPosition.PointB:
                        transform.position = pointB.position;
                        startPosition = transform.position;
                        movingToB = false;
                        break;
                    case StartPosition.Middle:
                        transform.position = (pointA.position + pointB.position) / 2f;
                        startPosition = transform.position;
                        movingToB = true;
                        break;
                }
            }
            else
            {
                startPosition = transform.position;
            }
        }
    }

    void Update()
    {
        if (movementType == MovementType.PointBased)
        {
            if (pointA == null || pointB == null)
            {
                Debug.LogError("❌ Please assign Point A and Point B for point-based movement!");
                return;
            }

            positionA = pointA.position;
            positionB = pointB.position;
        }

        if (continuousMovement)
        {
            MoveContinuously();
        }
        else
        {
            MoveBetweenPoints();
        }

        UpdateSoundSettings();
    }

    void CalculatePositions()
    {
        if (movementType == MovementType.DistanceBased)
        {
            if (movementDirection == MovementDirection.Horizontal)
            {
                positionA = startPosition + Vector3.left * moveDistance;
                positionB = startPosition + Vector3.right * moveDistance;
            }
            else
            {
                positionA = startPosition + Vector3.down * moveDistance;
                positionB = startPosition + Vector3.up * moveDistance;
            }
        }
    }

    void MoveContinuously()
    {
        if (movementType == MovementType.DistanceBased)
        {
            float movement = Mathf.PingPong(Time.time * moveSpeed, moveDistance * 2) - moveDistance;

            if (movementDirection == MovementDirection.Horizontal)
            {
                transform.position = startPosition + Vector3.right * movement;
            }
            else
            {
                transform.position = startPosition + Vector3.up * movement;
            }
        }
        else
        {
            float t = Mathf.PingPong(Time.time * moveSpeed, 1f);
            transform.position = Vector3.Lerp(positionA, positionB, t);
        }
    }

    void MoveBetweenPoints()
    {
        Vector3 targetPosition = movingToB ? positionB : positionA;
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
        {
            movingToB = !movingToB;
            PlayCollisionSound();
        }
    }

    void UpdateSoundSettings()
    {
        if (audioSource != null)
        {
            audioSource.volume = soundEnabled ? soundVolume : 0f;
        }
    }

    void PlayCollisionSound()
    {
        if (collisionSound != null && soundEnabled)
        {
            AudioSource.PlayClipAtPoint(collisionSound, transform.position, soundVolume);
        }
    }

    // دوال التحكم بالصوت
    public void ToggleSound()
    {
        soundEnabled = !soundEnabled;
        UpdateSoundSettings();
    }

    public void SetSoundVolume(float volume)
    {
        soundVolume = Mathf.Clamp01(volume);
        UpdateSoundSettings();
    }

    public void IncreaseVolume()
    {
        soundVolume = Mathf.Clamp01(soundVolume + 0.1f);
        UpdateSoundSettings();
    }

    public void DecreaseVolume()
    {
        soundVolume = Mathf.Clamp01(soundVolume - 0.1f);
        UpdateSoundSettings();
    }

    public void ChangeMovementDirection(MovementDirection newDirection)
    {
        movementDirection = newDirection;
        startPosition = transform.position;
        CalculatePositions();
        movingToB = true;
    }

    public void ChangeMovementType(MovementType newType)
    {
        movementType = newType;
        if (movementType == MovementType.DistanceBased)
        {
            startPosition = transform.position;
            CalculatePositions();
        }
        movingToB = true;
    }

    public void ChangeStartPosition(StartPosition newStartPosition)
    {
        startFrom = newStartPosition;
        InitializeStartPosition();
        CalculatePositions();
    }

    public void ResetToStartPosition()
    {
        InitializeStartPosition();
        movingToB = true;
        CalculatePositions();
    }

    void OnDrawGizmosSelected()
    {
        if (movementType == MovementType.DistanceBased)
        {
            if (Application.isPlaying)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(positionA, 0.2f);
                Gizmos.DrawWireSphere(positionB, 0.2f);
                Gizmos.DrawLine(positionA, positionB);
            }
            else
            {
                Gizmos.color = Color.red;
                Vector3 currentPos = transform.position;

                if (movementDirection == MovementDirection.Horizontal)
                {
                    Gizmos.DrawWireSphere(currentPos + Vector3.left * moveDistance, 0.2f);
                    Gizmos.DrawWireSphere(currentPos + Vector3.right * moveDistance, 0.2f);
                    Gizmos.DrawLine(currentPos + Vector3.left * moveDistance, currentPos + Vector3.right * moveDistance);
                }
                else
                {
                    Gizmos.DrawWireSphere(currentPos + Vector3.down * moveDistance, 0.2f);
                    Gizmos.DrawWireSphere(currentPos + Vector3.up * moveDistance, 0.2f);
                    Gizmos.DrawLine(currentPos + Vector3.down * moveDistance, currentPos + Vector3.up * moveDistance);
                }
            }
        }
        else
        {
            Gizmos.color = Color.blue;
            if (pointA != null)
            {
                Gizmos.DrawWireSphere(pointA.position, 0.3f);
            }
            if (pointB != null)
            {
                Gizmos.DrawWireSphere(pointB.position, 0.3f);
            }
            if (pointA != null && pointB != null)
            {
                Gizmos.DrawLine(pointA.position, pointB.position);

                // إظهار نقطة البداية المختارة
                Gizmos.color = Color.green;
                Vector3 startPos = Vector3.zero;
                switch (startFrom)
                {
                    case StartPosition.CurrentPosition:
                        startPos = transform.position;
                        break;
                    case StartPosition.PointA:
                        startPos = pointA.position;
                        break;
                    case StartPosition.PointB:
                        startPos = pointB.position;
                        break;
                    case StartPosition.Middle:
                        startPos = (pointA.position + pointB.position) / 2f;
                        break;
                }
                Gizmos.DrawWireCube(startPos, Vector3.one * 0.4f);
            }
        }
    }
}