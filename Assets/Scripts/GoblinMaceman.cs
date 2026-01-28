using UnityEngine;

public class GoblinMaceman : MonoBehaviour
{
    [Header("Movement")]
    public float wanderSpeed = 1f;
    public float chaseSpeed = 2f;
    public float chaseRange = 5f;
    public float attackRange = 1.5f;
    
    [Header("Attack")]
    public float windUpTime = 0.4f;
    public float spinDuration = 0.5f;
    public float recoveryTime = 0.8f;
    public float spinDriftSpeed = 2f;
    public int damage = 1;
    
    [Header("Health")]
    public int health = 2;
    
    // States
    private enum State { Wander, Chase, WindUp, Spinning, Recovery }
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
    private float spinRotation;
    private Vector2 spinDirection;
    private bool hasHitPlayer;
    
    // Original rotation for reset
    private Quaternion originalRotation;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalRotation = transform.rotation;
        
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
                if (distanceToPlayer < attackRange)
                {
                    StartWindUp();
                }
                else if (distanceToPlayer > chaseRange * 1.5f)
                {
                    currentState = State.Wander;
                }
                break;
                
            case State.WindUp:
                rb.linearVelocity = Vector2.zero;
                stateTimer -= Time.deltaTime;
                if (stateTimer <= 0)
                {
                    StartSpin();
                }
                break;
                
            case State.Spinning:
                Spin();
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
        rb.linearVelocity = Vector2.zero;
        
        spinDirection = (player.position - transform.position).normalized;
    }
    
    void StartSpin()
    {
        currentState = State.Spinning;
        stateTimer = spinDuration;
        spinRotation = 0f;
        hasHitPlayer = false;
    }
    
    void Spin()
    {
        float rotationThisFrame = (360f / spinDuration) * Time.deltaTime;
        spinRotation += rotationThisFrame;
        transform.rotation = Quaternion.Euler(0, 0, -spinRotation);
        
        float driftMultiplier = stateTimer / spinDuration;
        rb.linearVelocity = spinDirection * spinDriftSpeed * driftMultiplier;
    }
    
    void StartRecovery()
    {
        currentState = State.Recovery;
        stateTimer = recoveryTime;
        rb.linearVelocity = Vector2.zero;
        
        transform.rotation = originalRotation;
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (currentState == State.Spinning && !hasHitPlayer)
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
    
    void OnCollisionStay2D(Collision2D collision)
    {
        if (currentState == State.Spinning && !hasHitPlayer)
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