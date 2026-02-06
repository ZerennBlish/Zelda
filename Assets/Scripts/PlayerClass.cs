using UnityEngine;

public class PlayerClass : MonoBehaviour
{
    public enum ClassTier { Archer, Swordsman, Spearman, Paladin }
    
    [Header("Current Class")]
    public ClassTier currentClass = ClassTier.Archer;
    
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
    
    [Header("Beam Prefabs Per Class")]
    public GameObject swordsmanBeamPrefab;
    public GameObject spearmanBeamPrefab;
    public GameObject paladinBeamPrefab;
    // Archer has no beam — leave null
    
    // Half damage flag — true once Swordsman or above
    private bool hasArmor = false;
    
    private Melee melee;
    private PlayerHealth playerHealth;

    void Start()
    {
        melee = GetComponentInChildren<Melee>();
        playerHealth = GetComponent<PlayerHealth>();
        
        // Load saved class if one exists
        if (PlayerPrefs.HasKey("SavedClassTier"))
        {
            int savedTier = PlayerPrefs.GetInt("SavedClassTier");
            currentClass = (ClassTier)savedTier;
            hasArmor = (currentClass >= ClassTier.Swordsman);
        }
        
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
                hasArmor = true;
                Debug.Log("Gained armor! Damage taken is halved.");
                break;
                
            case ClassTier.Spearman:
                if (playerHealth != null)
                {
                    playerHealth.IncreaseMaxHealth(1);
                    Debug.Log("Gained +1 max heart!");
                }
                break;
                
            case ClassTier.Paladin:
                if (playerHealth != null)
                {
                    playerHealth.IncreaseMaxHealth(1);
                    Debug.Log("Gained +1 max heart!");
                }
                break;
        }
        
        ApplyClass();
        SaveClass();
    }
    
    /// <summary>
    /// Sets a specific class directly. Useful for loading saves
    /// or debug testing.
    /// </summary>
    public void SetClass(ClassTier tier)
    {
        currentClass = tier;
        hasArmor = (currentClass >= ClassTier.Swordsman);
        
        ApplyClass();
        SaveClass();
    }
    
    void SaveClass()
    {
        PlayerPrefs.SetInt("SavedClassTier", (int)currentClass);
        PlayerPrefs.Save();
    }
    
    /// <summary>
    /// Applies melee config and beam for the current class.
    /// Sprite animation is handled by PlayerAnimator which reads our class directly.
    /// </summary>
    void ApplyClass()
    {
        // Configure melee and beam
        if (melee != null)
        {
            switch (currentClass)
            {
                case ClassTier.Archer:
                    melee.swingArc = archerArc;
                    melee.hitboxDistance = archerReach;
                    melee.damage = archerDamage;
                    melee.swordBeamPrefab = null;
                    break;
                case ClassTier.Swordsman:
                    melee.swingArc = swordsmanArc;
                    melee.hitboxDistance = swordsmanReach;
                    melee.damage = swordsmanDamage;
                    melee.swordBeamPrefab = swordsmanBeamPrefab;
                    break;
                case ClassTier.Spearman:
                    melee.swingArc = spearmanArc;
                    melee.hitboxDistance = spearmanReach;
                    melee.damage = spearmanDamage;
                    melee.swordBeamPrefab = spearmanBeamPrefab;
                    break;
                case ClassTier.Paladin:
                    melee.swingArc = paladinArc;
                    melee.hitboxDistance = paladinReach;
                    melee.damage = paladinDamage;
                    melee.swordBeamPrefab = paladinBeamPrefab;
                    break;
            }
            
            Debug.Log("Class applied: " + currentClass + " | arc=" + melee.swingArc + " reach=" + melee.hitboxDistance + " damage=" + melee.damage + " beam=" + (melee.swordBeamPrefab != null ? melee.swordBeamPrefab.name : "none"));
        }
    }
    
    public bool HasArmor()
    {
        return hasArmor;
    }
    
    public ClassTier GetCurrentClass()
    {
        return currentClass;
    }
}