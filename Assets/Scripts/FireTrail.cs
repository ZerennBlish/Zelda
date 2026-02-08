using UnityEngine;

public class FireTrail : MonoBehaviour
{
    public float lifetime = 1.5f;
    public int damage = 1;
    public float damageInterval = 0.5f;
    public float fadeStartTime = 1f;
    
    private SpriteRenderer spriteRenderer;
    private float timer;
    private float damageTimer;
    private Color originalColor;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        timer = lifetime;
        damageTimer = 0f;
        
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    void Update()
    {
        timer -= Time.deltaTime;
        
        // Fade out near end of life
        if (timer <= fadeStartTime && spriteRenderer != null)
        {
            float alpha = timer / fadeStartTime;
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
        }
        
        if (timer <= 0)
        {
            Destroy(gameObject);
        }
        
        if (damageTimer > 0)
        {
            damageTimer -= Time.deltaTime;
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (damageTimer > 0) return;
        
        if (other.CompareTag("Enemy"))
        {
            IDamageable damageable = other.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage);
                
                HitFlash flash = other.GetComponent<HitFlash>();
                if (flash != null) flash.Flash();
                
                damageTimer = damageInterval;
            }
        }
        
        if (other.CompareTag("Destructible"))
        {
            Destructible destructible = other.GetComponent<Destructible>();
            if (destructible != null)
            {
                destructible.TakeDamage(damage);
                damageTimer = damageInterval;
            }
        }
    }
}