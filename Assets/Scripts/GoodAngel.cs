using UnityEngine;

public class GoodAngel : MonoBehaviour
{
    [Header("Gift")]
    public int heartsToGive = 1;
    
    [Header("Visual")]
    public float hoverHeight = 0.2f;
    public float hoverSpeed = 2f;
    
    private Vector3 startPosition;
    private bool hasGivenGift = false;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        // Gentle hover animation
        float newY = startPosition.y + Mathf.Sin(Time.time * hoverSpeed) * hoverHeight;
        transform.position = new Vector3(startPosition.x, newY, transform.position.z);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasGivenGift) return;
        
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.IncreaseMaxHealth(heartsToGive);
                hasGivenGift = true;
                
                // Optional: Add particle effect, sound, or animation here
                
                Destroy(gameObject, 0.5f); // Small delay so player sees what happened
            }
        }
    }
}