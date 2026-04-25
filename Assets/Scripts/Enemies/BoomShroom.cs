using UnityEngine;

public class BoomShroom : MonoBehaviour, IDamageable
{
    public float moveSpeed = 2f;
    public float chaseRange = 5f;
    public float wanderChangeTime = 2f;
    public float explosionRadius = 2f;
    public int explosionDamage = 1;
    public float blinkTime = 0.2f;
    public int blinkCount = 3;
    public GameObject explosionEffectPrefab;
    
    private Rigidbody2D rb;
    private Transform player;
    private Vector2 wanderDirection;
    private float wanderTimer;
    private bool isExploding = false;
    private bool hasExploded = false;
    private bool isDead = false;
    private SpriteRenderer spriteRenderer;
    private Dropper dropper;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        dropper = GetComponent<Dropper>();
        
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        
        PickNewWanderDirection();
    }

    void Update()
    {
        if (isExploding) return;
        if (player == null) return;
        
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        if (distanceToPlayer <= chaseRange)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            rb.linearVelocity = direction * moveSpeed;
        }
        else
        {
            wanderTimer -= Time.deltaTime;
            if (wanderTimer <= 0f)
            {
                PickNewWanderDirection();
            }
            rb.linearVelocity = wanderDirection * moveSpeed;
        }
    }

    void PickNewWanderDirection()
    {
        float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        wanderDirection = new Vector2(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle));
        wanderTimer = wanderChangeTime;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasExploded || isExploding) return;

        if (collision.gameObject.CompareTag("Player") ||
            collision.gameObject.CompareTag("Wall") ||
            collision.gameObject.CompareTag("CrackedWall"))
        {
            StartCoroutine(BlinkThenExplode());
        }
    }

    System.Collections.IEnumerator BlinkThenExplode()
    {
        isExploding = true;
        rb.linearVelocity = Vector2.zero;
        
        for (int i = 0; i < blinkCount; i++)
        {
            spriteRenderer.enabled = false;
            yield return new WaitForSeconds(blinkTime);
            spriteRenderer.enabled = true;
            yield return new WaitForSeconds(blinkTime);
        }
        
        Explode();
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;
        Explode();
    }

    public void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;
        isDead = true;

        // Damage to player + enemies + destructibles is handled by the spawned
        // ExplosionEffect, which uses the no-source TakeDamage so the shield
        // can't block a radial blast (matches Bomb behavior).
        if (explosionEffectPrefab != null)
        {
            GameObject effect = Instantiate(
                explosionEffectPrefab,
                transform.position,
                Quaternion.identity);

            ExplosionEffect ee = effect.GetComponent<ExplosionEffect>();
            if (ee != null)
            {
                ee.explosionRadius = explosionRadius;
                ee.explosionDamage = explosionDamage;
            }
        }

        if (dropper != null)
        {
            dropper.Drop();
        }

        Destroy(gameObject);
    }
}