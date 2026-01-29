using UnityEngine;

public class SkeletonMage : MonoBehaviour, IStunnable
{
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
        Vector2 randomOffset = Random.insideUnitCircle.normalized * teleportRange;
        Vector3 newPosition = transform.position + new Vector3(randomOffset.x, randomOffset.y, 0);
        
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