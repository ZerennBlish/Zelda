using UnityEngine;

public class PlayerBuff : MonoBehaviour
{
    public enum BuffType { Speed, Power, Heal, Resupply }
    
    private BuffType buffType;
    private float duration = 15f;
    private float timer;
    
    private PlayerController playerController;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    
    // Stored originals for restoration
    private float originalSpeed;
    private int originalDamage;
    private Melee meleeRef;

    public void Initialize(BuffType type)
    {
        buffType = type;
        playerController = GetComponent<PlayerController>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        
        switch (buffType)
        {
            case BuffType.Speed:
                timer = duration;
                if (playerController != null)
                {
                    originalSpeed = playerController.moveSpeed;
                    playerController.moveSpeed *= 1.5f;
                }
                ApplyTint(new Color(1f, 1f, 0.3f, 1f)); // Yellow
                break;
                
            case BuffType.Power:
                timer = duration;
                if (playerController != null && playerController.melee != null)
                {
                    meleeRef = playerController.melee;
                    originalDamage = meleeRef.damage;
                    meleeRef.damage *= 2;
                }
                ApplyTint(new Color(1f, 0.3f, 0.3f, 1f)); // Red
                break;
                
            case BuffType.Heal:
                PlayerHealth health = GetComponent<PlayerHealth>();
                if (health != null)
                {
                    health.Heal(health.maxHealth);
                }
                Destroy(this); // Instant effect, done
                return;
                
            case BuffType.Resupply:
                if (playerController != null)
                {
                    playerController.currentArrows = playerController.maxArrows;
                    playerController.currentBombs = playerController.maxBombs;
                    
                    if (playerController.arrowUI != null)
                    {
                        playerController.arrowUI.UpdateCount(playerController.currentArrows);
                    }
                    if (playerController.bombUI != null)
                    {
                        playerController.bombUI.UpdateCount(playerController.currentBombs);
                    }
                }
                Destroy(this); // Instant effect, done
                return;
        }
    }
    
    void ApplyTint(Color tintColor)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.Lerp(originalColor, tintColor, 0.3f);
        }
    }
    
    void Update()
    {
        timer -= Time.deltaTime;
        
        // Blink when about to expire (last 3 seconds)
        if (timer <= 3f && spriteRenderer != null)
        {
            float blinkRate = timer <= 1f ? 0.1f : 0.2f;
            bool showTint = Mathf.PingPong(Time.time / blinkRate, 1f) > 0.5f;
            
            if (showTint)
            {
                Color tint = buffType == BuffType.Speed ? 
                    new Color(1f, 1f, 0.3f, 1f) : new Color(1f, 0.3f, 0.3f, 1f);
                spriteRenderer.color = Color.Lerp(originalColor, tint, 0.3f);
            }
            else
            {
                spriteRenderer.color = originalColor;
            }
        }
        
        if (timer <= 0)
        {
            RemoveBuff();
        }
    }
    
    public void RemoveBuff()
    {
        switch (buffType)
        {
            case BuffType.Speed:
                if (playerController != null)
                {
                    playerController.moveSpeed = originalSpeed;
                }
                break;
                
            case BuffType.Power:
                if (meleeRef != null)
                {
                    meleeRef.damage = originalDamage;
                }
                break;
        }
        
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
        
        Destroy(this);
    }
}