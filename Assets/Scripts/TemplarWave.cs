using UnityEngine;

public class TemplarWave : MonoBehaviour
{
    public float speed = 10f;
    public float lifetime = 1.5f;
    public int damage = 4;
    
    [Header("Growth")]
    public float growthRate = 1.5f;
    public float maxScale = 2f;
    
    [Header("Blink Effect")]
    public float blinkRate = 0.06f;
    
    private Vector2 direction;
    private SpriteRenderer spriteRenderer;
    private float blinkTimer;
    private Vector3 startScale;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        startScale = transform.localScale;
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime, Space.World);
        
        // Wave grows as it travels — starts tight, expands outward
        float newScaleY = Mathf.Min(startScale.y * maxScale, transform.localScale.y + growthRate * Time.deltaTime);
        transform.localScale = new Vector3(transform.localScale.x, newScaleY, transform.localScale.z);
        
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
                
                HitFlash flash = other.GetComponent<HitFlash>();
                if (flash != null) flash.Flash();
                
                // Wave passes through — hits everything in its path
                return;
            }
            
            // All other enemies use the interface
            IDamageable damageable = other.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage);
                
                HitFlash flash = other.GetComponent<HitFlash>();
                if (flash != null) flash.Flash();
                
                // Passes through enemies
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