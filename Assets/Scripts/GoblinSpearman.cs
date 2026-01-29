using UnityEngine;

public class GoblinSpearman : MonoBehaviour, IStunnable
{
    [Header("Movement")]
    public float wanderSpeed = 1f;
    public float chaseSpeed = 2f;
    public float chaseRange = 5f;
    public float attackRange = 2.5f;
    
    [Header("Attack")]
    public float pullbackTime = 0.5f;
    public float chargeSpeed = 8f;
    public float chargeDuration = 0.4f;
    public float recoveryTime = 1f;
    public int damage = 1;
    
    [Header("Telegraph")]
    public Color telegraphColor = Color.red;
    public float pullbackDistance = 0.5f;
    
    [Header("Health")]
    public int health = 2;
    
    [Header("Stun")]
    public Color stunColor = new Color(0.5f, 0.5f, 1f, 1f);
    
    private enum State { Wander, Chase, Pullback, Charging, Recovery, Stunned }
    private State currentState = State.Wander;
    
    private Transform player;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    
    private Vector2 wanderDirection;
    private float wanderTimer;
    private float wanderInterval = 2f;
    
    private float stateTimer;
    private Vector2 chargeDirection;
    private Vector3 pullbackStartPos;
    private bool hasHitPlayer;
    
    private float stunTimer;
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
        
        if (currentState == State.Stunned)
        {
            rb.linearVelocity = Vector2.zero;
            stunTimer -= Time.deltaTime;
            if (stunTimer <= 0)
            {
                currentState = State.Wander;
                spriteRenderer.color = originalColor;
            }
            return;
        }
        
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
                    StartPullback();
                }
                else if (distanceToPlayer > chaseRange * 1.5f)
                {
                    currentState = State.Wander;
                }
                break;
                
            case State.Pullback:
                Pullback();
                stateTimer -= Time.deltaTime;
                if (stateTimer <= 0)
                {
                    StartCharge();
                }
                break;
                
            case State.Charging:
                Charge();
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
    
    void StartPullback()
    {
        currentState = State.Pullback;
        stateTimer = pullbackTime;
        
        chargeDirection = (player.position - transform.position).normalized;
        pullbackStartPos = transform.position;
        
        spriteRenderer.color = telegraphColor;
    }
    
    void Pullback()
    {
        Vector3 pullbackPos = pullbackStartPos - (Vector3)(chargeDirection * pullbackDistance);
        transform.position = Vector3.Lerp(pullbackStartPos, pullbackPos, 1 - (stateTimer / pullbackTime));
        rb.linearVelocity = Vector2.zero;
    }
    
    void StartCharge()
    {
        currentState = State.Charging;
        stateTimer = chargeDuration;
        hasHitPlayer = false;
        
        spriteRenderer.color = originalColor;
    }
    
    void Charge()
    {
        rb.linearVelocity = chargeDirection * chargeSpeed;
    }
    
    void StartRecovery()
    {
        currentState = State.Recovery;
        stateTimer = recoveryTime;
        rb.linearVelocity = Vector2.zero;
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (currentState == State.Stunned) return;
        
        if (currentState == State.Charging && !hasHitPlayer)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(damage, transform.position);
                    hasHitPlayer = true;
                }
            }
        }
        
        if (currentState == State.Charging && collision.gameObject.CompareTag("Wall"))
        {
            StartRecovery();
        }
    }
    
    void OnCollisionStay2D(Collision2D collision)
    {
        if (currentState == State.Stunned) return;
        
        if (currentState == State.Charging && !hasHitPlayer)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(damage, transform.position);
                    hasHitPlayer = true;
                }
            }
        }
    }
    
    public void Stun(float duration)
    {
        currentState = State.Stunned;
        stunTimer = duration;
        rb.linearVelocity = Vector2.zero;
        spriteRenderer.color = stunColor;
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