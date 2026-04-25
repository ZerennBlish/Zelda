using System.Collections.Generic;
using UnityEngine;

public class FireTrail : MonoBehaviour
{
    public float lifetime = 1.5f;
    public int damage = 1;
    public float damageInterval = 0.5f;
    public float fadeStartTime = 1f;

    private SpriteRenderer spriteRenderer;
    private float timer;
    private Color originalColor;

    // Per-target next-allowed-damage time so multiple enemies on the same
    // trail each take their own ticks instead of fighting over a global cooldown.
    private Dictionary<Collider2D, float> nextDamageTime =
        new Dictionary<Collider2D, float>();

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        timer = lifetime;

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    void Update()
    {
        timer -= Time.deltaTime;

        // Fade out near end of life
        if (timer <= fadeStartTime && spriteRenderer != null)
        {
            float alpha = timer / fadeStartTime;
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
        }

        if (timer <= 0)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        float now = Time.time;
        float nextTime;
        if (nextDamageTime.TryGetValue(other, out nextTime) && now < nextTime)
        {
            return;
        }

        if (other.CompareTag("Enemy"))
        {
            IDamageable damageable = other.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage);

                HitFlash flash = other.GetComponent<HitFlash>();
                if (flash != null) flash.Flash();

                nextDamageTime[other] = now + damageInterval;
            }
            return;
        }

        if (other.CompareTag("Destructible"))
        {
            Destructible destructible = other.GetComponent<Destructible>();
            if (destructible != null)
            {
                destructible.TakeDamage(damage);
                nextDamageTime[other] = now + damageInterval;
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        // Drop the entry so dead/departed targets don't bloat the dictionary
        nextDamageTime.Remove(other);
    }
}
