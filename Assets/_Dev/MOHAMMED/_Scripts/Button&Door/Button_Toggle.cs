using System;
using UnityEngine;
using UnityEngine.Events;

public class Button_Toggle : MonoBehaviour
{
    public Renderer buttonRenderer;
    public Color pressedColor = Color.red;
    public Color defaultColor = Color.gray;
    
    private int howManyOnButton = 0;

    [Header("Events")]
    public UnityEvent onPressed;
    public UnityEvent onReleased;

    void Start()
    {
        if (buttonRenderer == null)
            buttonRenderer = GetComponent<Renderer>();

        buttonRenderer.material.color = defaultColor;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player") || other.gameObject.CompareTag("Object"))
        {
            howManyOnButton++;
            buttonRenderer.material.color = pressedColor;
            if (howManyOnButton == 1) // only trigger once
                onPressed.Invoke();
        }
    }
    void OnCollisionExit2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player") || other.gameObject.CompareTag("Object"))
        {
            howManyOnButton--;
            if (howManyOnButton <= 0)
            {
                howManyOnButton = 0;
                buttonRenderer.material.color = defaultColor;
                onReleased.Invoke();
            }
        }
    }
    void Update()
    {
        
    }
}
