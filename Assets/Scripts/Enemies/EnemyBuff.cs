using UnityEngine;

public class EnemyBuff : MonoBehaviour
{
    public enum BuffType { Haste, Fortify, Regen }
    
    private BuffType buffType;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private float regenTimer;
    private float regenInterval = 3f;

    public void Initialize(BuffType type)
    {
        buffType = type;
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Tint the enemy to show the buff
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
            
            Color tint;
            switch (buffType)
            {
                case BuffType.Haste:
                    tint = new Color(1f, 1f, 0.3f, 1f); // Yellow
                    break;
                case BuffType.Fortify:
                    tint = new Color(0.3f, 0.5f, 1f, 1f); // Blue
                    break;
                case BuffType.Regen:
                    tint = new Color(0.3f, 1f, 0.3f, 1f); // Green
                    break;
                default:
                    tint = Color.white;
                    break;
            }
            
            spriteRenderer.color = Color.Lerp(originalColor, tint, 0.5f);
        }
        
        // Fortify: add +3 bonus HP immediately
        // Skip BoomShroom — its TakeDamage always triggers explosion
        if (buffType == BuffType.Fortify && GetComponent<BoomShroom>() == null)
        {
            IDamageable damageable = GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(-3);
            }
        }
        
        regenTimer = regenInterval;
    }
    
    void LateUpdate()
    {
        // Haste: multiply velocity every frame for 1.5x effective speed
        if (buffType == BuffType.Haste && rb != null)
        {
            rb.linearVelocity = Vector2.ClampMagnitude(rb.linearVelocity * 1.5f, 20f);
        }
        
        // Regen: heal 1 HP every few seconds
        // Skip BoomShroom — its TakeDamage always triggers explosion
        if (buffType == BuffType.Regen && GetComponent<BoomShroom>() == null)
        {
            regenTimer -= Time.deltaTime;
            if (regenTimer <= 0)
            {
                regenTimer = regenInterval;
                IDamageable damageable = GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(-1);
                }
            }
        }
    }
    
    public void RemoveBuff()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
        
        Destroy(this); // Removes component only, not the enemy
    }
}
