using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public GameObject arrowPrefab;
    public float fireRate = 0.3f;
    public Sword sword;
    
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
        
        Vector3 mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;
        Vector2 aimDirection = (mousePos - transform.position).normalized;
        
        if (Input.GetMouseButton(0))
        {
            facingDirection = aimDirection;
        }
        else if (movement != Vector2.zero)
        {
            facingDirection = movement.normalized;
        }
        
        // Shoot arrow - left click, space, or X button on controller
        if ((Input.GetMouseButton(0) || Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.JoystickButton3)) 
            && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
        
        // Swing sword - right click, F key, or Y button on controller
        if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.JoystickButton2))
        {
            if (sword != null)
            {
                sword.Swing(facingDirection);
            }
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