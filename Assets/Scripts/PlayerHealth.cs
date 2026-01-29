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
    
    void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerShield = GetComponentInChildren<PlayerShield>();
        
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
    
    public void TakeDamage(int damage)
    {
        TakeDamage(damage, transform.position);
    }
    
    public void TakeDamage(int damage, Vector2 attackSource)
    {
        if (isInvincible) return;
        
        // Check if shield blocks this attack
        if (playerShield != null && playerShield.BlocksAttackFrom(attackSource))
        {
            return;
        }
        
        // Set invincible immediately to block same-frame hits
        isInvincible = true;
        
        currentHealth -= damage;
        
        if (healthUI != null)
        {
            healthUI.UpdateHearts(currentHealth);
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
            healthUI.UpdateHearts(currentHealth);
        }
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
            healthUI.UpdateHearts(currentHealth);
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