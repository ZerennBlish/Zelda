using UnityEngine;

public class PlayerClass : MonoBehaviour
{
    public enum ClassTier { Archer, Swordsman, Spearman, Paladin }
    
    [Header("Current Class")]
    public ClassTier currentClass = ClassTier.Archer;
    
    [Header("Class Sprites (idle frame per class)")]
    public Sprite archerSprite;
    public Sprite swordsmanSprite;
    public Sprite spearmanSprite;
    public Sprite paladinSprite;
    
    [Header("Melee Config Per Class")]
    public float archerArc = 90f;
    public float archerReach = 0.5f;
    public int archerDamage = 1;
    
    public float swordsmanArc = 120f;
    public float swordsmanReach = 0.7f;
    public int swordsmanDamage = 2;
    
    public float spearmanArc = 30f;
    public float spearmanReach = 1.2f;
    public int spearmanDamage = 3;
    
    public float paladinArc = 180f;
    public float paladinReach = 0.9f;
    public int paladinDamage = 4;
    
    // Half damage flag — true once Swordsman or above
    private bool hasArmor = false;
    
    private SpriteRenderer spriteRenderer;
    private Melee melee;
    private PlayerHealth playerHealth;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        melee = GetComponentInChildren<Melee>();
        playerHealth = GetComponent<PlayerHealth>();
        
        // Apply whatever class is set (handles both fresh start and loaded saves)
        ApplyClass();
    }
    
    /// <summary>
    /// Upgrades to the next class tier. Call this from story triggers,
    /// item pickups, NPCs — whatever grants the upgrade.
    /// </summary>
    public void UpgradeClass()
    {
        // Already at max tier
        if (currentClass == ClassTier.Paladin)
        {
            Debug.Log("Already at max class: Paladin");
            return;
        }
        
        // Move to next tier
        currentClass = currentClass + 1;
        Debug.Log("Upgraded to: " + currentClass);
        
        // Grant upgrade bonuses based on what we just became
        switch (currentClass)
        {
            case ClassTier.Swordsman:
                // First upgrade — gain armor (half damage)
                hasArmor = true;
                Debug.Log("Gained armor! Damage taken is halved.");
                break;
                
            case ClassTier.Spearman:
                // Second upgrade — gain a heart
                if (playerHealth != null)
                {
                    playerHealth.IncreaseMaxHealth(1);
                    Debug.Log("Gained +1 max heart!");
                }
                break;
                
            case ClassTier.Paladin:
                // Third upgrade — gain a heart
                if (playerHealth != null)
                {
                    playerHealth.IncreaseMaxHealth(1);
                    Debug.Log("Gained +1 max heart!");
                }
                break;
        }
        
        ApplyClass();
    }
    
    /// <summary>
    /// Sets a specific class directly. Useful for loading saves
    /// or debug testing.
    /// </summary>
    public void SetClass(ClassTier tier)
    {
        currentClass = tier;
        
        // Armor applies at Swordsman and above
        hasArmor = (currentClass >= ClassTier.Swordsman);
        
        ApplyClass();
    }
    
    /// <summary>
    /// Applies all changes for the current class — sprite, melee config.
    /// </summary>
    void ApplyClass()
    {
        // Swap sprite
        if (spriteRenderer != null)
        {
            switch (currentClass)
            {
                case ClassTier.Archer:
                    if (archerSprite != null) spriteRenderer.sprite = archerSprite;
                    break;
                case ClassTier.Swordsman:
                    if (swordsmanSprite != null) spriteRenderer.sprite = swordsmanSprite;
                    break;
                case ClassTier.Spearman:
                    if (spearmanSprite != null) spriteRenderer.sprite = spearmanSprite;
                    break;
                case ClassTier.Paladin:
                    if (paladinSprite != null) spriteRenderer.sprite = paladinSprite;
                    break;
            }
        }
        
        // Configure melee
        if (melee != null)
        {
            switch (currentClass)
            {
                case ClassTier.Archer:
                    melee.swingArc = archerArc;
                    melee.hitboxDistance = archerReach;
                    melee.damage = archerDamage;
                    break;
                case ClassTier.Swordsman:
                    melee.swingArc = swordsmanArc;
                    melee.hitboxDistance = swordsmanReach;
                    melee.damage = swordsmanDamage;
                    break;
                case ClassTier.Spearman:
                    melee.swingArc = spearmanArc;
                    melee.hitboxDistance = spearmanReach;
                    melee.damage = spearmanDamage;
                    break;
                case ClassTier.Paladin:
                    melee.swingArc = paladinArc;
                    melee.hitboxDistance = paladinReach;
                    melee.damage = paladinDamage;
                    break;
            }
            
            Debug.Log("Melee set: arc=" + melee.swingArc + " reach=" + melee.hitboxDistance + " damage=" + melee.damage);
        }
    }
    
    /// <summary>
    /// Called by PlayerHealth to check if damage should be halved.
    /// Returns true if the player has armor (Swordsman+).
    /// </summary>
    public bool HasArmor()
    {
        return hasArmor;
    }
    
    public ClassTier GetCurrentClass()
    {
        return currentClass;
    }
}