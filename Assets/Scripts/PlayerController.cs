using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Jump Settings")]
    public float jumpForce = 10f;        // lower = snappier, less floaty
    public float doubleJumpForce = 8.5f;
    [Range(0.1f, 1f)] public float jumpCutMultiplier = 0.5f; // tap = short hop (variable jump height)

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.1f;
    public LayerMask groundLayer;

    [Header("Audio")]
    public AudioClip jumpSound;
    public AudioClip doubleJumpSound;
    public AudioClip deathSound;
    public AudioClip landSound;

    [Header("Effects")]
    public GameObject dustParticlePrefab;

    [Header("Speed Trail (optional)")]
    public TrailRenderer speedTrail;     // blue energy stream; intensifies with speed
    public float trailTimeMin   = 0.10f; // at base speed
    public float trailTimeMax   = 0.45f; // at max speed / during Speed Rush
    public float trailWidthMin  = 0.10f;
    public float trailWidthMax  = 0.35f;

    [Header("Crouch / Slide")]
    public KeyCode crouchKey = KeyCode.S;                         // Down arrow also works
    [Range(0.2f, 0.95f)] public float crouchHeightScale = 0.55f;  // collider shrink under lasers

    [Header("Death")]
    public float deathAnimDuration = 0.9f;  // pause to let the death animation finish before Game Over
    public float deathSinkDistance = 0.6f;  // how far the player sinks into the ground on death

    [Header("Jump Feel")]
    public float coyoteTime = 0.10f;        // grace period to still jump just after leaving ground
    public float jumpBufferTime = 0.12f;    // remembers a jump pressed slightly before landing
    public float fallMultiplier = 1.6f;     // extra gravity while falling (snappier descent)
    public float lowJumpMultiplier = 1.9f;  // extra gravity while rising + jump released (short hops)

    private float coyoteCounter;
    private float jumpBufferCounter;
    private bool jumpHeld;

    // ── private state ──────────────────────────────────
    private Rigidbody2D rb;
    private Animator anim;
    private AudioSource audioSource;

    private bool isGrounded;
    private bool wasGrounded;
    private bool canDoubleJump;
    private bool isDead;
    private bool isCrouching;
    private bool externalCrouchHeld;   // set by external input (e.g. mobile swipe-down)
    private int jumpsLeft;

    private Collider2D bodyCollider;
    private Vector2 standColliderSize;
    private Vector2 standColliderOffset;
    private Vector3 standLocalScale;
    private bool animHasCrouch;

    [Header("Shield Invulnerability")]
    public float invulnerabilityTime = 1.2f;
    private bool isInvulnerable;
    private SpriteRenderer spriteRenderer;

    // animator param hashes (faster than string lookup)
    private static readonly int HashIsRunning = Animator.StringToHash("isRunning");
    private static readonly int HashIsGrounded = Animator.StringToHash("isGrounded");
    private static readonly int HashIsDead     = Animator.StringToHash("isDead");
    private static readonly int HashVelocityY  = Animator.StringToHash("velocityY");
    private static readonly int HashIsCrouching = Animator.StringToHash("isCrouching");

    // ── Unity lifecycle ────────────────────────────────
    void Awake()
    {
        rb          = GetComponent<Rigidbody2D>();
        anim        = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        bodyCollider = GetComponent<Collider2D>();
        CacheStandingCollider();
        animHasCrouch = AnimatorHasParam("isCrouching");

        // play animations on unscaled time so the death clip still animates while the
        // game is frozen (Time.timeScale = 0) waiting to show the Game Over panel
        if (anim != null) anim.updateMode = AnimatorUpdateMode.UnscaledTime;
    }

    void Update()
    {
        if (isDead) return;

        CheckGround();
        ReadJumpInput();
        TryConsumeJumpBuffer();
        HandleVariableJump();
        HandleCrouchInput();
        UpdateAnimator();
        UpdateSpeedTrail();
    }

    void FixedUpdate()
    {
        ApplyGravityFeel();
    }

    // Snappier arc (Geometry-Dash style): extra gravity when falling, and when rising with the
    // jump key released. Only while actively playing (the world is frozen on death).
    void ApplyGravityFeel()
    {
        if (isDead || rb == null) return;
        if (GameManager.Instance == null || !GameManager.Instance.IsPlaying) return;

        float vy = rb.linearVelocity.y;
        if (vy < 0f)
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1f) * Time.fixedDeltaTime;
        else if (vy > 0f && !jumpHeld)
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1f) * Time.fixedDeltaTime;
    }

    // Variable jump height: releasing the jump key while rising cuts the jump short
    // (tap = short hop, hold = full jump) — makes jumping feel responsive.
    void HandleVariableJump()
    {
        bool released = Input.GetKeyUp(KeyCode.Space)
                     || Input.GetKeyUp(KeyCode.UpArrow)
                     || Input.GetKeyUp(KeyCode.W);
        if (released && rb.linearVelocity.y > 0f)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
    }

    // ── speed trail ────────────────────────────────────
    // Lengthens/widens the energy trail as game speed rises (and during Speed Rush,
    // which pushes CurrentSpeed past maxSpeed → intensity clamps at full).
    void UpdateSpeedTrail()
    {
        if (speedTrail == null) return;

        float t = GameManager.Instance != null ? GameManager.Instance.SpeedIntensity01 : 0f;
        speedTrail.time       = Mathf.Lerp(trailTimeMin,  trailTimeMax,  t);
        speedTrail.startWidth = Mathf.Lerp(trailWidthMin, trailWidthMax, t);
    }

    // ── ground check ───────────────────────────────────
    void CheckGround()
    {
        wasGrounded = isGrounded;
        isGrounded  = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (isGrounded && !wasGrounded)
        {
            // just landed
            jumpsLeft = 2;
            PlaySound(landSound, 0.6f);
            SpawnDust();
        }

        // coyote time: full while grounded, counts down once airborne
        if (isGrounded) coyoteCounter = coyoteTime;
        else            coyoteCounter -= Time.deltaTime;
    }

    // ── jump input ─────────────────────────────────────
    // Reads key state into a buffer (so a press just before landing still counts) and tracks
    // whether the jump key is held (for the variable-height short hop / low-jump gravity).
    void ReadJumpInput()
    {
        bool jumpPressed = Input.GetKeyDown(KeyCode.Space)
                        || Input.GetKeyDown(KeyCode.UpArrow)
                        || Input.GetKeyDown(KeyCode.W);
        jumpHeld = Input.GetKey(KeyCode.Space)
                || Input.GetKey(KeyCode.UpArrow)
                || Input.GetKey(KeyCode.W);

        if (jumpPressed) jumpBufferCounter = jumpBufferTime;
        else             jumpBufferCounter -= Time.deltaTime;
    }

    // Fires a buffered jump as soon as one is allowed (this frame or right after landing).
    void TryConsumeJumpBuffer()
    {
        if (jumpBufferCounter <= 0f) return;
        if (!CanJumpNow()) return;
        DoJump();
        jumpBufferCounter = 0f;
    }

    bool CanJumpNow()
    {
        if (isDead) return false;
        if (GameManager.Instance == null || !GameManager.Instance.IsPlaying) return false;
        bool unlimited = GameManager.Instance.SpeedRushActive;
        if (unlimited) return true;
        // grounded, within coyote grace, or with an air (double) jump remaining
        return isGrounded || coyoteCounter > 0f || jumpsLeft > 0;
    }

    // Public jump entry point for external input (e.g. mobile TouchInput); routed through the
    // same buffer so it benefits from coyote/buffer timing.
    public void TryJump()
    {
        jumpBufferCounter = jumpBufferTime;
        TryConsumeJumpBuffer();
    }

    void DoJump()
    {
        // first jump off the ground (or within coyote time) is the strong one; air jumps weaker
        bool firstJump = (isGrounded || coyoteCounter > 0f) && jumpsLeft >= 2;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, firstJump ? jumpForce : doubleJumpForce);
        PlaySound(firstJump ? jumpSound : doubleJumpSound);
        SpawnDust();

        coyoteCounter = 0f;
        if (jumpsLeft > 0) jumpsLeft--;
    }

    // Launches the player upward (jump pads / bounce orbs). Refreshes the air jump so the
    // player can still double-jump after a bounce. Called by JumpPad on contact.
    public void Bounce(float force)
    {
        if (isDead) return;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, force);
        jumpsLeft = 2;
        coyoteCounter = 0f;
        PlaySound(jumpSound);
        SpawnDust();
    }

    // External crouch hold (e.g. mobile swipe-down), OR-ed with the keyboard crouch keys.
    public void SetCrouchHeld(bool held)
    {
        externalCrouchHeld = held;
    }

    // ── crouch / slide ─────────────────────────────────
    void HandleCrouchInput()
    {
        // crouch only while actually playing and grounded; releasing the key stands up
        bool playing = GameManager.Instance != null && GameManager.Instance.IsPlaying;
        bool wantCrouch = playing && isGrounded &&
                          (Input.GetKey(crouchKey) || Input.GetKey(KeyCode.DownArrow) || externalCrouchHeld);

        if (wantCrouch != isCrouching)
        {
            isCrouching = wantCrouch;
            ApplyCrouchCollider(isCrouching);
        }
    }

    void CacheStandingCollider()
    {
        if (bodyCollider is CapsuleCollider2D cap) { standColliderSize = cap.size; standColliderOffset = cap.offset; }
        else if (bodyCollider is BoxCollider2D box) { standColliderSize = box.size; standColliderOffset = box.offset; }
        standLocalScale = transform.localScale;
    }

    // Shrinks the collider downward (feet stay planted) so a crouch fits under lasers.
    // Also squashes the sprite transform on Y to give a visual crouch without a dedicated clip.
    // Works with a Capsule or Box collider on the player.
    void ApplyCrouchCollider(bool crouch)
    {
        if (bodyCollider == null) return;

        Vector2 size = standColliderSize;
        Vector2 off  = standColliderOffset;
        if (crouch)
        {
            float h = standColliderSize.y * crouchHeightScale;
            off.y   = standColliderOffset.y - (standColliderSize.y - h) * 0.5f;
            size.y  = h;
        }

        // Remember the world-space bottom of the body BEFORE we change scale, so we can keep the
        // feet planted afterwards (squashing the transform otherwise lifts the sprite + the child
        // groundCheck, which caused the float + grounded flicker).
        float bottomBefore = bodyCollider.bounds.min.y;

        if (bodyCollider is CapsuleCollider2D cap) { cap.size = size; cap.offset = off; }
        else if (bodyCollider is BoxCollider2D box) { box.size = size; box.offset = off; }

        // Visual squash: compress Y scale ONLY (no X counter-scale, which caused the ugly
        // widening). Restore the exact standing scale when standing up.
        Vector3 ls = standLocalScale;
        if (crouch) ls.y = standLocalScale.y * crouchHeightScale;
        transform.localScale = ls;

        // Keep the feet planted: shift the body so the collider bottom stays where it was.
        Physics2D.SyncTransforms();
        float bottomAfter = bodyCollider.bounds.min.y;
        transform.position += new Vector3(0f, bottomBefore - bottomAfter, 0f);
        Physics2D.SyncTransforms();
    }

    bool AnimatorHasParam(string paramName)
    {
        if (anim == null) return false;
        foreach (var p in anim.parameters)
            if (p.name == paramName) return true;
        return false;
    }

    // ── animator ───────────────────────────────────────
    void UpdateAnimator()
    {
        if (anim == null) return;
        bool playing = GameManager.Instance != null && GameManager.Instance.IsPlaying;
        anim.SetBool(HashIsRunning,  playing && !isDead);   // Idle when not playing, Run when playing
        anim.SetBool(HashIsGrounded, isGrounded);
        anim.SetBool(HashIsDead,     isDead);
        anim.SetFloat(HashVelocityY, rb.linearVelocity.y);
        if (animHasCrouch) anim.SetBool(HashIsCrouching, isCrouching);
    }

    // ── death ──────────────────────────────────────────
    public void Die()
    {
        if (isDead) return;
        if (isInvulnerable) return;

        // Shield power-up: consume a charge instead of dying, with brief invulnerability so the
        // overlapping obstacle can scroll past harmlessly.
        if (PowerUpManager.Instance != null && PowerUpManager.Instance.TryConsumeShield())
        {
            StartCoroutine(InvulnerabilityRoutine());
            return;
        }

        isDead = true;

        PlaySound(deathSound);
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic; // stop physics
        transform.localScale = standLocalScale;  // undo any crouch squash

        if (anim != null)
            anim.SetBool(HashIsDead, true);

        StartCoroutine(DeathRoutine());
    }

    // Freeze the world immediately, let the death animation play (Animator runs on
    // unscaled time), then show the Game Over panel.
    IEnumerator DeathRoutine()
    {
        if (GameManager.Instance != null) GameManager.Instance.BeginDeath();

        // sink into the ground while the death animation plays (unscaled — world is frozen)
        Vector3 from = transform.position;
        Vector3 to   = from + Vector3.down * deathSinkDistance;
        float t = 0f;
        while (t < deathAnimDuration)
        {
            t += Time.unscaledDeltaTime;
            transform.position = Vector3.Lerp(from, to, t / deathAnimDuration);
            yield return null;
        }

        if (GameManager.Instance != null) GameManager.Instance.ShowGameOverScreen();
    }

    // Brief invulnerability after a shield save: blinks the sprite and ignores further hits.
    IEnumerator InvulnerabilityRoutine()
    {
        isInvulnerable = true;
        PlaySound(jumpSound, 0.8f);
        float t = 0f;
        while (t < invulnerabilityTime)
        {
            t += Time.deltaTime;
            if (spriteRenderer != null)
            {
                var c = spriteRenderer.color;
                c.a = Mathf.PingPong(t * 8f, 1f) * 0.6f + 0.4f;
                spriteRenderer.color = c;
            }
            yield return null;
        }
        if (spriteRenderer != null)
        {
            var c = spriteRenderer.color; c.a = 1f; spriteRenderer.color = c;
        }
        isInvulnerable = false;
    }

    // ── helpers ────────────────────────────────────────
    void PlaySound(AudioClip clip, float volume = 1f)
    {
        if (clip != null && audioSource != null)
            audioSource.PlayOneShot(clip, volume);
    }

    void SpawnDust()
    {
        if (dustParticlePrefab != null)
            Instantiate(dustParticlePrefab, groundCheck.position, Quaternion.identity);
    }

    public void ResetPlayer()
    {
        isDead   = false;
        jumpsLeft = 2;
        isCrouching = false;
        ApplyCrouchCollider(false);
        rb.bodyType = RigidbodyType2D.Dynamic;
        if (anim != null)
        {
            anim.SetBool(HashIsDead,     false);
            anim.SetBool(HashIsRunning,  true);
        }
    }

    // ── debug gizmo ────────────────────────────────────
    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
