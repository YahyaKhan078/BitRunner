using UnityEngine;

/// <summary>
/// Spinning saw hazard. Rotates for visual menace and optionally bobs up and down. Lethality
/// and horizontal scroll come from <see cref="ObstacleMover"/> (killOnContact = true), so this
/// only adds motion flavor. Additive — no existing scripts touched.
/// </summary>
public class MovingSaw : MonoBehaviour
{
    public float spinSpeed = 360f;     // degrees/sec
    public float bobAmplitude = 0f;    // 0 = no vertical bob
    public float bobSpeed = 2f;

    float baseY;
    float t;

    void OnEnable()
    {
        baseY = transform.position.y;
        t = 0f;
    }

    void Update()
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsPlaying) return;

        transform.Rotate(0f, 0f, -spinSpeed * Time.deltaTime);

        if (bobAmplitude > 0f)
        {
            t += Time.deltaTime * bobSpeed;
            var p = transform.position;
            p.y = baseY + Mathf.Sin(t) * bobAmplitude;
            transform.position = p;
        }
    }
}
