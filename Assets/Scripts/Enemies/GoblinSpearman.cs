using UnityEngine;

public class GoblinSpearman : StunnableEnemy
{
    [Header("Movement")]
    public float wanderSpeed = 1f;
    public float chaseSpeed = 2f;
    public float chaseRange = 5f;
    public float attackRange = 2.5f;

    [Header("Attack")]
    public float pullbackTime = 0.5f;
    public float chargeSpeed = 8f;
    public float chargeDuration = 0.4f;
    public float recoveryTime = 1f;
    public int damage = 1;

    [Header("Telegraph")]
    public Color telegraphColor = Color.red;
    public float pullbackDistance = 0.5f;

    private enum State { Wander, Chase, Pullback, Charging, Recovery, Stunned }
    private State currentState = State.Wander;

    private Vector2 wanderDirection;
    private float wanderTimer;
    private float wanderInterval = 2f;

    private float stateTimer;
    private Vector2 chargeDirection;
    private Vector3 pullbackStartPos;
    private bool hasHitPlayer;

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
                if (distanceToPlayer < attackRange)
                {
                    StartPullback();
                }
                else if (distanceToPlayer > chaseRange * 1.5f)
                {
                    currentState = State.Wander;
                }
                break;

            case State.Pullback:
                Pullback();
                stateTimer -= Time.deltaTime;
                if (stateTimer <= 0)
                {
                    StartCharge();
                }
                break;

            case State.Charging:
                Charge();
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

    void StartPullback()
    {
        currentState = State.Pullback;
        stateTimer = pullbackTime;

        chargeDirection = (player.position - transform.position).normalized;
        pullbackStartPos = transform.position;

        spriteRenderer.color = telegraphColor;
    }

    void Pullback()
    {
        Vector2 startPos = pullbackStartPos;
        Vector2 endPos = startPos - chargeDirection * pullbackDistance;
        Vector2 targetPos = Vector2.Lerp(startPos, endPos, 1 - (stateTimer / pullbackTime));
        rb.MovePosition(targetPos);
        rb.linearVelocity = Vector2.zero;
    }

    void StartCharge()
    {
        currentState = State.Charging;
        stateTimer = chargeDuration;
        hasHitPlayer = false;

        spriteRenderer.color = originalColor;
    }

    void Charge()
    {
        rb.linearVelocity = chargeDirection * chargeSpeed;
    }

    void StartRecovery()
    {
        currentState = State.Recovery;
        stateTimer = recoveryTime;
        rb.linearVelocity = Vector2.zero;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (currentState == State.Stunned) return;

        if (currentState == State.Charging && !hasHitPlayer)
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

        if (currentState == State.Charging && collision.gameObject.CompareTag("Wall"))
        {
            StartRecovery();
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (currentState == State.Stunned) return;

        if (currentState == State.Charging && !hasHitPlayer)
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

    protected override void OnStunEnter()
    {
        currentState = State.Stunned;
    }

    protected override void OnStunExit()
    {
        currentState = State.Wander;
    }
}
