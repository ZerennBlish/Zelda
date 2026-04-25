using UnityEngine;

public class SkeletonMage : MonoBehaviour, IStunnable, IDamageable{
    [Header("Movement")]
    public float patrolSpeed = 1f;
    public float patrolChangeTime = 2f;
    
    [Header("Combat")]
    public float detectRange = 7f;
    public float attackRange = 5f;
    public float fireRate = 2f;
    public GameObject projectilePrefab;
    
    [Header("Teleport")]
    public float teleportCooldown = 3f;
    public float teleportRange = 3f;
    public float dangerRange = 2f;
    [SerializeField] private LayerMask wallLayerMask;
    
    [Header("Health")]
    public int health = 2;
    
    [Header("Stun")]
    public Color stunColor = new Color(0.5f, 0.5f, 1f, 1f);
    
    private enum State { Patrol, Combat, Stunned }
    private State currentState = State.Patrol;
    
    private Transform player;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    
    private Vector2 patrolDirection;
    private float patrolTimer;
    private float nextFireTime;
    private float nextTeleportTime;
    
    private float stunTimer;
    private Color originalColor;
    private bool isDead = false;

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
        
        PickNewPatrolDirection();
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
                currentState = State.Patrol;
                spriteRenderer.color = originalColor;
                EnemyBuff buff = GetComponent<EnemyBuff>();
                if (buff != null) buff.ReapplyTint();
            }
            return;
        }
        
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        if (distanceToPlayer < detectRange)
        {
            currentState = State.Combat;
            Combat(distanceToPlayer);
        }
        else
        {
            currentState = State.Patrol;
            Patrol();
        }
    }
    
    void Patrol()
    {
        patrolTimer -= Time.deltaTime;
        if (patrolTimer <= 0)
        {
            PickNewPatrolDirection();
        }
        
        rb.linearVelocity = patrolDirection * patrolSpeed;
        UpdateFacing(patrolDirection);
    }
    
    void PickNewPatrolDirection()
    {
        float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        patrolDirection = new Vector2(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle));
        patrolTimer = patrolChangeTime;
    }
    
    void Combat(float distanceToPlayer)
    {
        rb.linearVelocity = Vector2.zero;
        
        Vector2 directionToPlayer = (player.position - transform.position).normalized;
        UpdateFacing(directionToPlayer);
        
        if (distanceToPlayer < dangerRange && Time.time >= nextTeleportTime)
        {
            Teleport();
            nextTeleportTime = Time.time + teleportCooldown;
        }
        
        if (distanceToPlayer <= attackRange && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }
    
    void Teleport()
    {
        const int maxAttempts = 10;
        Vector2 newPosition = transform.position;

        for (int i = 0; i < maxAttempts; i++)
        {
            Vector2 offset = Random.insideUnitCircle.normalized * teleportRange;
            Vector2 candidate = (Vector2)transform.position + offset;

            Collider2D blocker = Physics2D.OverlapCircle(candidate, 0.5f, wallLayerMask);
            if (blocker == null)
            {
                newPosition = candidate;
                break;
            }
        }

        transform.position = newPosition;
    }
    
    void Shoot()
    {
        if (projectilePrefab == null) return;
        
        Vector2 direction = (player.position - transform.position).normalized;
        Vector3 spawnPos = transform.position + (Vector3)(direction * 0.5f);
        
        Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
    }
    
    void UpdateFacing(Vector2 direction)
    {
        if (direction.x != 0)
        {
            spriteRenderer.flipX = direction.x < 0;
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
        if (isDead) return;
        health -= amount;

        if (health <= 0)
        {
            Die();
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