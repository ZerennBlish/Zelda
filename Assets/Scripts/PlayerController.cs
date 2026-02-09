using UnityEngine;
using System.Collections.Generic;

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
    
    [Header("Wand")]
    public GameObject fireBoltPrefab;
    public GameObject fireTrailPrefab;
    public float wandFireRate = 0.5f;
    
    [Header("Mount")]
    public Sprite horseSprite;
    public float mountedSpeedMultiplier = 2.5f;
    public int ramDamageToEnemy = 1;
    public int ramDamageToPlayer = 1;
    public float ramCooldown = 0.5f;
    
    [Header("Item Unlocks")]
    public bool hasBoomerang = false;
    public bool hasBombs = false;
    public bool hasGrapple = false;
    public bool hasWand = false;
    public bool hasBook = false;
    
    [Header("References")]
    public Melee melee;
    public ArrowUI arrowUI;
    public BombUI bombUI;
    
    private Rigidbody2D rb;
    private Collider2D playerCollider;
    private SpriteRenderer spriteRenderer;
    private Camera mainCamera;
    private Vector2 movement;
    private Vector2 facingDirection = Vector2.down;
    private float nextFireTime = 0f;
    private float nextWandFireTime = 0f;
    
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
    
    // Shooting animation state
    private bool isShooting = false;
    private float shootAnimTimer = 0f;
    public float shootAnimDuration = 0.3f;
    
    // Active item slot system
    public enum SubWeapon { Boomerang, Bombs, Grapple, Wand }
    private List<SubWeapon> unlockedWeapons = new List<SubWeapon>();
    private int currentWeaponIndex = 0;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        mainCamera = Camera.main;
        
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
        
        // Load item unlocks
        hasBoomerang = PlayerPrefs.GetInt("HasBoomerang", 0) == 1;
        hasBombs = PlayerPrefs.GetInt("HasBombs", 0) == 1;
        hasGrapple = PlayerPrefs.GetInt("HasGrapple", 0) == 1;
        hasWand = PlayerPrefs.GetInt("HasWand", 0) == 1;
        hasBook = PlayerPrefs.GetInt("HasBook", 0) == 1;
        
        // Load last equipped weapon
        currentWeaponIndex = PlayerPrefs.GetInt("EquippedWeaponIndex", 0);
        
        RebuildWeaponList();
        
        UpdateArrowUI();
        UpdateBombUI();
    }

    void Update()
    {
        if (DialogueBox.IsActive || ShopUI.IsActive || PauseManager.IsPaused) return;

        if (ramTimer > 0f)
        {
            ramTimer -= Time.deltaTime;
        }
        
        // Shooting animation countdown
        if (shootAnimTimer > 0f)
        {
            shootAnimTimer -= Time.deltaTime;
            if (shootAnimTimer <= 0f)
            {
                isShooting = false;
            }
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
        
        // --- MOVEMENT INPUT ---
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
        
        if (movement.magnitude > 1f)
        {
            movement = movement.normalized;
        }
        
        // --- AIM DIRECTION follows mouse ---
        if (mainCamera != null)
        {
            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0f;
            Vector2 aimDirection = ((Vector2)mouseWorldPos - (Vector2)transform.position);
            
            if (aimDirection.magnitude > 0.1f)
            {
                facingDirection = aimDirection.normalized;
            }
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
        
        // --- CYCLE SUB-WEAPON: Mouse Wheel / I key ---
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f)
        {
            CycleWeapon(1);
        }
        else if (scroll < 0f)
        {
            CycleWeapon(-1);
        }
        
        if (Input.GetKeyDown(KeyCode.I))
        {
            CycleWeapon(1);
        }
        
        // --- WEAPON CONTROLS (blocked while mounted) ---
        if (!isMounted)
        {
            // Melee - Space / Left Click / X button
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.JoystickButton2))
            {
                if (melee != null)
                {
                    melee.Swing(facingDirection);
                }
            }
            
            // Arrow - F / Middle Click / RB (hold to rapid fire)
            if ((Input.GetKey(KeyCode.F) || Input.GetMouseButton(2) || Input.GetKey(KeyCode.JoystickButton5)) && Time.time >= nextFireTime)
            {
                Shoot();
                nextFireTime = Time.time + fireRate;
            }
            
            // Active sub-weapon - E / Right Click / Y button
            if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.JoystickButton3))
            {
                UseActiveWeapon();
            }
        }
    }

    void FixedUpdate()
    {
        if (isGrappling) return;
        
        float currentSpeed = isMounted ? moveSpeed * mountedSpeedMultiplier : moveSpeed;
        rb.MovePosition(rb.position + movement * currentSpeed * Time.fixedDeltaTime);
    }
    
    // --- ACTIVE ITEM SLOT ---
    
    void RebuildWeaponList()
    {
        unlockedWeapons.Clear();
        
        if (hasBoomerang) unlockedWeapons.Add(SubWeapon.Boomerang);
        if (hasBombs) unlockedWeapons.Add(SubWeapon.Bombs);
        if (hasGrapple) unlockedWeapons.Add(SubWeapon.Grapple);
        if (hasWand) unlockedWeapons.Add(SubWeapon.Wand);
        
        // Keep index in bounds
        if (unlockedWeapons.Count > 0)
        {
            currentWeaponIndex = currentWeaponIndex % unlockedWeapons.Count;
        }
        else
        {
            currentWeaponIndex = 0;
        }
    }
    
    void CycleWeapon(int direction)
    {
        if (unlockedWeapons.Count <= 1) return;
        
        currentWeaponIndex += direction;
        
        // Wrap around
        if (currentWeaponIndex >= unlockedWeapons.Count)
        {
            currentWeaponIndex = 0;
        }
        else if (currentWeaponIndex < 0)
        {
            currentWeaponIndex = unlockedWeapons.Count - 1;
        }
        
        // Save equipped weapon
        PlayerPrefs.SetInt("EquippedWeaponIndex", currentWeaponIndex);
        PlayerPrefs.Save();
        
        Debug.Log("Equipped: " + GetActiveWeapon());
    }
    
    void UseActiveWeapon()
    {
        if (unlockedWeapons.Count == 0) return;
        
        SubWeapon active = unlockedWeapons[currentWeaponIndex];
        
        switch (active)
        {
            case SubWeapon.Boomerang:
                if (!boomerangOut)
                {
                    ThrowBoomerang();
                }
                break;
                
            case SubWeapon.Bombs:
                PlaceBomb();
                break;
                
            case SubWeapon.Grapple:
                FireGrapple();
                break;
                
            case SubWeapon.Wand:
                FireWand();
                break;
        }
    }
    
    /// <summary>
    /// Returns the currently equipped sub-weapon.
    /// Returns Boomerang as default if nothing unlocked,
    /// but UseActiveWeapon guards against empty list.
    /// </summary>
    public SubWeapon GetActiveWeapon()
    {
        if (unlockedWeapons.Count == 0) return SubWeapon.Boomerang;
        return unlockedWeapons[currentWeaponIndex];
    }
    
    /// <summary>
    /// Returns the count of unlocked sub-weapons.
    /// Useful for UI to know whether to show the weapon slot.
    /// </summary>
    public int GetUnlockedWeaponCount()
    {
        return unlockedWeapons.Count;
    }

    // --- MOUNT METHODS ---
    
    void Mount()
    {
        if (horseSprite == null) return;
        
        isMounted = true;
        spriteRenderer.sprite = horseSprite;
        
        if (melee != null)
        {
            melee.gameObject.SetActive(false);
        }
    }
    
    void Dismount()
    {
        isMounted = false;
        spriteRenderer.sprite = normalSprite;
        
        if (melee != null)
        {
            melee.gameObject.SetActive(true);
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
            
            HitFlash flash = collision.gameObject.GetComponent<HitFlash>();
            if (flash != null) flash.Flash();
            
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
            
            HitFlash flash = collision.gameObject.GetComponent<HitFlash>();
            if (flash != null) flash.Flash();
            
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
        
        isShooting = true;
        shootAnimTimer = shootAnimDuration;
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
    
    void FireWand()
    {
        if (fireBoltPrefab == null) return;
        if (Time.time < nextWandFireTime) return;
        
        nextWandFireTime = Time.time + wandFireRate;
        
        Vector3 spawnPos = transform.position + (Vector3)(facingDirection * 0.5f);
        GameObject bolt = Instantiate(fireBoltPrefab, spawnPos, Quaternion.identity);
        
        FireBolt fireBolt = bolt.GetComponent<FireBolt>();
        if (fireBolt != null)
        {
            fireBolt.SetDirection(facingDirection);
            
            // Book upgrade: +1 damage and enable fire trail
            if (hasBook)
            {
                fireBolt.damage += 1;
                fireBolt.hasFireTrail = true;
                fireBolt.fireTrailPrefab = fireTrailPrefab;
            }
        }
    }
    
    public void BoomerangReturned()
    {
        boomerangOut = false;
    }
    
    // --- ITEM UNLOCK ---
    
    public void UnlockItem(string itemName)
    {
        switch (itemName)
        {
            case "Boomerang":
                hasBoomerang = true;
                PlayerPrefs.SetInt("HasBoomerang", 1);
                break;
            case "Bombs":
                hasBombs = true;
                PlayerPrefs.SetInt("HasBombs", 1);
                currentBombs = maxBombs;
                UpdateBombUI();
                break;
            case "Grapple":
                hasGrapple = true;
                PlayerPrefs.SetInt("HasGrapple", 1);
                break;
            case "Wand":
                hasWand = true;
                PlayerPrefs.SetInt("HasWand", 1);
                break;
            case "Book":
                hasBook = true;
                PlayerPrefs.SetInt("HasBook", 1);
                break;
        }
        PlayerPrefs.Save();
        
        // Rebuild so the new weapon appears in the rotation
        RebuildWeaponList();
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
    
    public bool IsShooting()
    {
        return isShooting;
    }
}