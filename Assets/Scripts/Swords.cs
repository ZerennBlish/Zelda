using UnityEngine;

public class Sword : MonoBehaviour
{
    public float swingDuration = 0.3f;
    public float cooldown = 0.4f;
    public int damage = 1;
    public Collider2D hitbox;
    public float hitboxDistance = 0.5f;
    
    private bool canSwing = true;
    private bool isSwinging = false;
    private SpriteRenderer spriteRenderer;
    private Animator animator;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        
        if (hitbox != null)
        {
            hitbox.enabled = false;
        }
    }

    public void Swing(Vector2 direction)
    {
        if (!canSwing || isSwinging) return;
        
        // Position hitbox in facing direction
        transform.localPosition = direction * hitboxDistance;
        
        // Rotate sword to face direction
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
        
        if (animator != null)
        {
            animator.SetTrigger("Swing");
        }
        
        yield return new WaitForSeconds(swingDuration);
        
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
            BoomShroom shroom = other.GetComponent<BoomShroom>();
            if (shroom != null)
            {
                shroom.Explode();
            }
            else
            {
                Destroy(other.gameObject);
            }
        }
    }
}