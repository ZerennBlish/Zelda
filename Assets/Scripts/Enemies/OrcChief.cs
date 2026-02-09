using UnityEngine;
using System.Collections.Generic;

public class OrcChief : MonoBehaviour, IStunnable, IDamageable
{
    [Header("Movement")]
    public float wanderSpeed = 1f;
    public float chaseSpeed = 2f;
    public float chaseRange = 6f;
    public float attackRange = 1.8f;
    
    [Header("Attack")]
    public float windUpTime = 0.6f;
    public float swingDuration = 0.3f;
    public float swingLungeSpeed = 5f;
    public float recoveryTime = 1f;
    public int swingDamage = 2;
    public int contactDamage = 1;
    
    [Header("Telegraph")]
    public Color telegraphColor = Color.red;
    
    [Header("Health")]
    public int health = 5;
    
    [Header("Enemy Buff")]
    public float buffRadius = 12f;
    
    [Header("Stun")]
    public Color stunColor = new Color(0.5f, 0.5f, 1f, 1f);
    
    private enum State { Wander, Chase, WindUp, Swing, Recovery, Stunned }
    private State currentState = State.Wander;
    
    private Transform player;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    
    private Vector2 wanderDirection;
    private float wanderTimer;
    private float wanderInterval = 2f;
    
    private float stateTimer;
    private Vector2 attackDirection;
    private bool hasHitPlayer;
    
    private float stunTimer;
    private Color originalColor;
    
    // Buff tracking
    private bool hasBuffedEnemies = false;
    private EnemyBuff.BuffType chosenEnemyBuff;
    private List<EnemyBuff> activeBuffs = new List<EnemyBuff>();

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
        
        // Pick which buff this Chief will give its allies
        chosenEnemyBuff = (EnemyBuff.BuffType)Random.Range(0, 3);
        
        PickNewWanderDirection();
    }

    void Update()
    {
        if (player == null) return;
        
        // Handle stun
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
                    BuffNearbyEnemies();
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
                    StartSwing();
                }
                break;
                
            case State.Swing:
                rb.linearVelocity = attackDirection * swingLungeSpeed;
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
    
    void StartWindUp()
    {
        currentState = State.WindUp;
        stateTimer = windUpTime;
        rb.linearVelocity = Vector2.zero;
        
        // Lock onto player direction and telegraph
        attackDirection = (player.position - transform.position).normalized;
        spriteRenderer.color = telegraphColor;
    }
    
    void StartSwing()
    {
        currentState = State.Swing;
        stateTimer = swingDuration;
        hasHitPlayer = false;
        
        spriteRenderer.color = originalColor;
    }
    
    void StartRecovery()
    {
        currentState = State.Recovery;
        stateTimer = recoveryTime;
        rb.linearVelocity = Vector2.zero;
    }
    
    // --- BUFF SYSTEM ---
    
    void BuffNearbyEnemies()
    {
        if (hasBuffedEnemies) return;
        hasBuffedEnemies = true;
        
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        
        foreach (GameObject enemy in enemies)
        {
            // Skip self
            if (enemy == gameObject) continue;
            
            // Skip already buffed enemies
            if (enemy.GetComponent<EnemyBuff>() != null) continue;
            
            // Only buff enemies within the room (buffRadius)
            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            if (distance <= buffRadius)
            {
                EnemyBuff buff = enemy.AddComponent<EnemyBuff>();
                buff.Initialize(chosenEnemyBuff);
                activeBuffs.Add(buff);
            }
        }
    }
    
    // --- COLLISION ---
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (currentState == State.Stunned) return;
        
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                if (currentState == State.Swing && !hasHitPlayer)
                {
                    // Heavy swing — 2 damage
                    playerHealth.TakeDamage(swingDamage, transform.position);
                    hasHitPlayer = true;
                }
                else if (currentState != State.Swing)
                {
                    // Regular bump — 1 damage
                    playerHealth.TakeDamage(contactDamage, transform.position);
                }
            }
        }
        
        // Stop lunge if we hit a wall
        if (currentState == State.Swing && collision.gameObject.CompareTag("Wall"))
        {
            StartRecovery();
        }
    }
    
    void OnCollisionStay2D(Collision2D collision)
    {
        if (currentState == State.Stunned) return;
        
        if (currentState == State.Swing && !hasHitPlayer)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(swingDamage, transform.position);
                    hasHitPlayer = true;
                }
            }
        }
    }
    
    // --- INTERFACES ---
    
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
        // Remove all enemy buffs — allies lose their power
        foreach (EnemyBuff buff in activeBuffs)
        {
            if (buff != null)
            {
                buff.RemoveBuff();
            }
        }
        activeBuffs.Clear();
        
        // Reward the player with a random buff
        if (player != null)
        {
            // Remove existing player buff if they already have one
            PlayerBuff existingBuff = player.GetComponent<PlayerBuff>();
            if (existingBuff != null)
            {
                existingBuff.RemoveBuff();
            }
            
            PlayerBuff.BuffType playerBuffType = (PlayerBuff.BuffType)Random.Range(0, 4);
            PlayerBuff newBuff = player.gameObject.AddComponent<PlayerBuff>();
            newBuff.Initialize(playerBuffType);
        }
        
        // Drop loot
        Dropper dropper = GetComponent<Dropper>();
        if (dropper != null)
        {
            dropper.Drop();
        }
        
        Destroy(gameObject);
    }
}