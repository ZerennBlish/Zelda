using UnityEngine;

public class SpearBeam : MonoBehaviour
{
    public float speed = 16f;
    public float lifetime = 1.5f;
    public int damage = 3;
    
    [Header("Blink Effect")]
    public float blinkRate = 0.04f;
    
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
                
                HitFlash flash = other.GetComponent<HitFlash>();
                if (flash != null) flash.Flash();
                
                // Spear laser pierces through — no Destroy here
                return;
            }
            
            // All other enemies use the interface
            IDamageable damageable = other.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage);
                
                HitFlash flash = other.GetComponent<HitFlash>();
                if (flash != null) flash.Flash();
                
                // Pierces through enemies
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