using UnityEngine;

public class Arrow : MonoBehaviour
{
    public float speed = 10f;
    public float lifetime = 2f;
    public int damage = 1;
    private Vector2 direction;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime, Space.World);
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
            // Special case: ShieldKnight needs attack direction for blocking
            ShieldKnight knight = other.GetComponent<ShieldKnight>();
            if (knight != null)
            {
                knight.TakeDamage(damage, transform.position);
                
                // Flash on hit
                HitFlash flash = other.GetComponent<HitFlash>();
                if (flash != null) flash.Flash();
                
                Destroy(gameObject);
                return;
            }
            Debug.Log("Arrow hit " + other.name + " - attempting flash");
            // All other enemies use the interface
            IDamageable damageable = other.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage);
                
                // Flash on hit
                HitFlash flash = other.GetComponent<HitFlash>();
                if (flash != null) flash.Flash();
                
                Destroy(gameObject);
                return;
            }
        }
        
        if (other.CompareTag("Destructible"))
        {
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