using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    [Header("Audio Settings")]
    public AudioMixer audioMixer;
    public string masterVolumeParameter = "MasterVolume";

    [Header("UI References")]
    public Toggle soundToggle;
    public Slider volumeSlider;
    public Button increaseVolumeBtn;
    public Button decreaseVolumeBtn;

    private bool isSoundEnabled = true;
    private float currentVolume = 1f;

    void Start()
    {
        // ÊÍãíá ÇáÅÚÏÇÏÇÊ ÇáãÍİæÙÉ
        LoadAudioSettings();

        // ÅÚÏÇÏ æÇÌåÉ ÇáãÓÊÎÏã
        SetupUI();
    }

    void LoadAudioSettings()
    {
        // ÊÍãíá ÇáÅÚÏÇÏÇÊ ãä PlayerPrefs
        isSoundEnabled = PlayerPrefs.GetInt("SoundEnabled", 1) == 1;
        currentVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);

        ApplyAudioSettings();
    }

    void SaveAudioSettings()
    {
        PlayerPrefs.SetInt("SoundEnabled", isSoundEnabled ? 1 : 0);
        PlayerPrefs.SetFloat("MasterVolume", currentVolume);
        PlayerPrefs.Save();
    }

    void ApplyAudioSettings()
    {
        // ÊØÈíŞ ÇáÅÚÏÇÏÇÊ Úáì AudioMixer
        if (audioMixer != null)
        {
            float volume = isSoundEnabled ? Mathf.Log10(currentVolume) * 20 : -80f;
            audioMixer.SetFloat(masterVolumeParameter, volume);
        }

        // ÊØÈíŞ Úáì ÌãíÚ ÇáÓÈÇíß
        SawMovement[] saws = FindObjectsOfType<SawMovement>();
        foreach (SawMovement saw in saws)
        {
            saw.SetSoundVolume(currentVolume);
            if (!isSoundEnabled)
            {
                saw.ToggleSound();
            }
        }
    }

    void SetupUI()
    {
        if (soundToggle != null)
        {
            soundToggle.isOn = isSoundEnabled;
            soundToggle.onValueChanged.AddListener(ToggleSound);
        }

        if (volumeSlider != null)
        {
            volumeSlider.value = currentVolume;
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }

        if (increaseVolumeBtn != null)
            increaseVolumeBtn.onClick.AddListener(IncreaseVolume);

        if (decreaseVolumeBtn != null)
            decreaseVolumeBtn.onClick.AddListener(DecreaseVolume);
    }

    // ÏæÇá ÇáÊÍßã ÇáÚÇãÉ
    public void ToggleSound(bool enabled)
    {
        isSoundEnabled = enabled;
        ApplyAudioSettings();
        SaveAudioSettings();
    }

    public void ToggleSound()
    {
        isSoundEnabled = !isSoundEnabled;
        ApplyAudioSettings();
        SaveAudioSettings();
    }

    public void SetVolume(float volume)
    {
        currentVolume = Mathf.Clamp01(volume);
        ApplyAudioSettings();
        SaveAudioSettings();
    }

    public void IncreaseVolume()
    {
        currentVolume = Mathf.Clamp01(currentVolume + 0.1f);
        if (volumeSlider != null) volumeSlider.value = currentVolume;
        ApplyAudioSettings();
        SaveAudioSettings();
    }

    public void DecreaseVolume()
    {
        currentVolume = Mathf.Clamp01(currentVolume - 0.1f);
        if (volumeSlider != null) volumeSlider.value = currentVolume;
        ApplyAudioSettings();
        SaveAudioSettings();
    }

    // ááÍÕæá Úáì ÇáÅÚÏÇÏÇÊ ãä ÓßÑÈÊÇÊ ÃÎÑì
    public bool IsSoundEnabled() => isSoundEnabled;
    public float GetCurrentVolume() => currentVolume;
}