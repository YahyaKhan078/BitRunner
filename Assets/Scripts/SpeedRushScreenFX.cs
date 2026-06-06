using UnityEngine;
using UnityEngine.Rendering;

// Fades a dedicated post-processing Volume (chromatic aberration / vignette / lens
// distortion "speed lines" feel) in and out with GameManager.SpeedRushActive.
// Drives only its own Volume's weight, so the base GlobalVolume look is untouched.
public class SpeedRushScreenFX : MonoBehaviour
{
    [Header("Refs")]
    public Volume rushVolume;        // a global Volume whose profile has the rush overrides

    [Header("Tuning")]
    public float fadeSpeed = 6f;     // how fast the effect ramps in/out

    void Reset()
    {
        rushVolume = GetComponent<Volume>();
    }

    void Awake()
    {
        if (rushVolume == null) rushVolume = GetComponent<Volume>();
        if (rushVolume != null) rushVolume.weight = 0f;
    }

    void Update()
    {
        if (rushVolume == null) return;
        bool active = GameManager.Instance != null && GameManager.Instance.SpeedRushActive;
        float target = active ? 1f : 0f;
        // unscaled so it keeps animating regardless of any time freeze
        rushVolume.weight = Mathf.MoveTowards(rushVolume.weight, target, fadeSpeed * Time.unscaledDeltaTime);
    }
}
