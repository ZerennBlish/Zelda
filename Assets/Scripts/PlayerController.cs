using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public GameObject arrowPrefab;
    public float fireRate = 0.3f;
    
    private Rigidbody2D rb;
    private Vector2 movement;
    private Vector2 facingDirection = Vector2.down;
    private float nextFireTime = 0f;
    private Camera mainCam;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCam = Camera.main;
    }

    void Update()
    {
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
        
        if (movement.magnitude > 1f)
        {
            movement = movement.normalized;
        }
        
        // Mouse aiming - get direction from player to mouse cursor
        Vector3 mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;
        Vector2 aimDirection = (mousePos - transform.position).normalized;
        
        // Update facing direction based on input type
        if (Input.GetMouseButton(0))
        {
            // Mouse click - aim at cursor
            facingDirection = aimDirection;
        }
        else if (movement != Vector2.zero)
        {
            // Keyboard/controller movement - face movement direction
            facingDirection = movement.normalized;
        }
        
        if ((Input.GetMouseButton(0) || Input.GetKey(KeyCode.Space) || Input.GetButton("Fire1")) 
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
        
        Vector3 spawnPos = transform.position + (Vector3)(facingDirection * 0.5f);
        GameObject arrow = Instantiate(arrowPrefab, spawnPos, Quaternion.identity);
        arrow.GetComponent<Arrow>().SetDirection(facingDirection);
    }
}