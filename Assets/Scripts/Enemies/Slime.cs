using UnityEngine;

public class Slime : StunnableEnemy
{
    [Header("Movement")]
    public float wanderSpeed = 1f;
    public float chaseSpeed = 2f;
    public float chaseRange = 5f;

    [Header("Combat")]
    public int damage = 1;
    public float damageCooldown = 1f;

    private enum State { Wander, Chase, Stunned }
    private State currentState = State.Wander;

    private Vector2 wanderDirection;
    private float wanderTimer;
    private float wanderInterval = 2f;
    private float damageTimer;

    protected override void Start()
    {
        base.Start();
        PickNewWanderDirection();
    }

    void Update()
    {
        if (player == null) return;
        if (TickStun()) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer < chaseRange)
        {
            currentState = State.Chase;
        }
        else
        {
            currentState = State.Wander;
        }

        switch (currentState)
        {
            case State.Wander:
                Wander();
                break;
            case State.Chase:
                Chase();
                break;
        }

        if (damageTimer > 0)
        {
            damageTimer -= Time.deltaTime;
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

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (currentState == State.Stunned) return;

        if (collision.gameObject.CompareTag("Player") && damageTimer <= 0)
        {
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage, transform.position);
                damageTimer = damageCooldown;
            }
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (currentState == State.Stunned) return;

        if (collision.gameObject.CompareTag("Player") && damageTimer <= 0)
        {
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage, transform.position);
                damageTimer = damageCooldown;
            }
        }
    }

    protected override void OnStunEnter()
    {
        currentState = State.Stunned;
    }

    protected override void OnStunExit()
    {
        currentState = State.Wander;
    }
}
