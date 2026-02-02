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
    
    [Header("References")]
    public Sword sword;
    public ArrowUI arrowUI;
    public BombUI bombUI;
    
    private Rigidbody2D rb;
    private Vector2 movement;
    private Vector2 facingDirection = Vector2.down;
    private float nextFireTime = 0f;
    
    private bool boomerangOut = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
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
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
        
        if (movement.magnitude > 1f)
        {
            movement = movement.normalized;
        }
        
        // Update facing direction based on movement
        if (movement != Vector2.zero)
        {
            facingDirection = movement.normalized;
        }
        
        // Sword - Space or Left Click
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            if (sword != null)
            {
                sword.Swing(facingDirection);
            }
        }
        
        // Boomerang - E or Right Click
        if ((Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(1)) && !boomerangOut)
        {
            ThrowBoomerang();
        }
        
        // Arrow - F or Middle Click (hold to rapid fire)
        if ((Input.GetKey(KeyCode.F) || Input.GetMouseButton(2)) && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
        
        // Bomb - Q
        if (Input.GetKeyDown(KeyCode.Q))
        {
            PlaceBomb();
        }
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }

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
}