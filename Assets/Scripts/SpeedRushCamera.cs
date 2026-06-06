using UnityEngine;

// Subtle orthographic zoom on Speed Rush for a speed/whoosh sensation. Captures the
// scene's base size on Start and lerps to a slightly wider size while active, then back.
// Only touches Camera.orthographicSize, so it coexists with CameraShake (which moves
// the transform) without conflict.
[RequireComponent(typeof(Camera))]
public class SpeedRushCamera : MonoBehaviour
{
    [Header("Tuning")]
    public float zoomDelta = 0.6f;   // how much wider the view gets during Speed Rush
    public float lerpSpeed = 4f;

    private Camera cam;
    private float baseSize;

    void Awake()
    {
        cam = GetComponent<Camera>();
    }

    void Start()
    {
        baseSize = cam.orthographicSize;
    }

    void LateUpdate()
    {
        bool active = GameManager.Instance != null && GameManager.Instance.SpeedRushActive;
        float target = active ? baseSize + zoomDelta : baseSize;
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, target, lerpSpeed * Time.unscaledDeltaTime);
    }
}
