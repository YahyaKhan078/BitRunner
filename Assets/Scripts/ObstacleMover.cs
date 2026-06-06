using UnityEngine;

public class ObstacleMover : MonoBehaviour
{
    [Header("Destroy When Past")]
    public float destroyAtX = -15f;

    [Header("Collision")]
    // Obstacles kill the player on contact. Collectibles reuse this mover for
    // scrolling but set this false so touching them is harmless.
    public bool killOnContact = true;

    // speed is read from GameManager every frame so it stays in sync
    void Update()
    {
        float speed = GameManager.Instance != null
            ? GameManager.Instance.CurrentSpeed
            : 5f;

        transform.Translate(Vector3.left * speed * Time.deltaTime);

        if (transform.position.x < destroyAtX)
            Despawn();
    }

    // Returns to the pool if one exists, otherwise destroys (preserving old behaviour).
    void Despawn()
    {
        if (ObjectPool.Instance != null)
            ObjectPool.Instance.Release(gameObject);
        else
            Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!killOnContact) return;

        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
                player.Die();
        }
    }
}
