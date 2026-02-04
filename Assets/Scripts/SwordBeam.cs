using UnityEngine;

public class SwordBeam : MonoBehaviour
{
    public float speed = 12f;
    public float lifetime = 2f;
    public int damage = 1;
    
    [Header("Blink Effect")]
    public float blinkRate = 0.05f;
    
    private Vector2 direction;
    private SpriteRenderer spriteRenderer;
    private float blinkTimer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime, Space.World);
        
        // Blink on and off as it flies
        blinkTimer -= Time.deltaTime;
        if (blinkTimer <= 0)
        {
            blinkTimer = blinkRate;
            spriteRenderer.enabled = !spriteRenderer.enabled;
        }
    }

    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;
        
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            // ShieldKnight needs attack direction for blocking
            ShieldKnight knight = other.GetComponent<ShieldKnight>();
            if (knight != null)
            {
                knight.TakeDamage(damage, transform.position);
                Destroy(gameObject);
                return;
            }
            
            // All other enemies use the interface
            IDamageable damageable = other.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage);
                Destroy(gameObject);
                return;
            }
        }
        
        if (other.CompareTag("Destructible"))
        {
            Destructible destructible = other.GetComponent<Destructible>();
            if (destructible != null)
            {
                destructible.TakeDamage(damage);
            }
            Destroy(gameObject);
        }
        
        if (other.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
        
        if (other.CompareTag("CrackedWall"))
        {
            Destroy(gameObject);
        }
    }
}