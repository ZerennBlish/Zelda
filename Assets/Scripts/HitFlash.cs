using UnityEngine;

public class HitFlash : MonoBehaviour
{
    public Color flashColor = Color.white;
    public float flashDuration = 0.1f;
    
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private float flashTimer;
    private bool isFlashing = false;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    void Update()
    {
        if (!isFlashing) return;
        
        flashTimer -= Time.deltaTime;
        if (flashTimer <= 0)
        {
            spriteRenderer.color = originalColor;
            isFlashing = false;
        }
    }

    public void Flash()
    {
        if (spriteRenderer == null) return;
        
        spriteRenderer.color = flashColor;
        flashTimer = flashDuration;
        isFlashing = true;
    }
}