using UnityEngine;
using System.Collections.Generic;

public class Boomerang : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 12f;
    public float maxDistance = 6f;
    public float returnSpeed = 14f;
    public float rotationSpeed = 720f;
    
    [Header("Stun")]
    public float stunDuration = 2f;
    
    private Transform player;
    private Vector2 direction;
    private Vector3 startPosition;
    private bool returning = false;
    private PlayerController playerController;
    
    private List<Transform> carriedItems = new List<Transform>();
    private List<GameObject> alreadyHit = new List<GameObject>();

    public void Initialize(Transform playerTransform, Vector2 throwDirection, PlayerController controller)
    {
        player = playerTransform;
        direction = throwDirection.normalized;
        startPosition = transform.position;
        playerController = controller;
    }

    void Update()
    {
        if (player == null)
        {
            Destroy(gameObject);
            return;
        }
        
        // Spin
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
        
        if (!returning)
        {
            // Move outward
            transform.Translate(direction * speed * Time.deltaTime, Space.World);
            
            // Check if max distance reached
            float distanceTraveled = Vector2.Distance(startPosition, transform.position);
            if (distanceTraveled >= maxDistance)
            {
                returning = true;
            }
        }
        else
        {
            // Return to player's current position
            Vector2 toPlayer = (player.position - transform.position).normalized;
            transform.Translate(toPlayer * returnSpeed * Time.deltaTime, Space.World);
            
            // Move carried items with boomerang
            foreach (Transform item in carriedItems)
            {
                if (item != null)
                {
                    item.position = transform.position;
                }
            }
            
            // Check if returned to player
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);
            if (distanceToPlayer < 0.5f)
            {
                CatchBoomerang();
            }
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        // Handle enemies
        if (other.CompareTag("Enemy"))
        {
            if (!alreadyHit.Contains(other.gameObject))
            {
                alreadyHit.Add(other.gameObject);

                // Bats die in one hit
                Bat bat = other.GetComponent<Bat>();
                if (bat != null)
                {
                    bat.TakeDamage(1);

                    // Flash on kill hit
                    HitFlash flash = other.GetComponent<HitFlash>();
                    if (flash != null) flash.Flash();
                }
                else
                {
                    // ShieldKnights blocking from the front absorb the hit
                    // entirely — no stun, no damage, just bounce off
                    ShieldKnight knight = other.GetComponent<ShieldKnight>();
                    if (knight != null && knight.IsBlockingFrom(transform.position))
                    {
                        returning = true;
                        return;
                    }

                    // Stunnable enemies get stunned (the stun color IS the feedback)
                    IStunnable stunnable = other.GetComponent<IStunnable>();
                    if (stunnable != null)
                    {
                        stunnable.Stun(stunDuration);
                    }
                    else
                    {
                        // Non-stunnable, non-fragile enemies still take 1 damage
                        // (BoomShroom, GoblinThief). Otherwise the boomerang is
                        // useless against them.
                        IDamageable damageable = other.GetComponent<IDamageable>();
                        if (damageable != null)
                        {
                            damageable.TakeDamage(1);
                            HitFlash flash = other.GetComponent<HitFlash>();
                            if (flash != null) flash.Flash();
                        }
                    }
                }
            }

            returning = true;
        }

        // Grab pickups
        if (other.CompareTag("Pickup"))
        {
            Collectible collectible = other.GetComponent<Collectible>();
            if (collectible != null)
            {
                collectible.SetCarried(true);
            }

            carriedItems.Add(other.transform);
        }

        // Cut destructibles (bushes, pots) — boomerang keeps flying
        if (other.CompareTag("Destructible"))
        {
            Destructible destructible = other.GetComponent<Destructible>();
            if (destructible != null)
            {
                destructible.TakeDamage(1);
            }
            return;
        }

        // Bounce off walls
        if (other.CompareTag("Wall"))
        {
            returning = true;
        }
    }
    
    void CatchBoomerang()
    {
        // Collect carried items directly. Note that Collect() early-returns
        // without destroying the item if the player is at max health/arrows/
        // bombs — those items remain alive with isCarried=true. We rely on
        // OnDestroy iterating carriedItems to call SetCarried(false) on the
        // survivors, so we deliberately do NOT clear the list here.
        foreach (Transform item in carriedItems)
        {
            if (item != null)
            {
                Collectible collectible = item.GetComponent<Collectible>();
                if (collectible != null)
                {
                    collectible.CollectNow(player.gameObject);
                }
            }
        }

        // OnDestroy will fire BoomerangReturned and release any uncollected
        // carried items — single source of truth.
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        // Release any carried collectibles that didn't make it back to the
        // player. Otherwise they sit forever with isCarried=true (no bobbing,
        // OnTriggerEnter2D ignores Player) and become uncollectable.
        if (carriedItems != null)
        {
            foreach (Transform item in carriedItems)
            {
                if (item != null)
                {
                    Collectible collectible = item.GetComponent<Collectible>();
                    if (collectible != null)
                    {
                        collectible.SetCarried(false);
                    }
                }
            }
            carriedItems.Clear();
        }

        // If destroyed without catching (room transition, death, etc.),
        // clear the lock so the player can throw again
        if (playerController != null)
        {
            playerController.BoomerangReturned();
        }
    }
}