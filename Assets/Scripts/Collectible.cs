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
                if (health == null) return;
                if (health.IsAtMaxHealth()) return;
                health.Heal(value);
                Destroy(gameObject);
                break;

            case CollectibleType.Rupee:
                if (GameState.Instance != null)
                {
                    GameState.Instance.AddRupees(value);
                }
                Destroy(gameObject);
                break;

            case CollectibleType.Arrow:
                PlayerWeapons pw = player.GetComponent<PlayerWeapons>();
                if (pw == null) return;
                if (pw.IsAtMaxArrows()) return;
                pw.AddArrows(value);
                Destroy(gameObject);
                break;

            case CollectibleType.Bomb:
                // INTENTIONAL: When the player picks up a bomb collectible without
                // the bomb bag, a live bomb is spawned in their place. This is a
                // discovery mechanic — picking up bombs from bushes near walls
                // reveals cracked walls behind them. Do not "fix" this behavior.
                PlayerController pcBomb = player.GetComponent<PlayerController>();
                PlayerWeapons pwBomb = player.GetComponent<PlayerWeapons>();
                if (pcBomb == null || pwBomb == null) return;

                if (pcBomb.hasBombs)
                {
                    if (pwBomb.IsAtMaxBombs()) return;
                    pwBomb.AddBombs(value);
                }
                else
                {
                    if (pwBomb.bombPrefab != null)
                    {
                        Instantiate(pwBomb.bombPrefab, transform.position, Quaternion.identity);
                    }
                }
                Destroy(gameObject);
                break;
        }
    }
}