using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Master volume slider, persisted in PlayerPrefs ("MasterVolume", 0..1). Coexists with the
/// existing <see cref="SoundToggle"/>: the effective AudioListener volume is
/// (muted ? 0 : master), and the saved master level is re-applied whenever this loads, so a
/// mute/unmute returns to the chosen slider value rather than full blast. Purely additive.
/// </summary>
public class VolumeControl : MonoBehaviour
{
    public Slider slider;
    public TMP_Text valueLabel;   // optional "80%"

    const string VolKey = "MasterVolume";
    const string MuteKey = "Muted";

    void Awake()
    {
        Apply(GetMaster());
    }

    void Start()
    {
        if (slider != null)
        {
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.SetValueWithoutNotify(GetMaster());
            slider.onValueChanged.AddListener(OnSliderChanged);
        }
        Refresh(GetMaster());
    }

    void OnDestroy()
    {
        if (slider != null) slider.onValueChanged.RemoveListener(OnSliderChanged);
    }

    public void OnSliderChanged(float v)
    {
        PlayerPrefs.SetFloat(VolKey, v);
        // Changing the slider above zero implicitly unmutes.
        if (v > 0f && IsMuted()) PlayerPrefs.SetInt(MuteKey, 0);
        PlayerPrefs.Save();
        Apply(v);
        Refresh(v);
    }

    static float GetMaster() => Mathf.Clamp01(PlayerPrefs.GetFloat(VolKey, 1f));
    static bool IsMuted() => PlayerPrefs.GetInt(MuteKey, 0) == 1;

    static void Apply(float master)
    {
        AudioListener.volume = IsMuted() ? 0f : master;
    }

    void Refresh(float v)
    {
        if (valueLabel != null) valueLabel.text = Mathf.RoundToInt(v * 100f) + "%";
    }
}
