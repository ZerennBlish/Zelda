using UnityEngine;

public class GoblinSpearman : MonoBehaviour
{
    [Header("Movement")]
    public float wanderSpeed = 1f;
    public float chaseSpeed = 1.5f;
    public float chaseRange = 6f;
    public float chargeStartRange = 4f;
    
    [Header("Charge Attack")]
    public float windUpTime = 0.6f;
    public float chargeSpeed = 12f;
    public float chargeDuration = 0.5f;
    public float slideDuration = 0.3f;
    public float recoveryTime = 1f;
    public int damage = 1;
    
    [Header("Wind-Up Effects")]
    public float pullbackSpeed = 3f;
    public Color windUpColor = new Color(1f, 0.3f, 0.3f, 1f);
    
    [Header("Health")]
    public int health = 2;
    
    // States
    private enum State { Wander, Chase, WindUp, Charging, Sliding, Recovery }
    private State currentState = State.Wander;
    
    // References
    private Transform player;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    
    // Wander variables
    private Vector2 wanderDirection;
    private float wanderTimer;
    private float wanderInterval = 2f;
    
    // Attack variables
    private float stateTimer;
    private Vector2 chargeDirection;
    private float slideStartSpeed;
    private bool hasHitPlayer;
    
    // Color storage
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
                if (distanceToPlayer < chaseRange)
                {
                    currentState = State.Chase;
                }
                break;
                
            case State.Chase:
                Chase();
                if (distanceToPlayer < chargeStartRange)
                {
                    StartWindUp();
                }
                else if (distanceToPlayer > chaseRange * 1.5f)
                {
                    currentState = State.Wander;
                }
                break;
                
            case State.WindUp:
                WindUp();
                stateTimer -= Time.deltaTime;
                if (stateTimer <= 0)
                {
                    StartCharge();
                }
                break;
                
            case State.Charging:
                rb.linearVelocity = chargeDirection * chargeSpeed;
                stateTimer -= Time.deltaTime;
                if (stateTimer <= 0)
                {
                    StartSlide();
                }
                break;
                
            case State.Sliding:
                Slide();
                stateTimer -= Time.deltaTime;
                if (stateTimer <= 0)
                {
                    StartRecovery();
                }
                break;
                
            case State.Recovery:
                rb.linearVelocity = Vector2.zero;
                stateTimer -= Time.deltaTime;
                if (stateTimer <= 0)
                {
                    currentState = State.Chase;
                }
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
    
    void Chase()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        rb.linearVelocity = direction * chaseSpeed;
        
        if (direction.x != 0)
        {
            spriteRenderer.flipX = direction.x < 0;
        }
    }
    
    void StartWindUp()
    {
        currentState = State.WindUp;
        stateTimer = windUpTime;
        
        // Lock in charge direction
        chargeDirection = (player.position - transform.position).normalized;
        
        // Face charge direction
        if (chargeDirection.x != 0)
        {
            spriteRenderer.flipX = chargeDirection.x < 0;
        }
        
        // Flash red
        spriteRenderer.color = windUpColor;
    }
    
    void WindUp()
    {
        // Pull back away from charge direction
        rb.linearVelocity = -chargeDirection * pullbackSpeed;
    }
    
    void StartCharge()
    {
        currentState = State.Charging;
        stateTimer = chargeDuration;
        hasHitPlayer = false;
        
        // Reset color
        spriteRenderer.color = originalColor;
    }
    
    void StartSlide()
    {
        currentState = State.Sliding;
        stateTimer = slideDuration;
        slideStartSpeed = chargeSpeed;
    }
    
    void Slide()
    {
        float slideProgress = 1f - (stateTimer / slideDuration);
        float currentSpeed = Mathf.Lerp(slideStartSpeed, 0f, slideProgress);
        rb.linearVelocity = chargeDirection * currentSpeed;
    }
    
    void StartRecovery()
    {
        currentState = State.Recovery;
        stateTimer = recoveryTime;
        rb.linearVelocity = Vector2.zero;
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (currentState == State.Charging && !hasHitPlayer)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(damage);
                    hasHitPlayer = true;
                }
            }
            
            if (collision.gameObject.CompareTag("Wall"))
            {
                StartSlide();
            }
        }
    }
    
    void OnCollisionStay2D(Collision2D collision)
    {
        if (currentState == State.Charging && !hasHitPlayer)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(damage);
                    hasHitPlayer = true;
                }
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
        Dropper dropper = GetComponent<Dropper>();
        if (dropper != null)
        {
            dropper.Drop();
        }
        
        Destroy(gameObject);
    }
}