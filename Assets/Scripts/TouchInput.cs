using UnityEngine;

// Mobile touch input bridge. Tap = jump, swipe down (held) = crouch.
// Drives the existing PlayerController via its public TryJump() / SetCrouchHeld() hooks,
// so it never duplicates movement logic. Also serves as the start/restart tap on the
// idle and game-over screens (where a tap maps to the Space key the GameManager listens
// for — here we just call StartSequence/Restart paths indirectly via simulated jump on
// the player only during play; for start/restart the GameManager reads keyboard, so we
// keep this focused on in-run jump/crouch which is the requested behaviour).
//
// Uses the legacy Input.touches API (matches the project's legacy Input usage and works
// with the Input System's backend in "Both" mode).
public class TouchInput : MonoBehaviour
{
    [Header("Refs")]
    public PlayerController player;          // auto-found from GameManager if left empty

    [Header("Swipe tuning")]
    public float swipeDownThreshold = 60f;   // pixels of downward travel to trigger crouch
    public float tapMaxMovement = 40f;       // finger travel under this (and not a down-swipe) = tap

    private int activeFingerId = -1;
    private Vector2 startPos;
    private bool crouchActive;
    private bool swipedThisTouch;

    void Update()
    {
        if (player == null)
        {
            if (GameManager.Instance != null) player = GameManager.Instance.player;
            if (player == null) return;
        }

        if (Input.touchCount == 0)
        {
            if (crouchActive) { crouchActive = false; player.SetCrouchHeld(false); }
            activeFingerId = -1;
            return;
        }

        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch t = Input.GetTouch(i);

            if (t.phase == TouchPhase.Began && activeFingerId == -1)
            {
                activeFingerId = t.fingerId;
                startPos = t.position;
                swipedThisTouch = false;
            }
            else if (t.fingerId == activeFingerId)
            {
                Vector2 delta = t.position - startPos;

                // swipe down → crouch hold
                if (!swipedThisTouch && delta.y <= -swipeDownThreshold &&
                    Mathf.Abs(delta.y) > Mathf.Abs(delta.x))
                {
                    swipedThisTouch = true;
                    crouchActive = true;
                    player.SetCrouchHeld(true);
                }

                if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
                {
                    // tap (no significant movement, no down-swipe) → jump
                    if (!swipedThisTouch && delta.magnitude <= tapMaxMovement)
                        player.TryJump();

                    if (crouchActive) { crouchActive = false; player.SetCrouchHeld(false); }
                    activeFingerId = -1;
                }
            }
        }
    }
}
