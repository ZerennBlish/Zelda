using UnityEngine;
using System.Collections.Generic;

public class OrcChief : StunnableEnemy
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

    [Header("Enemy Buff")]
    public float buffRadius = 12f;

    private enum State { Wander, Chase, WindUp, Swing, Recovery, Stunned }
    private State currentState = State.Wander;

    private Vector2 wanderDirection;
    private float wanderTimer;
    private float wanderInterval = 2f;

    private float stateTimer;
    private Vector2 attackDirection;
    private bool hasHitPlayer;

    // Buff tracking
    private bool hasBuffedEnemies = false;
    private EnemyBuff.BuffType chosenEnemyBuff;
    private List<EnemyBuff> activeBuffs = new List<EnemyBuff>();

    protected override void Start()
    {
        base.Start();

        // Pick which buff this Chief will give its allies
        chosenEnemyBuff = (EnemyBuff.BuffType)Random.Range(0, 3);

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
        if (RoomManager.Instance == null) return;

        float roomW = RoomManager.Instance.roomWidth;
        float roomH = RoomManager.Instance.roomHeight;

        // Compute THIS chief's own room from its position so a chief near
        // a boundary doesn't accidentally buff the player's room instead.
        Vector2 myRoom = new Vector2(
            Mathf.Round(transform.position.x / roomW),
            Mathf.Round(transform.position.y / roomH)
        );
        Vector2 roomCenter = new Vector2(myRoom.x * roomW, myRoom.y * roomH);

        // Find enemies inside our room only
        Collider2D[] hits = Physics2D.OverlapBoxAll(
            roomCenter,
            new Vector2(roomW, roomH),
            0f);

        int buffedCount = 0;

        foreach (Collider2D col in hits)
        {
            if (col == null) continue;
            if (!col.CompareTag("Enemy")) continue;
            if (col.gameObject == gameObject) continue;

            // Skip already-buffed enemies (e.g. another chief got there first)
            if (col.GetComponent<EnemyBuff>() != null) continue;

            EnemyBuff buff = col.gameObject.AddComponent<EnemyBuff>();
            buff.Initialize(chosenEnemyBuff);
            activeBuffs.Add(buff);
            buffedCount++;
        }

        // Only mark spent if at least one enemy actually got buffed; lets us
        // retry on the next Wander → Chase transition if all candidates were
        // already buffed by another source the first time.
        if (buffedCount > 0)
        {
            hasBuffedEnemies = true;
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

    protected override void OnStunEnter()
    {
        currentState = State.Stunned;
    }

    protected override void OnStunExit()
    {
        currentState = State.Wander;
    }

    // Reward the player with a random buff. Ally-buff cleanup is handled
    // in OnDestroy so any death path (Die, scene unload, etc.) clears them.
    protected override void OnDie()
    {
        if (player != null)
        {
            // Don't pre-remove. PlayerBuff.Initialize handles same-type
            // dedupe and leaves different-type buffs alone, so an ongoing
            // Power/Speed buff survives a Heal/Resupply award.
            PlayerBuff.BuffType playerBuffType = (PlayerBuff.BuffType)Random.Range(0, 4);
            PlayerBuff newBuff = player.gameObject.AddComponent<PlayerBuff>();
            newBuff.Initialize(playerBuffType);
        }
    }

    void OnDestroy()
    {
        // Remove buffs from any still-living allies regardless of how the
        // chief died (Die, scene unload, room cleanup, etc.).
        if (activeBuffs != null)
        {
            foreach (EnemyBuff buff in activeBuffs)
            {
                if (buff != null)
                {
                    buff.RemoveBuff();
                }
            }
            activeBuffs.Clear();
        }
    }
}
