using UnityEngine;

// Cycles a SpriteRenderer through a list of frames. Use it for the pickup sparkle
// (assign the 6 sparkle-effect sub-sprites, loop = false, destroyOnFinish = true),
// for pulsing neon hazards (laser beams), or any simple frame animation.
[RequireComponent(typeof(SpriteRenderer))]
public class SpriteFlipbook : MonoBehaviour
{
    public Sprite[] frames;
    public float fps = 12f;
    public bool loop = true;
    public bool destroyOnFinish = false;   // one-shot effects when loop = false

    private SpriteRenderer sr;
    private float timer;
    private int index;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (frames != null && frames.Length > 0) sr.sprite = frames[0];
    }

    void Update()
    {
        if (frames == null || frames.Length == 0 || fps <= 0f) return;

        timer += Time.deltaTime;
        if (timer < 1f / fps) return;
        timer = 0f;

        index++;
        if (index >= frames.Length)
        {
            if (loop)
            {
                index = 0;
            }
            else
            {
                if (destroyOnFinish) { Destroy(gameObject); return; }
                enabled = false;        // hold last frame
                return;
            }
        }
        sr.sprite = frames[index];
    }
}
