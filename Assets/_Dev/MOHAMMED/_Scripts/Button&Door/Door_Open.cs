using UnityEngine;

public class Door_Open : MonoBehaviour
{
    public Vector2 openOffset = new Vector2(0, 3); // how far it moves when open
    public float speed = 3f;
    public int activeOnButton = 1;

    private Vector2 closedPosition;
    private Vector2 openPosition;
    private bool isOpen = false;
    private int activeButtons = 0; // tracks how many buttons are pressed
    void Start()
    {
        closedPosition = transform.position;
        openPosition = closedPosition + openOffset;
    }

    void Update()
    {
        Vector3 target = isOpen ? openPosition : closedPosition;
        transform.position = Vector3.Lerp(transform.position, target, Time.deltaTime * speed);
    }
    private void CheckDoorState()
    {
        // Example: require 2 buttons
        if (activeButtons >= activeOnButton)
            isOpen = true;
        else
            isOpen = false;
    }
    
    // Called by button
    public void ButtonPressed()
    {
        activeButtons++;
        CheckDoorState();
    }
    
    // Called by button
    public void ButtonReleased()
    {
        if (activeButtons >= activeOnButton)
        {
            CheckDoorState();
            return;
        }
        else
        {
            activeButtons--;
            if (activeButtons < 0) activeButtons = 0; // safety
            CheckDoorState();
        }

    }
}
