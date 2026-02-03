using UnityEngine;

public class Bat : MonoBehaviour, IDamageable
{
    [Header("Movement")]
    public float wanderSpeed = 2f;
    public float chaseSpeed = 3f;
    public float chaseRange = 5f;
    public float directionChangeTime = 1f;
    
    [Header("Combat")]
    public int damage = 1;
    public int health = 1;
    
    private Transform player;
    private Vector2 moveDirection;
    private float directionTimer;
    
    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        
        PickRandomDirection();
    }
    
    void Update()
    {
        if (player == null) return;
        
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        if (distanceToPlayer <= chaseRange)
        {
            // Chase player
            moveDirection = (player.position - transform.position).normalized;
            transform.position += (Vector3)moveDirection * chaseSpeed * Time.deltaTime;
        }
        else
        {
            // Wander randomly
            directionTimer -= Time.deltaTime;
            if (directionTimer <= 0)
            {
                PickRandomDirection();
            }
            transform.position += (Vector3)moveDirection * wanderSpeed * Time.deltaTime;
        }
    }
    
    void PickRandomDirection()
    {
        moveDirection = Random.insideUnitCircle.normalized;
        directionTimer = directionChangeTime;
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage, transform.position);
            }
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