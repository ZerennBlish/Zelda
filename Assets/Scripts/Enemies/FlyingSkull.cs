using UnityEngine;

public class FlyingSkull : MonoBehaviour, IStunnable
{
    [Header("Movement")]
    public float wanderSpeed = 2f;
    public float swoopSpeed = 6f;
    public float pullbackSpeed = 2f;
    public float changeDirectionInterval = 1.5f;
    public float chaseRange = 6f;
    
    [Header("Swoop")]
    public float pullbackDuration = 0.3f;
    public float swoopDuration = 0.5f;
    public float swoopCooldown = 1.5f;
    
    [Header("Room Bounds")]
    public float roomWidth = 18f;
    public float roomHeight = 10f;
    
    [Header("Combat")]
    public int damage = 1;
    
    [Header("Health")]
    public int health = 1;
    
    [Header("Stun")]
    public Color stunColor = new Color(0.5f, 0.5f, 1f, 1f);
    
    private enum State { Wander, Pullback, Swoop, Cooldown, Stunned }
    private State currentState = State.Wander;
    
    private Transform player;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    
    private Vector2 roomCenter;
    private Vector2 moveDirection;
    private Vector2 swoopDirection;
    private float directionTimer;
    private float stateTimer;
    private bool hasHitPlayer;
    
    private float stunTimer;
    private Color originalColor;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
        
        roomCenter = new Vector2(
            Mathf.Round(transform.position.x / roomWidth) * roomWidth,
            Mathf.Round(transform.position.y / roomHeight) * roomHeight
        );
        
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        
        PickNewDirection();
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
                    StartPullback();
                }
                break;
                
            case State.Pullback:
                Pullback();
                stateTimer -= Time.deltaTime;
                if (stateTimer <= 0)
                {
                    StartSwoop();
                }
                break;
                
            case State.Swoop:
                Swoop();
                stateTimer -= Time.deltaTime;
                if (stateTimer <= 0)
                {
                    StartCooldown();
                }
                break;
                
            case State.Cooldown:
                Wander();
                stateTimer -= Time.deltaTime;
                if (stateTimer <= 0)
                {
                    currentState = State.Wander;
                }
                break;
        }
        
        ClampToRoom();
    }
    
    void Wander()
    {
        directionTimer -= Time.deltaTime;
        if (directionTimer <= 0)
        {
            PickNewDirection();
        }
        
        rb.linearVelocity = moveDirection * wanderSpeed;
        UpdateFacing(moveDirection);
    }
    
    void PickNewDirection()
    {
        float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        moveDirection = new Vector2(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle));
        directionTimer = changeDirectionInterval;
    }
    
    void StartPullback()
    {
        currentState = State.Pullback;
        stateTimer = pullbackDuration;
        
        swoopDirection = (player.position - transform.position).normalized;
        UpdateFacing(swoopDirection);
    }
    
    void Pullback()
    {
        rb.linearVelocity = -swoopDirection * pullbackSpeed;
    }
    
    void StartSwoop()
    {
        currentState = State.Swoop;
        stateTimer = swoopDuration;
        hasHitPlayer = false;
    }
    
    void Swoop()
    {
        rb.linearVelocity = swoopDirection * swoopSpeed;
    }
    
    void StartCooldown()
    {
        currentState = State.Cooldown;
        stateTimer = swoopCooldown;
        PickNewDirection();
    }
    
    void UpdateFacing(Vector2 direction)
    {
        if (direction.x != 0)
        {
            spriteRenderer.flipX = direction.x < 0;
        }
    }
    
    void ClampToRoom()
    {
        float minX = roomCenter.x - (roomWidth / 2f) + 0.5f;
        float maxX = roomCenter.x + (roomWidth / 2f) - 0.5f;
        float minY = roomCenter.y - (roomHeight / 2f) + 0.5f;
        float maxY = roomCenter.y + (roomHeight / 2f) - 0.5f;
        
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);
        transform.position = pos;
        
        if (currentState == State.Wander || currentState == State.Cooldown)
        {
            if (transform.position.x == minX || transform.position.x == maxX ||
                transform.position.y == minY || transform.position.y == maxY)
            {
                PickNewDirection();
            }
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (currentState == State.Stunned) return;
        
        if (other.CompareTag("Player") && currentState == State.Swoop && !hasHitPlayer)
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage, transform.position);
                hasHitPlayer = true;
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