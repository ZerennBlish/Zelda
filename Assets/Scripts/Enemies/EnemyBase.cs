using UnityEngine;

public abstract class EnemyBase : MonoBehaviour, IDamageable
{
    [Header("Health")]
    public int health = 1;

    protected Rigidbody2D rb;
    protected SpriteRenderer spriteRenderer;
    protected Transform player;
    protected bool isDead = false;

    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    public virtual void TakeDamage(int amount)
    {
        if (isDead) return;
        health -= amount;
        if (health <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        if (isDead) return;
        isDead = true;
        if (rb != null) rb.linearVelocity = Vector2.zero;
        OnDie();
        DropAndDestroy();
    }

    // Hook for per-enemy death side effects that run before drop+destroy
    // (player-buff award on OrcChief, rupee refund on GoblinThief, etc.).
    protected virtual void OnDie() { }

    // Reusable death tail. Subclasses that fully override Die (SlimeSplitter
    // for the Small-size branch) call this for the standard path.
    protected void DropAndDestroy()
    {
        Dropper dropper = GetComponent<Dropper>();
        if (dropper != null)
        {
            dropper.Drop();
        }
        Destroy(gameObject);
    }
}
