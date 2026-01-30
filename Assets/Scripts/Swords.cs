using UnityEngine;

public class Sword : MonoBehaviour
{
    public float swingDuration = 0.3f;
    public float cooldown = 0.4f;
    public int damage = 1;
    public Collider2D hitbox;
    public float hitboxDistance = 0.5f;
    public Sprite idleSprite;
    public Sprite[] swingSprites;
    
    private bool canSwing = true;
    private bool isSwinging = false;
    private SpriteRenderer spriteRenderer;
    private Transform player;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        player = transform.parent;
        
        if (hitbox != null)
        {
            hitbox.enabled = false;
        }
    }

    public void Swing(Vector2 direction)
    {
        if (!canSwing || isSwinging) return;
        
        transform.localPosition = direction * hitboxDistance;
        
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.localRotation = Quaternion.Euler(0, 0, angle);
        
        StartCoroutine(DoSwing());
    }

    System.Collections.IEnumerator DoSwing()
    {
        isSwinging = true;
        canSwing = false;
        
        if (hitbox != null)
        {
            hitbox.enabled = true;
        }
        
        float frameTime = swingDuration / swingSprites.Length;
        for (int i = 0; i < swingSprites.Length; i++)
        {
            spriteRenderer.sprite = swingSprites[i];
            yield return new WaitForSeconds(frameTime);
        }
        
        spriteRenderer.sprite = idleSprite;
        
        if (hitbox != null)
        {
            hitbox.enabled = false;
        }
        
        isSwinging = false;
        
        yield return new WaitForSeconds(cooldown);
        canSwing = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy") && isSwinging)
        {
            Bat bat = other.GetComponent<Bat>();
            if (bat != null)
            {
                bat.TakeDamage(damage);
                return;
            }
            
            BoomShroom shroom = other.GetComponent<BoomShroom>();
            if (shroom != null)
            {
                shroom.Explode();
                return;
            }
            
            Slime slime = other.GetComponent<Slime>();
            if (slime != null)
            {
                slime.TakeDamage(damage);
                return;
            }
            
            SlimeSplitter splitter = other.GetComponent<SlimeSplitter>();
            if (splitter != null)
            {
                splitter.TakeDamage(damage);
                return;
            }
            
            GoblinArcher archer = other.GetComponent<GoblinArcher>();
            if (archer != null)
            {
                archer.TakeDamage(damage);
                return;
            }
            
            GoblinMaceman maceman = other.GetComponent<GoblinMaceman>();
            if (maceman != null)
            {
                maceman.TakeDamage(damage);
                return;
            }
            
            GoblinSpearman spearman = other.GetComponent<GoblinSpearman>();
            if (spearman != null)
            {
                spearman.TakeDamage(damage);
                return;
            }
            
            GoblinThief thief = other.GetComponent<GoblinThief>();
            if (thief != null)
            {
                thief.TakeDamage(damage);
                return;
            }
            
            SkeletonMage mage = other.GetComponent<SkeletonMage>();
            if (mage != null)
            {
                mage.TakeDamage(damage);
                return;
            }
            
            ShieldKnight knight = other.GetComponent<ShieldKnight>();
            if (knight != null)
            {
                knight.TakeDamage(damage, player.position);
                return;
            }
            
            FlyingSkull skull = other.GetComponent<FlyingSkull>();
            if (skull != null)
            {
                skull.TakeDamage(damage);
                return;
            }
            
            Destructible destructible = other.GetComponent<Destructible>();
            if (destructible != null)
            {
                destructible.TakeDamage(damage);
                return;
            }
        }
    }
}