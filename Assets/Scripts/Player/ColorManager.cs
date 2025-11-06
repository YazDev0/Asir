using UnityEngine;

public class ColorManager : MonoBehaviour
{
    public static ColorManager Instance;

    [Header("Game Colors")]
    public Color redColor = Color.red;
    public Color greenColor = Color.green;
    public Color blueColor = Color.blue;

    [Header("UI References")]
    public UnityEngine.UI.Image colorIndicator;
    public TMPro.TextMeshProUGUI colorText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void UpdateColorUI(Color currentColor, string colorName)
    {
        if (colorIndicator != null)
        {
            colorIndicator.color = currentColor;
        }

        if (colorText != null)
        {
            colorText.text = $"Current: {colorName}";
            colorText.color = currentColor;
        }
    }

    public string GetColorName(Color color)
    {
        if (color == redColor) return "RED";
        if (color == greenColor) return "GREEN";
        if (color == blueColor) return "BLUE";
        return "UNKNOWN";
    }
}