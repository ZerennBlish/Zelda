using UnityEngine;
using System.Collections.Generic;

public class Melee : MonoBehaviour
{
    public float swingDuration = 0.3f;
    public float cooldown = 0.4f;
    public int damage = 1;
    public Collider2D hitbox;
    public float hitboxDistance = 0.5f;
    
    [Header("Swing Arc")]
    public float swingArc = 90f;
    
    [Header("Sword Beam")]
    public GameObject swordBeamPrefab;
    public float beamSpawnDistance = 0.6f;
    
    private bool canSwing = true;
    private bool isSwinging = false;
    private Transform player;
    private PlayerHealth playerHealth;
    private SpriteRenderer spriteRenderer;
    private List<Collider2D> hitThisSwing = new List<Collider2D>();
    
    // Swing arc tracking
    private float swingTimer;
    private float baseAngle;

    void Start()
    {
        player = transform.parent;
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (player != null)
        {
            playerHealth = player.GetComponent<PlayerHealth>();
        }
        
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }
    }

    public void Swing(Vector2 direction)
    {
        if (!canSwing || isSwinging) return;
        
        transform.localPosition = direction * hitboxDistance;
        
        // 180 offset flips the bow to face the right way
        baseAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 180f;
        
        // Start at the beginning of the arc
        transform.localRotation = Quaternion.Euler(0, 0, baseAngle + (swingArc / 2f));
        
        if (swordBeamPrefab != null && playerHealth != null)
        {
            if (playerHealth.currentHealth >= playerHealth.maxHealth)
            {
                FireBeam(direction);
            }
        }
        
        StartCoroutine(DoSwing());
    }
    
    void FireBeam(Vector2 direction)
    {
        Vector3 spawnPos = player.position + (Vector3)(direction * beamSpawnDistance);
        GameObject beam = Instantiate(swordBeamPrefab, spawnPos, Quaternion.identity);
        
        SwordBeam sb = beam.GetComponent<SwordBeam>();
        if (sb != null)
        {
            sb.damage = damage;
            sb.SetDirection(direction);
        }
    }

    System.Collections.IEnumerator DoSwing()
    {
        isSwinging = true;
        canSwing = false;
        hitThisSwing.Clear();
        swingTimer = swingDuration;
        
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }
        
        yield return new WaitForSeconds(swingDuration);
        
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }
        
        isSwinging = false;
        
        yield return new WaitForSeconds(cooldown);
        canSwing = true;
    }
    
    void Update()
    {
        if (!isSwinging) return;
        
        // Sweep the bow through the arc during the swing
        swingTimer -= Time.deltaTime;
        float progress = 1f - (swingTimer / swingDuration);
        float currentAngle = baseAngle + Mathf.Lerp(swingArc / 2f, -swingArc / 2f, progress);
        transform.localRotation = Quaternion.Euler(0, 0, currentAngle);
    }
    
    void FixedUpdate()
    {
        if (!isSwinging) return;
        if (hitbox == null) return;
        
        BoxCollider2D box = hitbox as BoxCollider2D;
        if (box == null) return;
        
        Vector2 center = (Vector2)transform.position + box.offset;
        Vector2 size = box.size * (Vector2)transform.lossyScale;
        float angle = transform.eulerAngles.z;
        
        Collider2D[] hits = Physics2D.OverlapBoxAll(center, size, angle);
        
        foreach (Collider2D hit in hits)
        {
            if (hitThisSwing.Contains(hit)) continue;
            
            if (hit.CompareTag("Enemy"))
            {
                hitThisSwing.Add(hit);
                
                ShieldKnight knight = hit.GetComponent<ShieldKnight>();
                if (knight != null)
                {
                    knight.TakeDamage(damage, player.position);
                    
                    HitFlash flash = hit.GetComponent<HitFlash>();
                    if (flash != null) flash.Flash();
                    
                    continue;
                }
                
                IDamageable damageable = hit.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(damage);
                    
                    HitFlash flash = hit.GetComponent<HitFlash>();
                    if (flash != null) flash.Flash();
                    
                    continue;
                }
            }
            
            if (hit.CompareTag("Destructible"))
            {
                hitThisSwing.Add(hit);
                
                Destructible destructible = hit.GetComponent<Destructible>();
                if (destructible != null)
                {
                    destructible.TakeDamage(damage);
                }
            }
        }
    }
}