using UnityEngine;

public class GoblinArcher : MonoBehaviour, IStunnable, IDamageable
{
    [Header("Movement")]
    public float patrolSpeed = 1f;
    public float fleeSpeed = 3f;
    public float patrolRadius = 2f;
    public float patrolChangeTime = 2f;
    
    [Header("Combat")]
    public float detectRange = 6f;
    public float fleeRange = 4f;
    public float fireRate = 1.5f;
    public GameObject arrowPrefab;
    
    [Header("Stats")]
    public int health = 2;
    public int contactDamage = 1;
    
    [Header("Stun")]
    public Color stunColor = new Color(0.5f, 0.5f, 1f, 1f);
    
    private enum State { Patrol, Combat, Stunned }
    private State currentState = State.Patrol;
    
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Transform player;
    private Vector2 wanderDirection;
    private float wanderTimer;
    private float nextFireTime;
    private Vector3 startPosition;
    private Dropper dropper;
    private bool isDead = false;
    
    private float stunTimer;
    private Color originalColor;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
        dropper = GetComponent<Dropper>();
        startPosition = transform.position;
        
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        
        PickNewPatrolDirection();
    }

    void Update()
    {
        if (isDead) return;
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
        
        if (distanceToPlayer <= detectRange)
        {
            currentState = State.Combat;
            
            if (distanceToPlayer <= fleeRange)
            {
                Vector2 fleeDirection = (transform.position - player.position).normalized;
                rb.linearVelocity = fleeDirection * fleeSpeed;
                UpdateFacing(-fleeDirection);
            }
            else
            {
                rb.linearVelocity = Vector2.zero;
            }
            
            if (Time.time >= nextFireTime)
            {
                Shoot();
                nextFireTime = Time.time + fireRate;
            }
        }
        else
        {
            currentState = State.Patrol;
            
            wanderTimer -= Time.deltaTime;
            if (wanderTimer <= 0f)
            {
                PickNewPatrolDirection();
            }
            
            rb.linearVelocity = wanderDirection * patrolSpeed;
            UpdateFacing(wanderDirection);
            
            float distanceFromStart = Vector2.Distance(transform.position, startPosition);
            if (distanceFromStart > patrolRadius)
            {
                Vector2 returnDirection = (startPosition - transform.position).normalized;
                rb.linearVelocity = returnDirection * patrolSpeed;
            }
        }
    }

    void PickNewPatrolDirection()
    {
        float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        wanderDirection = new Vector2(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle));
        wanderTimer = patrolChangeTime;
    }
    
    void UpdateFacing(Vector2 direction)
    {
        if (direction.x != 0)
        {
            spriteRenderer.flipX = direction.x < 0;
        }
    }
    
    void Shoot()
    {
        if (arrowPrefab == null) return;
        
        Vector2 direction = (player.position - transform.position).normalized;
        Vector3 spawnPos = transform.position + (Vector3)(direction * 0.5f);
        
        GameObject arrow = Instantiate(arrowPrefab, spawnPos, Quaternion.identity);
        arrow.GetComponent<EnemyArrow>().SetDirection(direction);
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (currentState == State.Stunned) return;
        
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(contactDamage, transform.position);
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
    
    public void TakeDamage(int damage)
    {
        health -= damage;
        
        if (health <= 0)
        {
            Die();
        }
    }
    
    void Die()
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        
        if (dropper != null)
        {
            dropper.Drop();
        }
        
        Destroy(gameObject);
    }
}