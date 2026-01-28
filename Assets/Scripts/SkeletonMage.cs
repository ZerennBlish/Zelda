using UnityEngine;

public class SkeletonMage : MonoBehaviour
{
    [Header("Movement")]
    public float patrolSpeed = 1f;
    public float patrolRadius = 2f;
    public float detectRange = 6f;
    public float teleportRange = 3f;
    
    [Header("Teleport")]
    public float minTeleportDistance = 4f;
    public float maxTeleportDistance = 6f;
    public float teleportCooldown = 3f;
    public float fadeDuration = 0.3f;
    
    [Header("Attack")]
    public float fireRate = 2f;
    public GameObject magicProjectilePrefab;
    
    [Header("Health")]
    public int health = 2;
    
    private enum State { Patrol, Attack, Teleporting }
    private State currentState = State.Patrol;
    
    private Transform player;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    
    private Vector2 patrolCenter;
    private Vector2 patrolTarget;
    
    private float nextFireTime = 0f;
    private float nextTeleportTime = 0f;
    
    private bool isFadingOut = false;
    private bool isFadingIn = false;
    private float fadeTimer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        patrolCenter = transform.position;
        
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        
        PickNewPatrolTarget();
    }

    void Update()
    {
        if (player == null) return;
        
        // Handle fading
        if (isFadingOut)
        {
            fadeTimer -= Time.deltaTime;
            SetAlpha(fadeTimer / fadeDuration);
            
            if (fadeTimer <= 0)
            {
                isFadingOut = false;
                DoTeleport();
                isFadingIn = true;
                fadeTimer = fadeDuration;
            }
            return;
        }
        
        if (isFadingIn)
        {
            fadeTimer -= Time.deltaTime;
            SetAlpha(1f - (fadeTimer / fadeDuration));
            
            if (fadeTimer <= 0)
            {
                isFadingIn = false;
                SetAlpha(1f);
                currentState = State.Attack;
            }
            return;
        }
        
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        // Start teleport if player gets too close
        if (distanceToPlayer < teleportRange && Time.time >= nextTeleportTime)
        {
            StartTeleport();
            return;
        }
        
        if (distanceToPlayer < detectRange)
        {
            currentState = State.Attack;
        }
        else
        {
            currentState = State.Patrol;
        }
        
        switch (currentState)
        {
            case State.Patrol:
                Patrol();
                break;
                
            case State.Attack:
                rb.linearVelocity = Vector2.zero;
                FacePlayer();
                TryShoot();
                break;
        }
    }
    
    void Patrol()
    {
        if (Vector2.Distance(transform.position, patrolTarget) < 0.5f)
        {
            PickNewPatrolTarget();
        }
        
        Vector2 direction = (patrolTarget - (Vector2)transform.position).normalized;
        rb.linearVelocity = direction * patrolSpeed;
        
        if (direction.x != 0)
        {
            spriteRenderer.flipX = direction.x < 0;
        }
    }
    
    void PickNewPatrolTarget()
    {
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        patrolTarget = patrolCenter + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * patrolRadius;
    }
    
    void FacePlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        if (direction.x != 0)
        {
            spriteRenderer.flipX = direction.x < 0;
        }
    }
    
    void SetAlpha(float alpha)
    {
        Color c = spriteRenderer.color;
        c.a = alpha;
        spriteRenderer.color = c;
    }
    
    void StartTeleport()
    {
        currentState = State.Teleporting;
        rb.linearVelocity = Vector2.zero;
        isFadingOut = true;
        fadeTimer = fadeDuration;
        nextTeleportTime = Time.time + teleportCooldown;
    }
    
    void DoTeleport()
    {
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float distance = Random.Range(minTeleportDistance, maxTeleportDistance);
        
        Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * distance;
        Vector3 newPos = player.position + new Vector3(offset.x, offset.y, 0);
        
        transform.position = newPos;
        patrolCenter = newPos;
    }
    
    void TryShoot()
    {
        if (Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }
    
    void Shoot()
    {
        if (magicProjectilePrefab == null) return;
        
        Vector2 direction = (player.position - transform.position).normalized;
        Vector3 spawnPos = transform.position + (Vector3)(direction * 0.5f);
        
        Instantiate(magicProjectilePrefab, spawnPos, Quaternion.identity);
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