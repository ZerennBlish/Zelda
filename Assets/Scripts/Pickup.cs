using UnityEngine;

public class Pickup : MonoBehaviour
{
    public enum PickupType { Heart, Rupee }
    public PickupType pickupType;
    public int value = 1;
    public float bobSpeed = 2f;
    public float bobHeight = 0.1f;
    
    private Vector3 startPos;
    
    void Start()
    {
        startPos = transform.position;
    }
    
    void Update()
    {
        // Gentle bobbing animation
        float newY = startPos.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(startPos.x, newY, startPos.z);
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            switch (pickupType)
            {
                case PickupType.Heart:
                    PlayerHealth health = other.GetComponent<PlayerHealth>();
                    if (health != null && health.currentHealth < health.maxHealth)
                    {
                        health.Heal(value);
                        Destroy(gameObject);
                    }
                    break;
                    
                case PickupType.Rupee:
                    GameState.Instance.AddRupees(value);
                    Destroy(gameObject);
                    break;
            }
        }
    }
}