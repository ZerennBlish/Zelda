using UnityEngine;

public class MummyProjectile : MonoBehaviour
{
    public float lifetime = 3f;
    public int damage = 1;
    
    private Vector2 direction;
    private float speed;
    
    void Start()
    {
        Destroy(gameObject, lifetime);
    }
    
    void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime, Space.World);
    }
    
    public void SetDirection(Vector2 dir, float spd)
    {
        direction = dir.normalized;
        speed = spd;
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage, transform.position);
            }
            Destroy(gameObject);
            return;
        }

        if (other.CompareTag("Destructible"))
        {
            Destructible destructible = other.GetComponent<Destructible>();
            if (destructible != null)
            {
                destructible.TakeDamage(1);
            }
            Destroy(gameObject);
            return;
        }

        if (other.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}