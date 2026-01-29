using UnityEngine;

public class SlimeSplitter : MonoBehaviour
{
    public enum SlimeSize { Large, Medium, Small }
    
    [Header("Size")]
    public SlimeSize currentSize = SlimeSize.Large;
    
    [Header("Movement")]
    public float wanderSpeed = 1f;
    public float chaseSpeed = 2f;
    public float chaseRange = 5f;
    
    [Header("Combat")]
    public int damage = 1;
    public float damageCooldown = 1f;
    
    [Header("Health")]
    public int health = 2;
    
    [Header("Split Settings")]
    public GameObject slimePrefab;
    public int splitCount = 2;
    public float splitSpread = 0.5f;
    
    [Header("Size Scales")]
    public float largeScale = 0.6f;
    public float mediumScale = 0.4f;
    public float smallScale = 0.25f;
    
    [Header("Size Stats")]
    public int largeHealth = 3;
    public int mediumHealth = 2;
    public int smallHealth = 1;
    
    private enum State { Wander, Chase }
    private State currentState = State.Wander;
    
    private Transform player;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    
    private Vector2 wanderDirection;
    private float wanderTimer;
    private float wanderInterval = 2f;
    private float damageTimer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        
        ApplySizeStats();
        PickNewWanderDirection();
    }
    
    void ApplySizeStats()
    {
        float scale = largeScale;
        
        switch (currentSize)
        {
            case SlimeSize.Large:
                scale = largeScale;
                health = largeHealth;
                break;
            case SlimeSize.Medium:
                scale = mediumScale;
                health = mediumHealth;
                break;
            case SlimeSize.Small:
                scale = smallScale;
                health = smallHealth;
                break;
        }
        
        transform.localScale = new Vector3(scale, scale, 1f);
    }

    void Update()
    {
        if (player == null) return;
        
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        if (distanceToPlayer < chaseRange)
        {
            currentState = State.Chase;
        }
        else
        {
            currentState = State.Wander;
        }
        
        switch (currentState)
        {
            case State.Wander:
                Wander();
                break;
            case State.Chase:
                Chase();
                break;
        }
        
        if (damageTimer > 0)
        {
            damageTimer -= Time.deltaTime;
        }
    }
    
    void Wander()
    {
        wanderTimer -= Time.deltaTime;
        if (wanderTimer <= 0)
        {
            PickNewWanderDirection();
        }
        
        rb.linearVelocity = wanderDirection * wanderSpeed;
        UpdateFacing(wanderDirection);
    }
    
    void PickNewWanderDirection()
    {
        float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        wanderDirection = new Vector2(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle));
        wanderTimer = wanderInterval;
    }
    
    void Chase()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        rb.linearVelocity = direction * chaseSpeed;
        UpdateFacing(direction);
    }
    
    void UpdateFacing(Vector2 direction)
    {
        if (direction.x != 0)
        {
            spriteRenderer.flipX = direction.x < 0;
        }
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && damageTimer <= 0)
        {
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage, transform.position);
                damageTimer = damageCooldown;
            }
        }
    }
    
    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && damageTimer <= 0)
        {
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage, transform.position);
                damageTimer = damageCooldown;
            }
        }
    }
    
    public void TakeDamage(int amount)
    {
        health -= amount;
        
        if (health <= 0)
        {
            Die();
        }
    }
    
    void Die()
    {
        if (currentSize != SlimeSize.Small && slimePrefab != null)
        {
            SlimeSize nextSize = (currentSize == SlimeSize.Large) ? SlimeSize.Medium : SlimeSize.Small;
            
            for (int i = 0; i < splitCount; i++)
            {
                Vector2 offset = Random.insideUnitCircle * splitSpread;
                Vector3 spawnPos = transform.position + new Vector3(offset.x, offset.y, 0);
                
                GameObject newSlime = Instantiate(slimePrefab, spawnPos, Quaternion.identity);
                SlimeSplitter splitter = newSlime.GetComponent<SlimeSplitter>();
                if (splitter != null)
                {
                    splitter.currentSize = nextSize;
                }
            }
        }
        else
        {
            Dropper dropper = GetComponent<Dropper>();
            if (dropper != null)
            {
                dropper.Drop();
            }
        }
        
        Destroy(gameObject);
    }
}