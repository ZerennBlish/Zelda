using UnityEngine;

public class Collectible : MonoBehaviour
{
    public enum CollectibleType { Heart, Rupee, Arrow, Bomb }
    
    [SerializeField]
    public CollectibleType type = CollectibleType.Heart;
    
    [SerializeField]
    public int value = 1;
    
    [Header("Bobbing")]
    public float bobSpeed = 2f;
    public float bobHeight = 0.1f;
    
    private Vector3 startPosition;
    private bool isCarried = false;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        if (!isCarried)
        {
            float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            transform.position = new Vector3(startPosition.x, newY, transform.position.z);
        }
    }
    
    public void SetCarried(bool carried)
    {
        isCarried = carried;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isCarried) return;
        
        if (other.CompareTag("Player"))
        {
            Collect(other.gameObject);
        }
    }
    
    public void CollectNow(GameObject player)
    {
        Collect(player);
    }
    
    public void Collect(GameObject player)
    {
        switch (type)
        {
            case CollectibleType.Heart:
                PlayerHealth health = player.GetComponent<PlayerHealth>();
                if (health != null)
                {
                    health.Heal(value);
                }
                break;
                
            case CollectibleType.Rupee:
                if (GameState.Instance != null)
                {
                    GameState.Instance.AddRupees(value);
                }
                break;
                
            case CollectibleType.Arrow:
                PlayerController pc = player.GetComponent<PlayerController>();
                if (pc != null)
                {
                    pc.AddArrows(value);
                }
                break;
                
            case CollectibleType.Bomb:
                PlayerController pcBomb = player.GetComponent<PlayerController>();
                if (pcBomb != null)
                {
                    pcBomb.AddBombs(value);
                }
                break;
        }
        
        Destroy(gameObject);
    }
}