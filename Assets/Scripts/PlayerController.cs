using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    
    [Header("Arrow")]
    public GameObject arrowPrefab;
    public float fireRate = 0.3f;
    public int maxArrows = 30;
    public int currentArrows = 30;
    
    [Header("Boomerang")]
    public GameObject boomerangPrefab;
    
    [Header("Bomb")]
    public GameObject bombPrefab;
    public int maxBombs = 10;
    public int currentBombs = 10;
    
    [Header("Grappling Hook")]
    public GameObject grapplingHookPrefab;
    public float grapplePullSpeed = 12f;
    
    [Header("Mount")]
    public Sprite horseSprite;
    public float mountedSpeedMultiplier = 2.5f;
    public int ramDamageToEnemy = 1;
    public int ramDamageToPlayer = 1;
    public float ramCooldown = 0.5f;
    
    [Header("References")]
    public Sword sword;
    public ArrowUI arrowUI;
    public BombUI bombUI;
    
    private Rigidbody2D rb;
    private Collider2D playerCollider;
    private SpriteRenderer spriteRenderer;
    private Vector2 movement;
    private Vector2 facingDirection = Vector2.down;
    private float nextFireTime = 0f;
    
    private bool boomerangOut = false;
    
    // Grapple state
    private bool isGrappling = false;
    private bool isPullingPlayer = false;
    private Vector3 grappleTarget;
    private GameObject grappleHookInstance;
    
    // Mount state
    private bool isMounted = false;
    private Sprite normalSprite;
    private float ramTimer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Store the normal player sprite so we can swap back
        normalSprite = spriteRenderer.sprite;
        
        if (PlayerPrefs.HasKey("SavedArrows"))
        {
            currentArrows = PlayerPrefs.GetInt("SavedArrows");
        }
        else
        {
            currentArrows = maxArrows;
        }
        
        if (PlayerPrefs.HasKey("SavedBombs"))
        {
            currentBombs = PlayerPrefs.GetInt("SavedBombs");
        }
        else
        {
            currentBombs = maxBombs;
        }
        
        UpdateArrowUI();
        UpdateBombUI();
    }

    void Update()
    {
        // Ram cooldown ticks regardless of state
        if (ramTimer > 0f)
        {
            ramTimer -= Time.deltaTime;
        }
        
        // --- GRAPPLE STATE: block all other input ---
        if (isGrappling)
        {
            if (isPullingPlayer)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position, 
                    grappleTarget, 
                    grapplePullSpeed * Time.deltaTime
                );
                
                if (Vector2.Distance(transform.position, grappleTarget) < 0.1f)
                {
                    transform.position = grappleTarget;
                    EndGrapple();
                }
            }
            
            return;
        }
        
        // --- NORMAL INPUT ---
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
        
        if (movement.magnitude > 1f)
        {
            movement = movement.normalized;
        }
        
        if (movement != Vector2.zero)
        {
            facingDirection = movement.normalized;
        }
        
        // --- MOUNT TOGGLE - M / Back button ---
        if (Input.GetKeyDown(KeyCode.M) || Input.GetKeyDown(KeyCode.JoystickButton6))
        {
            if (isMounted)
            {
                Dismount();
            }
            else
            {
                Mount();
            }
        }
        
        // --- WEAPON CONTROLS (blocked while mounted) ---
        if (!isMounted)
        {
            // Sword - Space / Left Click / X button
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.JoystickButton2))
            {
                if (sword != null)
                {
                    sword.Swing(facingDirection);
                }
            }
            
            // Boomerang - E / Right Click / Y button
            if ((Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.JoystickButton3)) && !boomerangOut)
            {
                ThrowBoomerang();
            }
            
            // Arrow - F / Middle Click / RB (hold to rapid fire)
            if ((Input.GetKey(KeyCode.F) || Input.GetMouseButton(2) || Input.GetKey(KeyCode.JoystickButton5)) && Time.time >= nextFireTime)
            {
                Shoot();
                nextFireTime = Time.time + fireRate;
            }
            
            // Bomb - Q / LB
            if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.JoystickButton4))
            {
                PlaceBomb();
            }
            
            // Grappling Hook - G / A button
            if (Input.GetKeyDown(KeyCode.G) || Input.GetKeyDown(KeyCode.JoystickButton0))
            {
                FireGrapple();
            }
        }
    }

    void FixedUpdate()
    {
        if (isGrappling) return;
        
        float currentSpeed = isMounted ? moveSpeed * mountedSpeedMultiplier : moveSpeed;
        rb.MovePosition(rb.position + movement * currentSpeed * Time.fixedDeltaTime);
    }

    // --- MOUNT METHODS ---
    
    void Mount()
    {
        if (horseSprite == null) return;
        
        isMounted = true;
        
        // Swap player sprite to horse
        spriteRenderer.sprite = horseSprite;
        
        // Hide sword
        if (sword != null)
        {
            SpriteRenderer swordSR = sword.GetComponent<SpriteRenderer>();
            if (swordSR != null)
            {
                swordSR.enabled = false;
            }
        }
    }
    
    void Dismount()
    {
        isMounted = false;
        
        // Swap back to normal player sprite
        spriteRenderer.sprite = normalSprite;
        
        // Show sword again
        if (sword != null)
        {
            SpriteRenderer swordSR = sword.GetComponent<SpriteRenderer>();
            if (swordSR != null)
            {
                swordSR.enabled = true;
            }
        }
    }
    
    // --- MOUNT COLLISION (ram damage) ---
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isMounted) return;
        if (ramTimer > 0f) return;
        
        if (collision.gameObject.CompareTag("Enemy"))
        {
            IDamageable damageable = collision.gameObject.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(ramDamageToEnemy);
            }
            
            PlayerHealth playerHealth = GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(ramDamageToPlayer);
            }
            
            ramTimer = ramCooldown;
        }
    }
    
    void OnCollisionStay2D(Collision2D collision)
    {
        if (!isMounted) return;
        if (ramTimer > 0f) return;
        
        if (collision.gameObject.CompareTag("Enemy"))
        {
            IDamageable damageable = collision.gameObject.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(ramDamageToEnemy);
            }
            
            PlayerHealth playerHealth = GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(ramDamageToPlayer);
            }
            
            ramTimer = ramCooldown;
        }
    }

    // --- GRAPPLE METHODS ---
    
    void FireGrapple()
    {
        if (grapplingHookPrefab == null) return;
        
        isGrappling = true;
        isPullingPlayer = false;
        
        movement = Vector2.zero;
        rb.linearVelocity = Vector2.zero;
        
        Vector3 spawnPos = transform.position + (Vector3)(facingDirection * 0.5f);
        grappleHookInstance = Instantiate(grapplingHookPrefab, spawnPos, Quaternion.identity);
        grappleHookInstance.GetComponent<GrapplingHook>().Initialize(transform, facingDirection, this);
    }
    
    public void GrappleLatched(Vector3 targetPosition)
    {
        isPullingPlayer = true;
        grappleTarget = targetPosition;
        
        if (playerCollider != null)
        {
            playerCollider.enabled = false;
        }
        
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;
    }
    
    public void GrappleGrabbed()
    {
        isPullingPlayer = false;
    }
    
    public void GrappleMissed()
    {
        isGrappling = false;
        isPullingPlayer = false;
        grappleHookInstance = null;
    }
    
    public void GrappleFinished()
    {
        isGrappling = false;
        isPullingPlayer = false;
        grappleHookInstance = null;
    }
    
    void EndGrapple()
    {
        if (playerCollider != null)
        {
            playerCollider.enabled = true;
        }
        
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.linearVelocity = Vector2.zero;
        
        isGrappling = false;
        isPullingPlayer = false;
        
        if (grappleHookInstance != null)
        {
            Destroy(grappleHookInstance);
            grappleHookInstance = null;
        }
    }
    
    // --- WEAPON METHODS ---

    void Shoot()
    {
        if (arrowPrefab == null) return;
        if (currentArrows <= 0) return;
        
        currentArrows--;
        UpdateArrowUI();
        
        Vector3 spawnPos = transform.position + (Vector3)(facingDirection * 0.5f);
        GameObject arrow = Instantiate(arrowPrefab, spawnPos, Quaternion.identity);
        arrow.GetComponent<Arrow>().SetDirection(facingDirection);
    }
    
    void ThrowBoomerang()
    {
        if (boomerangPrefab == null) return;
        
        boomerangOut = true;
        
        Vector3 spawnPos = transform.position + (Vector3)(facingDirection * 0.5f);
        GameObject boomerang = Instantiate(boomerangPrefab, spawnPos, Quaternion.identity);
        boomerang.GetComponent<Boomerang>().Initialize(transform, facingDirection, this);
    }
    
    void PlaceBomb()
    {
        if (bombPrefab == null) return;
        if (currentBombs <= 0) return;
        
        currentBombs--;
        PlayerPrefs.SetInt("SavedBombs", currentBombs);
        PlayerPrefs.Save();
        UpdateBombUI();
        
        Instantiate(bombPrefab, transform.position, Quaternion.identity);
    }
    
    public void BoomerangReturned()
    {
        boomerangOut = false;
    }
    
    // --- INVENTORY METHODS ---
    
    public void AddArrows(int amount)
    {
        currentArrows += amount;
        if (currentArrows > maxArrows)
        {
            currentArrows = maxArrows;
        }
        UpdateArrowUI();
    }
    
    public void AddBombs(int amount)
    {
        currentBombs += amount;
        if (currentBombs > maxBombs)
        {
            currentBombs = maxBombs;
        }
        PlayerPrefs.SetInt("SavedBombs", currentBombs);
        PlayerPrefs.Save();
        UpdateBombUI();
    }
    
    void UpdateArrowUI()
    {
        if (arrowUI != null)
        {
            arrowUI.UpdateCount(currentArrows);
        }
    }
    
    void UpdateBombUI()
    {
        if (bombUI != null)
        {
            bombUI.UpdateCount(currentBombs);
        }
    }
    
    public Vector2 GetFacingDirection()
    {
        return facingDirection;
    }
    
    public bool IsMounted()
    {
        return isMounted;
    }
}