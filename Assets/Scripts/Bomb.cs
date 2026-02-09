using UnityEngine;

public class Bomb : MonoBehaviour
{
    [Header("Timing")]
    public float fuseTime = 2.5f;
    public float blinkStartTime = 1.5f;
    public float blinkSpeed = 0.1f;
    
    [Header("Explosion")]
    public GameObject explosionPrefab;
    public float explosionRadius = 2f;
    public int explosionDamage = 2;
    
    private float timer;
    private SpriteRenderer spriteRenderer;
    private bool isBlinking = false;
    private bool hasExploded = false;

    void Start()
    {
        timer = fuseTime;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        timer -= Time.deltaTime;
        
        // Start blinking when close to explosion
        if (timer <= blinkStartTime && !isBlinking)
        {
            isBlinking = true;
            InvokeRepeating("Blink", 0f, blinkSpeed);
        }
        
        // Explode when timer runs out
        if (timer <= 0)
        {
            Explode();
        }
    }
    
    void Blink()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled;
        }
    }
    
    void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;

        // Create explosion effect
        if (explosionPrefab != null)
        {
            GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            ExplosionEffect effect = explosion.GetComponent<ExplosionEffect>();
            if (effect != null)
            {
                effect.explosionRadius = explosionRadius;
                effect.explosionDamage = explosionDamage;
            }
        }
        
        Destroy(gameObject);
    }
}