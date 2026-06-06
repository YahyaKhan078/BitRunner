using UnityEngine;
using TMPro;

// Smoothed FPS readout for the top-right technical HUD.
// Put on (or point `label` at) a TMP text. Uses unscaled time, so it reports true
// frame rate even while the game is frozen (timeScale = 0) on the game-over screen.
public class FPSCounter : MonoBehaviour
{
    public TMP_Text label;
    public string prefix = "FPS ";
    public float refresh = 0.25f;     // seconds between text updates

    private float smoothedDt;
    private float timer;

    void Awake()
    {
        if (label == null) label = GetComponent<TMP_Text>();
        smoothedDt = Time.unscaledDeltaTime;
    }

    void Update()
    {
        // exponential moving average of frame time → stable number
        smoothedDt = Mathf.Lerp(smoothedDt, Time.unscaledDeltaTime, 0.1f);

        timer += Time.unscaledDeltaTime;
        if (timer >= refresh && label != null)
        {
            timer = 0f;
            int fps = smoothedDt > 0f ? Mathf.RoundToInt(1f / smoothedDt) : 0;
            label.text = prefix + fps;
        }
    }
}
