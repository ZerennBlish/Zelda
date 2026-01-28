using UnityEngine;

public class GoblinArcher : MonoBehaviour
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
    
    private Rigidbody2D rb;
    private Transform player;
    private Vector2 wanderDirection;
    private float wanderTimer;
    private float nextFireTime;
    private Vector3 startPosition;
    private Dropper dropper;
    private bool isDead = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        dropper = GetComponent<Dropper>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        startPosition = transform.position;
        PickNewPatrolDirection();
    }

    void Update()
    {
        if (isDead) return;
        
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        if (distanceToPlayer <= detectRange)
        {
            // Player detected
            if (distanceToPlayer <= fleeRange)
            {
                // Too close - flee away from player
                Vector2 fleeDirection = (transform.position - player.position).normalized;
                rb.linearVelocity = fleeDirection * fleeSpeed;
            }
            else
            {
                // Good range - stop and shoot
                rb.linearVelocity = Vector2.zero;
            }
            
            // Shoot at player
            if (Time.time >= nextFireTime)
            {
                Shoot();
                nextFireTime = Time.time + fireRate;
            }
        }
        else
        {
            // Patrol small area around start position
            wanderTimer -= Time.deltaTime;
            if (wanderTimer <= 0f)
            {
                PickNewPatrolDirection();
            }
            
            rb.linearVelocity = wanderDirection * patrolSpeed;
            
            // Stay near start position
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
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(contactDamage);
            }
        }
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