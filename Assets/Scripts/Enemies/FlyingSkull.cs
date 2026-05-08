using UnityEngine;

public class FlyingSkull : StunnableEnemy
{
    [Header("Movement")]
    public float wanderSpeed = 2f;
    public float swoopSpeed = 6f;
    public float pullbackSpeed = 2f;
    public float changeDirectionInterval = 1.5f;
    public float chaseRange = 6f;

    [Header("Swoop")]
    public float pullbackDuration = 0.3f;
    public float swoopDuration = 0.5f;
    public float swoopCooldown = 1.5f;

    [Header("Wall Detection")]
    [SerializeField] private LayerMask wallLayerMask;

    [Header("Combat")]
    public int damage = 1;

    private enum State { Wander, Pullback, Swoop, Cooldown, Stunned }
    private State currentState = State.Wander;

    private Vector2 roomCenter;
    private Vector2 moveDirection;
    private Vector2 swoopDirection;
    private float directionTimer;
    private float stateTimer;
    private bool hasHitPlayer;

    protected override void Start()
    {
        base.Start();

        // Single source of truth: RoomManager owns the room dimensions.
        if (RoomManager.Instance != null)
        {
            float roomW = RoomManager.Instance.roomWidth;
            float roomH = RoomManager.Instance.roomHeight;
            roomCenter = new Vector2(
                Mathf.Round(transform.position.x / roomW) * roomW,
                Mathf.Round(transform.position.y / roomH) * roomH
            );
        }
        else
        {
            roomCenter = transform.position;
        }

        PickNewDirection();
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
                    StartPullback();
                }
                break;

            case State.Pullback:
                Pullback();
                stateTimer -= Time.deltaTime;
                if (stateTimer <= 0)
                {
                    StartSwoop();
                }
                break;

            case State.Swoop:
                Swoop();
                stateTimer -= Time.deltaTime;
                if (stateTimer <= 0)
                {
                    StartCooldown();
                }
                break;

            case State.Cooldown:
                Wander();
                stateTimer -= Time.deltaTime;
                if (stateTimer <= 0)
                {
                    currentState = State.Wander;
                }
                break;
        }

        CheckWallCollision();
        ClampToRoom();
    }

    void CheckWallCollision()
    {
        if (rb == null) return;
        if (rb.linearVelocity.sqrMagnitude < 0.01f) return;

        Vector2 nextPos = (Vector2)transform.position + rb.linearVelocity * Time.deltaTime;
        Collider2D wallHit = Physics2D.OverlapCircle(nextPos, 0.4f, wallLayerMask);

        if (wallHit == null) return;

        // State-aware response: Pullback/Swoop abort to Cooldown so the skull
        // doesn't keep ramming the wall; Wander/Cooldown just pick a new
        // direction. Velocity is overridden this frame to avoid one frame of
        // stale wall-pushing motion.
        if (currentState == State.Pullback || currentState == State.Swoop)
        {
            StartCooldown();
        }
        else
        {
            PickNewDirection();
        }

        rb.linearVelocity = moveDirection * wanderSpeed;
    }

    void Wander()
    {
        directionTimer -= Time.deltaTime;
        if (directionTimer <= 0)
        {
            PickNewDirection();
        }

        rb.linearVelocity = moveDirection * wanderSpeed;
        UpdateFacing(moveDirection);
    }

    void PickNewDirection()
    {
        float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        moveDirection = new Vector2(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle));
        directionTimer = changeDirectionInterval;
    }

    void StartPullback()
    {
        currentState = State.Pullback;
        stateTimer = pullbackDuration;

        swoopDirection = (player.position - transform.position).normalized;
        UpdateFacing(swoopDirection);
    }

    void Pullback()
    {
        rb.linearVelocity = -swoopDirection * pullbackSpeed;
    }

    void StartSwoop()
    {
        currentState = State.Swoop;
        stateTimer = swoopDuration;
        hasHitPlayer = false;
    }

    void Swoop()
    {
        rb.linearVelocity = swoopDirection * swoopSpeed;
    }

    void StartCooldown()
    {
        currentState = State.Cooldown;
        stateTimer = swoopCooldown;
        PickNewDirection();
    }

    void UpdateFacing(Vector2 direction)
    {
        if (direction.x != 0)
        {
            spriteRenderer.flipX = direction.x < 0;
        }
    }

    void ClampToRoom()
    {
        if (RoomManager.Instance == null) return;

        float roomW = RoomManager.Instance.roomWidth;
        float roomH = RoomManager.Instance.roomHeight;

        float minX = roomCenter.x - (roomW / 2f) + 0.5f;
        float maxX = roomCenter.x + (roomW / 2f) - 0.5f;
        float minY = roomCenter.y - (roomH / 2f) + 0.5f;
        float maxY = roomCenter.y + (roomH / 2f) - 0.5f;

        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);
        transform.position = pos;

        if (currentState == State.Wander || currentState == State.Cooldown)
        {
            if (transform.position.x == minX || transform.position.x == maxX ||
                transform.position.y == minY || transform.position.y == maxY)
            {
                PickNewDirection();
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (currentState == State.Stunned) return;

        if (other.CompareTag("Player") && currentState == State.Swoop && !hasHitPlayer)
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage, transform.position);
                hasHitPlayer = true;
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
