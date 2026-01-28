using UnityEngine;

public class MagicProjectile : MonoBehaviour
{
    public float speed = 6f;
    public float homingStrength = 2f;
    public float lifetime = 5f;
    public int damage = 1;
    
    private Transform player;
    private Vector2 moveDirection;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            moveDirection = (player.position - transform.position).normalized;
        }
        
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        if (player != null)
        {
            Vector2 toPlayer = (player.position - transform.position).normalized;
            moveDirection = Vector2.Lerp(moveDirection, toPlayer, homingStrength * Time.deltaTime).normalized;
        }
        
        rb.linearVelocity = moveDirection * speed;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }
            Destroy(gameObject);
        }
        else if (other.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}