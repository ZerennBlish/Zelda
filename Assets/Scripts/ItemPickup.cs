using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public enum ItemType
    {
        Boomerang,
        BombBag,
        Grapple,
        Wand,
        Book,
        ClassUpgrade
    }
    
    [Header("Item Settings")]
    public ItemType itemType = ItemType.Boomerang;
    
    [Header("Class Upgrade (only used if ItemType is ClassUpgrade)")]
    public PlayerClass.ClassTier upgradeTo = PlayerClass.ClassTier.Swordsman;
    
    [Header("Visual")]
    public float bobSpeed = 2f;
    public float bobHeight = 0.15f;
    public float glowPulseSpeed = 2f;
    public float glowPulseMin = 0.7f;
    
    private Vector3 startPosition;
    private SpriteRenderer spriteRenderer;
    private bool collected = false;

    void Start()
    {
        startPosition = transform.position;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // Bob up and down
        float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(startPosition.x, newY, transform.position.z);
        
        // Gentle brightness pulse so it stands out
        if (spriteRenderer != null)
        {
            float pulse = Mathf.Lerp(glowPulseMin, 1f, (Mathf.Sin(Time.time * glowPulseSpeed) + 1f) / 2f);
            spriteRenderer.color = new Color(pulse, pulse, pulse, 1f);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (collected) return;
        
        if (other.CompareTag("Player"))
        {
            collected = true;
            
            PlayerController player = other.GetComponent<PlayerController>();
            PlayerClass playerClass = other.GetComponent<PlayerClass>();
            
            switch (itemType)
            {
                case ItemType.Boomerang:
                    if (player != null) player.UnlockItem("Boomerang");
                    Debug.Log("Picked up: Boomerang");
                    break;
                    
                case ItemType.BombBag:
                    if (player != null) player.UnlockItem("Bombs");
                    Debug.Log("Picked up: Bomb Bag");
                    break;
                    
                case ItemType.Grapple:
                    if (player != null) player.UnlockItem("Grapple");
                    Debug.Log("Picked up: Grappling Hook");
                    break;
                    
                case ItemType.Wand:
                    if (player != null) player.UnlockItem("Wand");
                    Debug.Log("Picked up: Wand");
                    break;
                    
                case ItemType.Book:
                    if (player != null) player.UnlockItem("Book");
                    Debug.Log("Picked up: Book");
                    break;
                    
                case ItemType.ClassUpgrade:
                    if (playerClass != null)
                    {
                        playerClass.SetClass(upgradeTo);
                        Debug.Log("Class upgrade: " + upgradeTo);
                    }
                    break;
            }
            
            Destroy(gameObject);
        }
    }
}