using UnityEngine;

public class Mummy : StunnableEnemy
{
    [Header("Spin")]
    public float spinSpeed = 720f;

    [Header("Shooting")]
    public GameObject projectilePrefab;
    public float fireRate = 0.05f;
    public float projectileSpeed = 6f;

    [Header("Burrow")]
    public float aboveGroundTime = 4f;
    public float undergroundTime = 2f;
    public float burrowRadius = 3f;
    [SerializeField] private LayerMask wallLayerMask;

    [Header("Contact")]
    public int contactDamage = 1;

    private enum State { Spinning, Burrowing, Underground, Emerging, Stunned }
    private State currentState = State.Spinning;

    private Collider2D col;

    private float stateTimer;
    private float fireTimer;
    private float currentRotation;

    protected override void Start()
    {
        base.Start();
        col = GetComponent<Collider2D>();
        stateTimer = aboveGroundTime;
    }

    void Update()
    {
        if (isDead) return;
        switch (currentState)
        {
            case State.Spinning:
                Spin();
                Shoot();
                stateTimer -= Time.deltaTime;
                if (stateTimer <= 0)
                {
                    StartBurrowing();
                }
                break;

            case State.Burrowing:
                stateTimer -= Time.deltaTime;
                float burrowProgress = 1 - (stateTimer / 0.5f);
                transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, burrowProgress);
                if (stateTimer <= 0)
                {
                    GoUnderground();
                }
                break;

            case State.Underground:
                stateTimer -= Time.deltaTime;
                if (stateTimer <= 0)
                {
                    StartEmerging();
                }
                break;

            case State.Emerging:
                stateTimer -= Time.deltaTime;
                float emergeProgress = 1 - (stateTimer / 0.5f);
                transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, emergeProgress);

                // Enable collider once mummy is at least half-visible
                if (col != null && emergeProgress >= 0.5f && !col.enabled)
                {
                    col.enabled = true;
                }

                if (stateTimer <= 0)
                {
                    currentState = State.Spinning;
                    stateTimer = aboveGroundTime;
                }
                break;

            case State.Stunned:
                TickStun();
                break;
        }
    }

    void Spin()
    {
        // Modulo 360 prevents float drift over long encounters
        currentRotation = (currentRotation + spinSpeed * Time.deltaTime) % 360f;

        if (rb != null)
        {
            rb.MoveRotation(currentRotation);
        }
        else
        {
            transform.rotation = Quaternion.Euler(0, 0, currentRotation);
        }
    }

    void Shoot()
    {
        fireTimer -= Time.deltaTime;
        if (fireTimer <= 0)
        {
            fireTimer = fireRate;

            float angle = currentRotation * Mathf.Deg2Rad;
            Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

            if (projectilePrefab != null)
            {
                GameObject proj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
                MummyProjectile mp = proj.GetComponent<MummyProjectile>();
                if (mp != null)
                {
                    mp.SetDirection(direction, projectileSpeed);
                }
            }
        }
    }

    void StartBurrowing()
    {
        currentState = State.Burrowing;
        stateTimer = 0.5f;
        col.enabled = false;
    }

    void GoUnderground()
    {
        currentState = State.Underground;
        stateTimer = undergroundTime;
        spriteRenderer.enabled = false;

        // Validated re-emerge: try a few random offsets, fall back to current
        // position if every candidate would put us inside a wall.
        const int maxAttempts = 10;
        Vector2 newPosition = transform.position;

        for (int i = 0; i < maxAttempts; i++)
        {
            Vector2 offset = Random.insideUnitCircle * burrowRadius;
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

    void StartEmerging()
    {
        currentState = State.Emerging;
        stateTimer = 0.5f;
        spriteRenderer.enabled = true;
        // Collider stays disabled until the emerge case in Update gates it on
        // half-visible scale, so the player can't take contact damage from a
        // barely-visible mummy.
        transform.localScale = Vector3.zero;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (currentState == State.Underground ||
            currentState == State.Burrowing ||
            currentState == State.Stunned ||
            currentState == State.Emerging)
            return;

        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(contactDamage, transform.position);
            }
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (currentState == State.Underground ||
            currentState == State.Burrowing ||
            currentState == State.Stunned ||
            currentState == State.Emerging)
            return;

        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(contactDamage, transform.position);
            }
        }
    }

    // Mummy can't be stunned during the phases where it isn't visible/present.
    protected override bool CanBeStunned()
    {
        return currentState != State.Underground &&
               currentState != State.Burrowing &&
               currentState != State.Emerging;
    }

    protected override void OnStunEnter()
    {
        currentState = State.Stunned;
    }

    protected override void OnStunExit()
    {
        currentState = State.Spinning;
    }

    // Aligned with CanBeStunned guards — burrowing/underground/emerging mummies
    // can't be hit (visual: not present or fading in).
    public override void TakeDamage(int amount)
    {
        if (isDead) return;
        if (currentState == State.Underground ||
            currentState == State.Burrowing ||
            currentState == State.Emerging)
            return;

        health -= amount;
        if (health <= 0)
        {
            Die();
        }
    }
}
