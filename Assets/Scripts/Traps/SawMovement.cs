using UnityEngine;
using UnityEngine.Audio;

public class SawMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveDistance = 3f;
    public float moveSpeed = 2f;
    public bool continuousMovement = true;
    public MovementDirection movementDirection = MovementDirection.Horizontal;

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

    void Start()
    {
        startPosition = transform.position;
        CalculatePositions();

        // ≈÷«›… AudioSource  ·ﬁ«∆Ì«
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.volume = soundVolume;
        audioSource.loop = true;

        //  ‘€Ì· ’Ê  «·Õ—ﬂ… ≈–« ﬂ«‰ „ Ê›—«
        if (movementSound != null && soundEnabled)
        {
            audioSource.clip = movementSound;
            audioSource.Play();
        }
    }

    void Update()
    {
        if (continuousMovement)
        {
            MoveContinuously();
        }
        else
        {
            MoveBetweenPoints();
        }

        //  ÕœÌÀ ≈⁄œ«œ«  «·’Ê 
        UpdateSoundSettings();
    }

    void CalculatePositions()
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

    void MoveContinuously()
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

    void MoveBetweenPoints()
    {
        Vector3 targetPosition = movingToB ? positionB : positionA;
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
        {
            movingToB = !movingToB;
            //  ‘€Ì· ’Ê  ⁄‰œ «·«’ÿœ«„ ( €ÌÌ— «·« Ã«Â)
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

    // œÊ«· «· Õﬂ„ »«·’Ê 
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

    public void ResetToStartPosition()
    {
        transform.position = startPosition;
        movingToB = true;
        CalculatePositions();
    }

    void OnDrawGizmosSelected()
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
}