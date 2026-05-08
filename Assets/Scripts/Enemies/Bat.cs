using UnityEngine;

public class Bat : EnemyBase
{
    [Header("Movement")]
    public float wanderSpeed = 2f;
    public float chaseSpeed = 3f;
    public float chaseRange = 5f;
    public float directionChangeTime = 1f;

    [Header("Combat")]
    public int damage = 1;

    private Vector2 moveDirection;
    private float directionTimer;

    protected override void Start()
    {
        base.Start();
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
            rb.linearVelocity = moveDirection * chaseSpeed;
        }
        else
        {
            // Wander randomly
            directionTimer -= Time.deltaTime;
            if (directionTimer <= 0)
            {
                PickRandomDirection();
            }
            rb.linearVelocity = moveDirection * wanderSpeed;
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
}
