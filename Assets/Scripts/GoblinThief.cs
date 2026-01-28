using UnityEngine;

public class GoblinThief : MonoBehaviour
{
    [Header("Movement")]
    public float wanderSpeed = 1f;
    public float sneakSpeed = 2f;
    public float dashSpeed = 10f;
    public float fleeSpeed = 6f;
    public float detectRange = 7f;
    public float dashRange = 2f;
    
    [Header("Stealing")]
    public int stealAmount = 5;
    public float dashDuration = 0.3f;
    public GameObject rupeePickupPrefab;
    
    [Header("Visual Feedback")]
    public Color fleeingWithLootColor = new Color(0.5f, 1f, 0.5f, 1f);
    
    [Header("Health")]
    public int health = 1;
    
    // States
    private enum State { Wander, Sneak, Dash, Flee }
    private State currentState = State.Wander;
    
    // References
    private Transform player;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    
    // Wander variables
    private Vector2 wanderDirection;
    private float wanderTimer;
    private float wanderInterval = 2f;
    
    // Dash variables
    private float stateTimer;
    private Vector2 dashDirection;
    private bool hasStolen;
    private int stolenRupees = 0;
    
    // Flee variables
    private Vector2 fleeDirection;
    
    // Original color
    private Color originalColor;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
        
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        
        PickNewWanderDirection();
    }

    void Update()
    {
        if (player == null) return;
        
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        switch (currentState)
        {
            case State.Wander:
                Wander();
                // Spot player and they have rupees?
                if (distanceToPlayer < detectRange && GameState.Instance.rupees > 0)
                {
                    currentState = State.Sneak;
                }
                break;
                
            case State.Sneak:
                Sneak();
                // Close enough to dash?
                if (distanceToPlayer < dashRange)
                {
                    StartDash();
                }
                // Player has no rupees? Go back to wandering
                else if (GameState.Instance.rupees <= 0)
                {
                    currentState = State.Wander;
                }
                // Lost player?
                else if (distanceToPlayer > detectRange * 1.5f)
                {
                    currentState = State.Wander;
                }
                break;
                
            case State.Dash:
                rb.linearVelocity = dashDirection * dashSpeed;
                stateTimer -= Time.deltaTime;
                if (stateTimer <= 0)
                {
                    // Dash ended without hitting player, flee anyway
                    StartFlee();
                }
                break;
                
            case State.Flee:
                Flee();
                break;
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
    }
    
    void PickNewWanderDirection()
    {
        float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        wanderDirection = new Vector2(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle));
        wanderTimer = wanderInterval;
    }
    
    void Sneak()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        rb.linearVelocity = direction * sneakSpeed;
        
        if (direction.x != 0)
        {
            spriteRenderer.flipX = direction.x < 0;
        }
    }
    
    void StartDash()
    {
        currentState = State.Dash;
        stateTimer = dashDuration;
        hasStolen = false;
        
        dashDirection = (player.position - transform.position).normalized;
        
        if (dashDirection.x != 0)
        {
            spriteRenderer.flipX = dashDirection.x < 0;
        }
    }
    
    void StartFlee()
    {
        currentState = State.Flee;
        
        // Flee away from player
        fleeDirection = (transform.position - player.position).normalized;
        
        // Face flee direction
        if (fleeDirection.x != 0)
        {
            spriteRenderer.flipX = fleeDirection.x < 0;
        }
        
        // If we have loot, turn green to show "I got the goods"
        if (stolenRupees > 0)
        {
            spriteRenderer.color = fleeingWithLootColor;
        }
    }
    
    void Flee()
    {
        // Keep updating flee direction away from player
        fleeDirection = (transform.position - player.position).normalized;
        rb.linearVelocity = fleeDirection * fleeSpeed;
        
        if (fleeDirection.x != 0)
        {
            spriteRenderer.flipX = fleeDirection.x < 0;
        }
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (currentState == State.Dash && !hasStolen)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                // Steal rupees!
                stolenRupees = GameState.Instance.StealRupees(stealAmount);
                hasStolen = true;
                
                // Immediately flee
                StartFlee();
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
        // Drop stolen rupees - kill him to get your money back!
        if (stolenRupees > 0 && rupeePickupPrefab != null)
        {
            for (int i = 0; i < stolenRupees; i++)
            {
                // Spawn with slight random offset so they don't stack
                Vector2 offset = Random.insideUnitCircle * 0.5f;
                Vector3 spawnPos = transform.position + new Vector3(offset.x, offset.y, 0);
                Instantiate(rupeePickupPrefab, spawnPos, Quaternion.identity);
            }
        }
        
        // Also drop normal loot
        Dropper dropper = GetComponent<Dropper>();
        if (dropper != null)
        {
            dropper.Drop();
        }
        
        Destroy(gameObject);
    }
}