using UnityEngine;

public class PlayerShield : MonoBehaviour
{
    [Header("Shield Settings")]
    public float shieldDistance = 0.4f;
    public float shieldArc = 120f;
    
    [Header("Input")]
    public KeyCode blockKey = KeyCode.LeftShift;
    public KeyCode blockButton = KeyCode.JoystickButton1; // Xbox B
    
    private SpriteRenderer spriteRenderer;
    private PlayerController playerController;
    private Transform playerTransform;
    private bool isBlocking = false;
    private Vector2 blockDirection;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerController = GetComponentInParent<PlayerController>();
        playerTransform = playerController.transform;
        
        spriteRenderer.enabled = false;
    }

    void Update()
    {
        isBlocking = Input.GetKey(blockKey) || Input.GetKey(blockButton);
        
        spriteRenderer.enabled = isBlocking;
        
        if (isBlocking)
        {
            UpdateShieldPosition();
        }
    }
    
    void UpdateShieldPosition()
    {
        blockDirection = playerController.GetFacingDirection();
        
        transform.localPosition = blockDirection * shieldDistance;
        
        if (Mathf.Abs(blockDirection.x) > Mathf.Abs(blockDirection.y))
        {
            spriteRenderer.flipX = blockDirection.x < 0;
            spriteRenderer.flipY = false;
        }
        else
        {
            spriteRenderer.flipX = false;
            spriteRenderer.flipY = blockDirection.y < 0;
        }
    }
    
    public bool IsBlocking()
    {
        return isBlocking;
    }
    
    public bool BlocksAttackFrom(Vector2 attackSource)
    {
        if (!isBlocking) return false;
        
        Vector2 toAttacker = (attackSource - (Vector2)playerTransform.position).normalized;
        float angle = Vector2.Angle(blockDirection, toAttacker);
        
        return angle < shieldArc / 2f;
    }
}