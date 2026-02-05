using UnityEngine;

public class HitFlash : MonoBehaviour
{
    public Color flashColor = new Color(0.29f, 0f, 0.51f, 1f); // Indigo
    public float flashDuration = 0.2f;
    
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