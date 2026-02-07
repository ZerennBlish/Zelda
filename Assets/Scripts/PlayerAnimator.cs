using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    [Header("Sprite Sheets (54 per class — 6 columns x 9 rows)")]
    public Sprite[] archerSprites;
    public Sprite[] swordsmanSprites;
    public Sprite[] spearmanSprites;
    public Sprite[] paladinSprites;
    
    [Header("Animation Speed (frames per second)")]
    public float idleFrameRate = 6f;
    public float walkFrameRate = 10f;
    public float attackFrameRate = 15f;
    
    // Layout per 54-sprite sheet (6 per row):
    // Row 0 (0-5):   idle down
    // Row 1 (6-11):  idle right
    // Row 2 (12-17): idle up
    // Row 3 (18-23): walk down
    // Row 4 (24-29): walk right
    // Row 5 (30-35): walk up
    // Row 6 (36-41): attack down
    // Row 7 (42-47): attack right
    // Row 8 (48-53): attack up
    
    private enum AnimState { Idle, Walk, Attack }
    private enum Direction { Down, Right, Up }
    
    private SpriteRenderer spriteRenderer;
    private PlayerController playerController;
    private PlayerClass playerClass;
    private Melee melee;
    
    private AnimState currentState = AnimState.Idle;
    private Direction currentDirection = Direction.Down;
    
    private float frameTimer;
    private int currentFrame;
    private Sprite[] activeSprites;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerController = GetComponent<PlayerController>();
        playerClass = GetComponent<PlayerClass>();
        melee = GetComponentInChildren<Melee>();
        
        activeSprites = GetSpritesForCurrentClass();
    }

    void Update()
    {
        if (playerController != null && playerController.IsMounted()) return;
        
        activeSprites = GetSpritesForCurrentClass();
        
        UpdateDirection();
        UpdateState();
        Animate();
    }
    
    Sprite[] GetSpritesForCurrentClass()
    {
        if (playerClass == null) return archerSprites;
        
        switch (playerClass.GetCurrentClass())
        {
            case PlayerClass.ClassTier.Archer:    return archerSprites;
            case PlayerClass.ClassTier.Swordsman:  return swordsmanSprites;
            case PlayerClass.ClassTier.Spearman:   return spearmanSprites;
            case PlayerClass.ClassTier.Paladin:    return paladinSprites;
            default: return archerSprites;
        }
    }
    
    void UpdateDirection()
    {
        if (playerController == null) return;
        
        Vector2 facing = playerController.GetFacingDirection();
        bool flip = false;
        
        if (Mathf.Abs(facing.x) > Mathf.Abs(facing.y))
        {
            currentDirection = Direction.Right;
            if (facing.x < 0) flip = true;
        }
        else if (facing.y > 0)
        {
            currentDirection = Direction.Up;
        }
        else
        {
            currentDirection = Direction.Down;
        }
        
        spriteRenderer.flipX = flip;
    }
    
    void UpdateState()
    {
        // Archer attack animation triggers on bow fire
        // All other classes trigger on melee swing
        bool attacking = false;
        
        if (playerClass != null && playerClass.GetCurrentClass() == PlayerClass.ClassTier.Archer)
        {
            attacking = playerController != null && playerController.IsShooting();
        }
        else
        {
            attacking = melee != null && melee.IsSwinging();
        }
        
        if (attacking)
        {
            SetState(AnimState.Attack);
            return;
        }
        
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        
        if (Mathf.Abs(h) > 0.1f || Mathf.Abs(v) > 0.1f)
        {
            SetState(AnimState.Walk);
        }
        else
        {
            SetState(AnimState.Idle);
        }
    }
    
    void SetState(AnimState newState)
    {
        if (currentState != newState)
        {
            currentState = newState;
            currentFrame = 0;
            frameTimer = 0f;
        }
    }
    
    void Animate()
    {
        if (activeSprites == null || activeSprites.Length == 0) return;
        
        float rate = GetFrameRate();
        frameTimer += Time.deltaTime;
        
        if (frameTimer >= 1f / rate)
        {
            frameTimer -= 1f / rate;
            currentFrame++;
        }
        
        // State offset: Idle=0, Walk=18, Attack=36
        // Direction offset: Down=0, Right=6, Up=12
        int stateOffset = (int)currentState * 18;
        int directionOffset = (int)currentDirection * 6;
        int startIndex = stateOffset + directionOffset;
        
        currentFrame = currentFrame % 6;
        
        int spriteIndex = startIndex + currentFrame;
        
        if (spriteIndex < activeSprites.Length)
        {
            spriteRenderer.sprite = activeSprites[spriteIndex];
        }
    }
    
    float GetFrameRate()
    {
        switch (currentState)
        {
            case AnimState.Idle:   return idleFrameRate;
            case AnimState.Walk:   return walkFrameRate;
            case AnimState.Attack: return attackFrameRate;
            default: return idleFrameRate;
        }
    }
}