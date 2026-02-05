using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 3;
    public int currentHealth;
    public int maxLives = 3;
    public int currentLives;
    public HealthUI healthUI;
    public LivesUI livesUI;
    public GameObject gameOverScreen;
    
    public float invincibilityTime = 2f;
    public float blinkRate = 0.1f;
    private bool isInvincible = false;
    private SpriteRenderer spriteRenderer;
    private PlayerShield playerShield;
    private PlayerClass playerClass;
    
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerShield = GetComponentInChildren<PlayerShield>();
        playerClass = GetComponent<PlayerClass>();
        
        if (PlayerPrefs.HasKey("SavedMaxHealth"))
        {
            maxHealth = PlayerPrefs.GetInt("SavedMaxHealth");
        }
        
        currentHealth = maxHealth;
        
        if (healthUI != null)
        {
            healthUI.UpdateHearts(currentHealth, maxHealth);
        }
        
        if (SaveManager.Instance != null && SaveManager.Instance.HasSaveData())
        {
            currentLives = SaveManager.Instance.GetSavedLives();
        }
        else
        {
            currentLives = maxLives;
        }
        
        if (livesUI != null)
        {
            livesUI.UpdateLives(currentLives);
        }
    }
    
    /// <summary>
    /// Applies armor reduction if player has it (Swordsman+).
    /// Damage of 1 becomes 0 with armor — minimum 1 if original was 2+.
    /// This gives half-heart feel: most hits deal less, big hits still hurt.
    /// </summary>
    int ApplyArmor(int damage)
    {
        if (playerClass != null && playerClass.HasArmor())
        {
            damage = Mathf.Max(1, damage / 2);
        }
        return damage;
    }
    
    // No attack source — skip shield check entirely
    // Used by hazards, environmental damage, or anything without a direction
    public void TakeDamage(int damage)
    {
        if (isInvincible) return;
        
        damage = ApplyArmor(damage);
        
        isInvincible = true;
        
        currentHealth -= damage;
        
        if (healthUI != null)
        {
            healthUI.UpdateHearts(currentHealth, maxHealth);
        }
        
        if (currentHealth <= 0)
        {
            LoseLife();
        }
        else
        {
            StartCoroutine(InvincibilityFrames());
        }
    }
    
    // Has attack source — shield can block if facing the right way
    // Used by enemies, projectiles, anything with a position
    public void TakeDamage(int damage, Vector2 attackSource)
    {
        if (isInvincible) return;
        
        if (playerShield != null && playerShield.BlocksAttackFrom(attackSource))
        {
            return;
        }
        
        damage = ApplyArmor(damage);
        
        isInvincible = true;
        
        currentHealth -= damage;
        
        if (healthUI != null)
        {
            healthUI.UpdateHearts(currentHealth, maxHealth);
        }
        
        if (currentHealth <= 0)
        {
            LoseLife();
        }
        else
        {
            StartCoroutine(InvincibilityFrames());
        }
    }
    
    public void Heal(int amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
        
        if (healthUI != null)
        {
            healthUI.UpdateHearts(currentHealth, maxHealth);
        }
    }
    
    public void IncreaseMaxHealth(int amount)
    {
        maxHealth += amount;
        currentHealth += amount;
        
        if (healthUI != null)
        {
            healthUI.UpdateHearts(currentHealth, maxHealth);
        }
        
        PlayerPrefs.SetInt("SavedMaxHealth", maxHealth);
        PlayerPrefs.Save();
    }
    
    public void DecreaseMaxHealth(int amount)
    {
        maxHealth -= amount;
        if (maxHealth < 1)
        {
            maxHealth = 1;
        }
        
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
        
        if (healthUI != null)
        {
            healthUI.UpdateHearts(currentHealth, maxHealth);
        }
        
        PlayerPrefs.SetInt("SavedMaxHealth", maxHealth);
        PlayerPrefs.Save();
    }
    
    void LoseLife()
    {
        currentLives--;
        
        if (livesUI != null)
        {
            livesUI.UpdateLives(currentLives);
        }
        
        if (currentLives <= 0)
        {
            GameOver();
        }
        else
        {
            Respawn();
        }
    }
    
    void Respawn()
    {
        currentHealth = maxHealth;
        
        if (healthUI != null)
        {
            healthUI.UpdateHearts(currentHealth, maxHealth);
        }
        
        StartCoroutine(InvincibilityFrames());
    }
    
    System.Collections.IEnumerator InvincibilityFrames()
    {
        float elapsed = 0f;
        while (elapsed < invincibilityTime)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return new WaitForSeconds(blinkRate);
            elapsed += blinkRate;
        }
        
        spriteRenderer.enabled = true;
        isInvincible = false;
    }
    
    void GameOver()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.DeleteSave();
        }
        
        if (gameOverScreen != null)
        {
            gameOverScreen.SetActive(true);
            Time.timeScale = 0f;
        }
    }
}