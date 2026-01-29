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
    public KeyCode boomerangKey = KeyCode.E;
    public string boomerangButton = "Fire4";
    
    [Header("References")]
    public Sword sword;
    public ArrowUI arrowUI;
    
    private Rigidbody2D rb;
    private Vector2 movement;
    private Vector2 facingDirection = Vector2.down;
    private float nextFireTime = 0f;
    private Camera mainCam;
    
    private bool boomerangOut = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCam = Camera.main;
        
        currentArrows = maxArrows;
        UpdateArrowUI();
    }

    void Update()
    {
        // Movement input
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
        
        if (movement.magnitude > 1f)
        {
            movement = movement.normalized;
        }
        
        // Aim direction
        Vector3 mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;
        Vector2 aimDirection = (mousePos - transform.position).normalized;
        
        // Update facing based on mouse or movement
        if (Input.GetMouseButton(0) || Input.GetMouseButton(1) || Input.GetMouseButton(2))
        {
            facingDirection = aimDirection;
        }
        else if (movement != Vector2.zero)
        {
            facingDirection = movement.normalized;
        }
        
        // Sword swing (Left Click / F / X button)
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.F) || Input.GetButtonDown("Fire2"))
        {
            if (sword != null)
            {
                sword.Swing(facingDirection);
            }
        }
        
        // Boomerang (Right Click / E / RB)
        if ((Input.GetMouseButtonDown(1) || Input.GetKeyDown(boomerangKey) || Input.GetButtonDown(boomerangButton)) && !boomerangOut)
        {
            ThrowBoomerang();
        }
        
        // Shoot arrow (Middle Click / Space / Y button)
        if ((Input.GetMouseButton(2) || Input.GetKey(KeyCode.Space) || Input.GetButton("Fire1")) 
            && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
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
    
    void UpdateArrowUI()
    {
        if (arrowUI != null)
        {
            arrowUI.UpdateCount(currentArrows);
        }
    }
    
    public Vector2 GetFacingDirection()
    {
        return facingDirection;
    }
}