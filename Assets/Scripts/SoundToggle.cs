using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Global sound on/off toggle, persisted in PlayerPrefs. Put on a button (top-right HUD,
// main menu, help panel) and wire its OnClick → Toggle(). Affects ALL audio via the
// AudioListener, and re-applies the saved state in every scene.
public class SoundToggle : MonoBehaviour
{
    [Header("Optional visuals")]
    public TMP_Text label;      // e.g. shows "Sound: On" / "Sound: Off"
    public Image icon;          // e.g. speaker / muted-speaker
    public Sprite onSprite;
    public Sprite offSprite;

    const string Key = "Muted";

    void Awake()  { Apply(IsMuted()); }   // enforce saved state as soon as the scene loads
    void Start()  { Refresh(); }

    public void Toggle()
    {
        bool muted = !IsMuted();
        PlayerPrefs.SetInt(Key, muted ? 1 : 0);
        PlayerPrefs.Save();
        Apply(muted);
        Refresh();
    }

    static bool IsMuted() => PlayerPrefs.GetInt(Key, 0) == 1;

    static void Apply(bool muted) => AudioListener.volume = muted ? 0f : 1f;

    void Refresh()
    {
        bool muted = IsMuted();
        if (label != null) label.text = muted ? "Sound: Off" : "Sound: On";
        if (icon != null && onSprite != null && offSprite != null)
            icon.sprite = muted ? offSprite : onSprite;
    }
}
