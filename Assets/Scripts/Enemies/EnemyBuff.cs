using UnityEngine;

public class EnemyBuff : MonoBehaviour
{
    public enum BuffType { Haste, Fortify, Regen }
    
    private BuffType buffType;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Color tintColor;
    private float regenTimer;
    private float regenInterval = 3f;
    private bool removed = false;

    public void Initialize(BuffType type)
    {
        // Defensive dedupe: matches PlayerBuff's pattern (Batch 1). Today
        // OrcChief is the only caller and it pre-checks, so this is
        // unreachable in current code. But it protects against future buff
        // sources stacking Fortify's instant +3 HP grant.
        EnemyBuff[] existing = GetComponents<EnemyBuff>();
        foreach (EnemyBuff buff in existing)
        {
            if (buff != this && buff.buffType == type)
            {
                buff.RemoveBuff();
            }
        }

        buffType = type;
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Tint the enemy to show the buff
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;

            switch (buffType)
            {
                case BuffType.Haste:
                    tintColor = new Color(1f, 1f, 0.3f, 1f); // Yellow
                    break;
                case BuffType.Fortify:
                    tintColor = new Color(0.3f, 0.5f, 1f, 1f); // Blue
                    break;
                case BuffType.Regen:
                    tintColor = new Color(0.3f, 1f, 0.3f, 1f); // Green
                    break;
                default:
                    tintColor = Color.white;
                    break;
            }

            spriteRenderer.color = Color.Lerp(originalColor, tintColor, 0.5f);
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
        if (removed) return;
        removed = true;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }

        Destroy(this); // Removes component only, not the enemy
    }

    // Called by stunnable enemies after they restore their own originalColor
    // on stun-end, so the buff tint isn't silently erased.
    public void ReapplyTint()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.Lerp(originalColor, tintColor, 0.5f);
        }
    }
}
