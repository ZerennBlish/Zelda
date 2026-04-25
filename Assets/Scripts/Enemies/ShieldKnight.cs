using UnityEngine;

public class ShieldKnight : MonoBehaviour, IStunnable, IDamageable{
    [Header("Movement")]
    public float wanderSpeed = 0.8f;
    public float chaseSpeed = 1.5f;
    public float chaseRange = 5f;
    public float attackRange = 1.2f;
    
    [Header("Attack")]
    public float attackCooldown = 1.5f;
    public int damage = 1;
    public float bashDistance = 0.6f;
    public float bashDuration = 0.2f;
    
    [Header("Shield")]
    public Transform shieldTransform;
    public float shieldDistance = 0.3f;
    public float shieldArc = 120f;
    
    [Header("Health")]
    public int health = 3;
    
    [Header("Block Feedback")]
    public Color blockFlashColor = new Color(0.5f, 0.5f, 1f, 1f);
    public float blockFlashDuration = 0.1f;
    
    [Header("Stun")]
    public Color stunColor = new Color(0.5f, 0.5f, 1f, 1f);
    
    private enum State { Wander, Chase, Attack, Cooldown, Stunned }
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
    private float bashTimer;
    private bool hasHitPlayer;
    
    private Color originalColor;
    private Color originalShieldColor;

    private float stunTimer;
    private bool isDead = false;

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
        if (isDead) return;
        if (player == null) return;

        // Handle stunned state
        if (currentState == State.Stunned)
        {
            rb.linearVelocity = Vector2.zero;
            stunTimer -= Time.deltaTime;
            if (stunTimer <= 0)
            {
                currentState = State.Wander;
                spriteRenderer.color = originalColor;
                if (shieldRenderer != null)
                {
                    shieldRenderer.color = originalShieldColor;
                }
                EnemyBuff buff = GetComponent<EnemyBuff>();
                if (buff != null) buff.ReapplyTint();
            }
            return;
        }

        // Tick the attack cooldown every non-stunned frame so a stun-during-
        // cooldown doesn't leave attackTimer permanently stuck above zero.
        if (attackTimer > 0) attackTimer -= Time.deltaTime;

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
                bashTimer -= Time.deltaTime;

                // Animate shield bash
                if (shieldTransform != null)
                {
                    float bashProgress = 1f - (bashTimer / bashDuration);
                    if (bashProgress < 0.5f)
                    {
                        // Thrust forward
                        shieldTransform.localPosition = facingDirection * (shieldDistance + bashDistance * (bashProgress * 2f));
                    }
                    else
                    {
                        // Pull back
                        shieldTransform.localPosition = facingDirection * (shieldDistance + bashDistance * (1f - (bashProgress - 0.5f) * 2f));
                    }
                }

                if (bashTimer <= 0)
                {
                    EndAttack();
                }
                break;

            case State.Cooldown:
                rb.linearVelocity = Vector2.zero;
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
        if (currentState == State.Attack) return; // Don't override during bash
        
        shieldTransform.localPosition = facingDirection * shieldDistance;
        
        float angle = Mathf.Atan2(facingDirection.y, facingDirection.x) * Mathf.Rad2Deg;
        shieldTransform.localRotation = Quaternion.Euler(0, 0, angle - 90f);
    }
    
    void StartAttack()
    {
        currentState = State.Attack;
        bashTimer = bashDuration;
        hasHitPlayer = false;
        
        // Face the player
        facingDirection = (player.position - transform.position).normalized;
    }
    
    void EndAttack()
    {
        currentState = State.Cooldown;
        attackTimer = attackCooldown;
        
        // Reset shield position
        if (shieldTransform != null)
        {
            shieldTransform.localPosition = facingDirection * shieldDistance;
        }
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (currentState == State.Stunned) return;
        
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
        if (currentState == State.Stunned) return;
        
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
    
    public void Stun(float duration)
    {
        currentState = State.Stunned;
        stunTimer = duration;
        rb.linearVelocity = Vector2.zero;
        spriteRenderer.color = stunColor;
        if (shieldRenderer != null)
        {
            shieldRenderer.color = stunColor;
        }
    }
    
    // Pure direction check — true if attack is inside the shield arc and the
    // knight is awake. Stunned knights cannot block. Used by TakeDamage and
    // also by callers that need to gate stun/pull (Boomerang, GrapplingHook).
    private bool IsBlockingDirection(Vector2 attackSource)
    {
        if (currentState == State.Stunned) return false;
        Vector2 attackDirection = (attackSource - (Vector2)transform.position).normalized;
        float angle = Vector2.Angle(facingDirection, attackDirection);
        return angle < shieldArc / 2f;
    }

    // Public query for non-damage effects (stun, pull). Returns true if the
    // attack is blocked. Triggers the block flash so the player gets the
    // same visual feedback they'd get from a blocked damage hit.
    public bool IsBlockingFrom(Vector2 attackSource)
    {
        if (IsBlockingDirection(attackSource))
        {
            Block();
            return true;
        }
        return false;
    }

    // Returns true if damage was applied, false if blocked. Callers use the
    // return value to decide whether to play HitFlash — block flash is handled
    // internally and shouldn't be overwritten by a hit flash on top.
    public bool TakeDamage(int amount, Vector2 attackSource)
    {
        if (isDead) return false;

        if (currentState == State.Stunned)
        {
            health -= amount;
            if (health <= 0)
            {
                Die();
            }
            return true;
        }

        if (IsBlockingDirection(attackSource))
        {
            Block();
            return false;
        }

        health -= amount;

        if (health <= 0)
        {
            Die();
        }

        return true;
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;
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

        yield return new WaitForSecondsRealtime(blockFlashDuration);

        // Don't overwrite stun color (or post-death state) if we got stunned
        // or killed during the flash wait.
        if (isDead) yield break;
        if (currentState == State.Stunned) yield break;

        spriteRenderer.color = originalColor;
        if (shieldRenderer != null)
        {
            shieldRenderer.color = originalShieldColor;
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;
        Dropper dropper = GetComponent<Dropper>();
        if (dropper != null)
        {
            dropper.Drop();
        }

        Destroy(gameObject);
    }
}