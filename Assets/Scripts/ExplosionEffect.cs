using System.Collections.Generic;
using UnityEngine;

public class ExplosionEffect : MonoBehaviour
{
    public float lifetime = 0.5f;
    public float explosionRadius = 2f;
    public int explosionDamage = 2;

    // Reusable to avoid GC; size accommodates dense crowds
    private static readonly Collider2D[] hitBuffer = new Collider2D[32];

    // Dedupe per-root so an enemy with multiple colliders only takes one hit
    private HashSet<GameObject> alreadyHit = new HashSet<GameObject>();

    void Start()
    {
        DealDamage();
        Destroy(gameObject, lifetime);
    }

    void DealDamage()
    {
        int count = Physics2D.OverlapCircle(
            transform.position, explosionRadius, default(ContactFilter2D).NoFilter(), hitBuffer);
        alreadyHit.Clear();

        for (int i = 0; i < count; i++)
        {
            Collider2D hit = hitBuffer[i];
            if (hit == null) continue;

            GameObject root = hit.transform.root.gameObject;
            if (!alreadyHit.Add(root)) continue;

            if (hit.CompareTag("Player"))
            {
                PlayerHealth playerHealth = hit.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    // No-source overload: radial blast bypasses shield
                    playerHealth.TakeDamage(explosionDamage);
                }
            }

            if (hit.CompareTag("Enemy"))
            {
                IDamageable damageable = hit.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(explosionDamage);

                    HitFlash flash = hit.GetComponent<HitFlash>();
                    if (flash != null) flash.Flash();
                }
            }

            if (hit.CompareTag("Destructible"))
            {
                Destructible destructible = hit.GetComponent<Destructible>();
                if (destructible != null)
                {
                    destructible.TakeDamage(explosionDamage);
                }
            }

            if (hit.CompareTag("CrackedWall"))
            {
                CrackedWall crackedWall = hit.GetComponent<CrackedWall>();
                if (crackedWall != null)
                {
                    crackedWall.TakeDamage(explosionDamage);
                }
            }
        }
    }
}
