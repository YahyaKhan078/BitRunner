using UnityEngine;

// One-shot visual effect helper (used by the pickup sparkle): grows and fades a
// SpriteRenderer over `lifetime`, then destroys the GameObject.
public class AutoDestroy : MonoBehaviour
{
    public float lifetime = 0.45f;
    public float growBy   = 0.8f;    // extra scale added across the lifetime
    public bool  fadeOut  = true;

    private float age;
    private Vector3 startScale;
    private SpriteRenderer sr;
    private Color startColor;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        startScale = transform.localScale;
        if (sr != null) startColor = sr.color;
    }

    void Update()
    {
        age += Time.deltaTime;
        float t = lifetime > 0f ? Mathf.Clamp01(age / lifetime) : 1f;

        transform.localScale = startScale * (1f + growBy * t);

        if (fadeOut && sr != null)
        {
            Color c = startColor;
            c.a = startColor.a * (1f - t);
            sr.color = c;
        }

        if (age >= lifetime)
            Destroy(gameObject);
    }
}
