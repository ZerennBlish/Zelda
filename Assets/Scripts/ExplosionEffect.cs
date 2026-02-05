using UnityEngine;

public class ExplosionEffect : MonoBehaviour
{
    public float lifetime = 0.5f;
    public float explosionRadius = 2f;
    public int explosionDamage = 2;

    void Start()
    {
        DealDamage();
        Destroy(gameObject, lifetime);
    }
    
    void DealDamage()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                PlayerHealth playerHealth = hit.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(explosionDamage, transform.position);
                }
            }
            
            if (hit.CompareTag("Enemy"))
            {
                IDamageable damageable = hit.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(explosionDamage);
                    
                    HitFlash flash = hit.GetComponent<HitFlash>();
                    if (flash != null) flash.Flash();
                }
            }
            
            if (hit.CompareTag("Destructible"))
            {
                Destructible destructible = hit.GetComponent<Destructible>();
                if (destructible != null)
                {
                    destructible.TakeDamage(explosionDamage);
                }
            }
            
            if (hit.CompareTag("CrackedWall"))
            {
                CrackedWall crackedWall = hit.GetComponent<CrackedWall>();
                if (crackedWall != null)
                {
                    crackedWall.TakeDamage(explosionDamage);
                }
            }
        }
    }
}