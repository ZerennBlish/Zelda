using UnityEngine;

public class FireBolt : MonoBehaviour
{
    public float speed = 8f;
    public float lifetime = 2f;
    public int damage = 1;
    
    [Header("Fire Trail (activated by Book)")]
    public bool hasFireTrail = false;
    public GameObject fireTrailPrefab;
    public float trailSpawnRate = 0.1f;
    
    private Vector2 direction;
    private float trailTimer;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime, Space.World);
        
        // Spawn fire trail segments if Book upgrade is active
        if (hasFireTrail && fireTrailPrefab != null)
        {
            trailTimer -= Time.deltaTime;
            if (trailTimer <= 0)
            {
                trailTimer = trailSpawnRate;
                Instantiate(fireTrailPrefab, transform.position, Quaternion.identity);
            }
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
            ShieldKnight knight = other.GetComponent<ShieldKnight>();
            if (knight != null)
            {
                knight.TakeDamage(damage, transform.position);
                
                HitFlash flash = other.GetComponent<HitFlash>();
                if (flash != null) flash.Flash();
                
                Destroy(gameObject);
                return;
            }
            
            IDamageable damageable = other.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage);
                
                HitFlash flash = other.GetComponent<HitFlash>();
                if (flash != null) flash.Flash();
                
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