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

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        
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
            // Check for BoomShroom
            BoomShroom shroom = other.GetComponent<BoomShroom>();
            if (shroom != null)
            {
                shroom.Explode();
                return;
            }
            
            // Check for Slime
            Slime slime = other.GetComponent<Slime>();
            if (slime != null)
            {
                slime.TakeDamage(damage);
                return;
            }
            
            // Check for GoblinArcher
            GoblinArcher archer = other.GetComponent<GoblinArcher>();
            if (archer != null)
            {
                archer.TakeDamage(damage);
                return;
            }
            
            // Check for Destructible
            Destructible destructible = other.GetComponent<Destructible>();
            if (destructible != null)
            {
                destructible.TakeDamage(damage);
            }
            else
            {
                Destroy(other.gameObject);
            }
        }
    }
}