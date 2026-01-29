using UnityEngine;

public class ShieldKnight : MonoBehaviour
{
    [Header("Movement")]
    public float wanderSpeed = 0.8f;
    public float chaseSpeed = 1.5f;
    public float chaseRange = 5f;
    public float attackRange = 1.2f;
    
    [Header("Attack")]
    public float attackCooldown = 1.5f;
    public int damage = 1;
    
    [Header("Shield")]
    public Transform shieldTransform;
    public float shieldDistance = 0.3f;
    public float shieldArc = 120f;
    
    [Header("Health")]
    public int health = 3;
    
    [Header("Block Feedback")]
    public Color blockFlashColor = new Color(0.5f, 0.5f, 1f, 1f);
    public float blockFlashDuration = 0.1f;
    
    private enum State { Wander, Chase, Attack, Cooldown }
    private State currentState = State.Wander;
    
    private Transform player;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private SpriteRenderer shieldRenderer;
    
    private Vector2 wanderDirection;
    private float wanderTimer;
    private float wanderInterval = 2f;
    
    private Vector2 facingDirection = Vector2.down;
    
    private float attackTimer;
    private bool hasHitPlayer;
    
    private Color originalColor;
    private Color originalShieldColor;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
        
        if (shieldTransform != null)
        {
            shieldRenderer = shieldTransform.GetComponent<SpriteRenderer>();
            if (shieldRenderer != null)
            {
                originalShieldColor = shieldRenderer.color;
            }
        }
        
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        
        PickNewWanderDirection();
        UpdateShieldPosition();
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
                if (distanceToPlayer < attackRange && attackTimer <= 0)
                {
                    StartAttack();
                }
                else if (distanceToPlayer > chaseRange * 1.5f)
                {
                    currentState = State.Wander;
                }
                break;
                
            case State.Attack:
                rb.linearVelocity = Vector2.zero;
                break;
                
            case State.Cooldown:
                rb.linearVelocity = Vector2.zero;
                attackTimer -= Time.deltaTime;
                if (attackTimer <= 0)
                {
                    currentState = State.Chase;
                }
                break;
        }
        
        UpdateShieldPosition();
    }
    
    void Wander()
    {
        wanderTimer -= Time.deltaTime;
        if (wanderTimer <= 0)
        {
            PickNewWanderDirection();
        }
        
        rb.linearVelocity = wanderDirection * wanderSpeed;
        UpdateFacingDirection(wanderDirection);
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
        UpdateFacingDirection(direction);
    }
    
    void UpdateFacingDirection(Vector2 moveDirection)
    {
        if (moveDirection.magnitude > 0.1f)
        {
            facingDirection = moveDirection.normalized;
            
            if (moveDirection.x != 0)
            {
                spriteRenderer.flipX = moveDirection.x < 0;
            }
        }
    }
    
    void UpdateShieldPosition()
    {
        if (shieldTransform == null) return;
        
        shieldTransform.localPosition = facingDirection * shieldDistance;
        
        float angle = Mathf.Atan2(facingDirection.y, facingDirection.x) * Mathf.Rad2Deg;
        shieldTransform.localRotation = Quaternion.Euler(0, 0, angle - 90f);
    }
    
    void StartAttack()
    {
        currentState = State.Attack;
        hasHitPlayer = false;
        
        Invoke("EndAttack", 0.3f);
    }
    
    void EndAttack()
    {
        currentState = State.Cooldown;
        attackTimer = attackCooldown;
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (currentState == State.Attack && !hasHitPlayer)
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
    
    void OnCollisionStay2D(Collision2D collision)
    {
        if (currentState == State.Attack && !hasHitPlayer)
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
    
    public void TakeDamage(int amount, Vector2 attackSource)
    {
Vector2 attackDirection = (attackSource - (Vector2)transform.position).normalized;        float angle = Vector2.Angle(facingDirection, attackDirection);
        
        if (angle < shieldArc / 2f)
        {
            Block();
            return;
        }
        
        health -= amount;
        
        if (health <= 0)
        {
            Die();
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
    
    void Block()
    {
        StartCoroutine(BlockFlash());
    }
    
    System.Collections.IEnumerator BlockFlash()
    {
        spriteRenderer.color = blockFlashColor;
        if (shieldRenderer != null)
        {
            shieldRenderer.color = blockFlashColor;
        }
        
        yield return new WaitForSeconds(blockFlashDuration);
        
        spriteRenderer.color = originalColor;
        if (shieldRenderer != null)
        {
            shieldRenderer.color = originalShieldColor;
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