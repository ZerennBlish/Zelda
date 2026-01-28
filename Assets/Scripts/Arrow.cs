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
            // Check for BoomShroom
            BoomShroom shroom = other.GetComponent<BoomShroom>();
            if (shroom != null)
            {
                shroom.Explode();
                Destroy(gameObject);
                return;
            }
            
            // Check for Slime
            Slime slime = other.GetComponent<Slime>();
            if (slime != null)
            {
                slime.TakeDamage(damage);
                Destroy(gameObject);
                return;
            }
            
            // Check for GoblinArcher
            GoblinArcher archer = other.GetComponent<GoblinArcher>();
            if (archer != null)
            {
                archer.TakeDamage(damage);
                Destroy(gameObject);
                return;
            }
            
            // Check for Destructible (bushes, etc)
            Destructible destructible = other.GetComponent<Destructible>();
            if (destructible != null)
            {
                destructible.TakeDamage(damage);
            }
            else
            {
                Destroy(other.gameObject);
            }
            Destroy(gameObject);
        }
        
        if (other.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}