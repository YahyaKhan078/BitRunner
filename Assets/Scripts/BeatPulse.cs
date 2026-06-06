using UnityEngine;

/// <summary>
/// Subtle audio-reactive scale pulse (Geometry-Dash signature). Pulses on a fixed BPM while a
/// run is active — apply to a background layer or HUD accent. Lightweight and additive; uses a
/// configurable BPM rather than FFT so it stays cheap and predictable.
/// </summary>
public class BeatPulse : MonoBehaviour
{
    public float bpm = 128f;
    public float pulseScale = 0.06f;   // fraction added at the beat
    public bool onlyWhilePlaying = true;

    Vector3 baseScale;
    float beatInterval;
    float timer;

    void OnEnable()
    {
        baseScale = transform.localScale;
        beatInterval = 60f / Mathf.Max(1f, bpm);
        timer = 0f;
    }

    void Update()
    {
        if (onlyWhilePlaying && (GameManager.Instance == null || !GameManager.Instance.IsPlaying))
        {
            transform.localScale = Vector3.Lerp(transform.localScale, baseScale, 4f * Time.deltaTime);
            return;
        }

        timer += Time.deltaTime;
        float phase = (timer % beatInterval) / beatInterval;   // 0..1 within a beat
        // sharp attack on the beat, decay across it
        float pulse = Mathf.Pow(1f - phase, 3f);
        transform.localScale = baseScale * (1f + pulse * pulseScale);
    }
}
