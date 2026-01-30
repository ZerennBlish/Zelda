using UnityEngine;

public class GoblinThief : MonoBehaviour
{
    [Header("Movement")]
    public float wanderSpeed = 1.5f;
    public float sneakSpeed = 2f;
    public float dashSpeed = 6f;
    public float fleeSpeed = 4f;
    
    [Header("Ranges")]
    public float detectRange = 6f;
    public float dashRange = 2f;
    public float stealRange = 0.8f;
    
    [Header("Timing")]
    public float dashDuration = 0.3f;
    public float fleeDuration = 3f;
    
    [Header("Stealing")]
    public int stealAmount = 5;
    public int stolenRupees = 0;
    
    [Header("Health")]
    public int health = 1;
    
    private enum State { Wander, Sneak, Dash, Steal, Flee }
    private State currentState = State.Wander;
    
    private Transform player;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    
    private Vector2 wanderDirection;
    private float wanderTimer;
    private float wanderInterval = 2f;
    
    private float stateTimer;
    private Vector2 dashDirection;
    private bool hasStolen = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
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
                if (distanceToPlayer < detectRange && !hasStolen)
                {
                    currentState = State.Sneak;
                }
                break;
                
            case State.Sneak:
                Sneak();
                if (distanceToPlayer < dashRange)
                {
                    StartDash();
                }
                else if (distanceToPlayer > detectRange * 1.5f)
                {
                    currentState = State.Wander;
                }
                break;
                
            case State.Dash:
                Dash();
                stateTimer -= Time.deltaTime;
                if (distanceToPlayer < stealRange)
                {
                    Steal();
                }
                else if (stateTimer <= 0)
                {
                    currentState = State.Sneak;
                }
                break;
                
            case State.Flee:
                Flee();
                stateTimer -= Time.deltaTime;
                if (stateTimer <= 0 || distanceToPlayer > detectRange * 2f)
                {
                    currentState = State.Wander;
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
    
    void Sneak()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        rb.linearVelocity = direction * sneakSpeed;
        UpdateFacing(direction);
    }
    
    void StartDash()
    {
        currentState = State.Dash;
        stateTimer = dashDuration;
        dashDirection = (player.position - transform.position).normalized;
    }
    
    void Dash()
    {
        rb.linearVelocity = dashDirection * dashSpeed;
    }
    
    void Steal()
    {
        GameState gameState = FindFirstObjectByType<GameState>();
        if (gameState != null)
        {
            int stolen = gameState.StealRupees(stealAmount);
            stolenRupees += stolen;
        }
        
        hasStolen = true;
        currentState = State.Flee;
        stateTimer = fleeDuration;
    }
    
    void Flee()
    {
        Vector2 direction = (transform.position - player.position).normalized;
        rb.linearVelocity = direction * fleeSpeed;
        UpdateFacing(direction);
    }
    
    void UpdateFacing(Vector2 direction)
    {
        if (direction.x != 0)
        {
            spriteRenderer.flipX = direction.x < 0;
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