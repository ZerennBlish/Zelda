using UnityEngine;

public class ExplosionEffect : MonoBehaviour
{
    public float lifetime = 0.5f;
    public float explosionRadius = 2f;
    public int explosionDamage = 2;

    void Start()
    {
        DealDamage();
        Destroy(gameObject, lifetime);
    }
    
    void DealDamage()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                PlayerHealth playerHealth = hit.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(explosionDamage, transform.position);
                }
            }
            
            if (hit.CompareTag("Enemy"))
            {
                Bat bat = hit.GetComponent<Bat>();
                if (bat != null) { bat.TakeDamage(explosionDamage); continue; }
                
                BoomShroom shroom = hit.GetComponent<BoomShroom>();
                if (shroom != null) { shroom.Explode(); continue; }
                
                Slime slime = hit.GetComponent<Slime>();
                if (slime != null) { slime.TakeDamage(explosionDamage); continue; }
                
                SlimeSplitter splitter = hit.GetComponent<SlimeSplitter>();
                if (splitter != null) { splitter.TakeDamage(explosionDamage); continue; }
                
                GoblinArcher archer = hit.GetComponent<GoblinArcher>();
                if (archer != null) { archer.TakeDamage(explosionDamage); continue; }
                
                GoblinMaceman maceman = hit.GetComponent<GoblinMaceman>();
                if (maceman != null) { maceman.TakeDamage(explosionDamage); continue; }
                
                GoblinSpearman spearman = hit.GetComponent<GoblinSpearman>();
                if (spearman != null) { spearman.TakeDamage(explosionDamage); continue; }
                
                GoblinThief thief = hit.GetComponent<GoblinThief>();
                if (thief != null) { thief.TakeDamage(explosionDamage); continue; }
                
                SkeletonMage mage = hit.GetComponent<SkeletonMage>();
                if (mage != null) { mage.TakeDamage(explosionDamage); continue; }
                
                ShieldKnight knight = hit.GetComponent<ShieldKnight>();
                if (knight != null) { knight.TakeDamage(explosionDamage); continue; }
                
                FlyingSkull skull = hit.GetComponent<FlyingSkull>();
                if (skull != null) { skull.TakeDamage(explosionDamage); continue; }
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