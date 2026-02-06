using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    [Header("Character Sprites (54 per class — 9 rows of 6)")]
    public Sprite[] archerSprites;      // 54 sprites: idle/walk/attack
    public Sprite[] swordsmanSprites;   // 54 sprites: idle/walk/attack
    public Sprite[] spearmanSprites;    // 54 sprites: idle/walk/attack
    public Sprite[] paladinSprites;     // 54 sprites: idle/walk/attack
    
    [Header("Death/Ghost Sprites (shared across all classes)")]
    public Sprite[] deathSprites;       // 10 sprites: death/blink/ghost
    
    [Header("Animation Speed (frames per second)")]
    public float idleFrameRate = 6f;
    public float walkFrameRate = 10f;
    public float attackFrameRate = 15f;
    public float deathFrameRate = 8f;
    
    // Main sprite layout per class sheet (54 sprites, 6 per row):
    // Row 0 (0-5):   idle down
    // Row 1 (6-11):  idle right
    // Row 2 (12-17): idle up
    // Row 3 (18-23): walk down
    // Row 4 (24-29): walk right
    // Row 5 (30-35): walk up
    // Row 6 (36-41): attack down
    // Row 7 (42-47): attack right
    // Row 8 (48-53): attack up
    //
    // Death sprites: separate array, 10 frames
    
    private enum AnimState { Idle, Walk, Attack, Death }
    private enum Direction { Down, Right, Up }
    
    private SpriteRenderer spriteRenderer;
    private PlayerController playerController;
    private PlayerClass playerClass;
    private PlayerHealth playerHealth;
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
        playerHealth = GetComponent<PlayerHealth>();
        melee = GetComponentInChildren<Melee>();
        
        activeSprites = GetSpritesForCurrentClass();
    }

    void Update()
    {
        // Don't animate while mounted — horse sprite handles that
        if (playerController != null && playerController.IsMounted()) return;
        
        activeSprites = GetSpritesForCurrentClass();
        
        UpdateDirection();
        UpdateState();
        Animate();
    }
    
    /// <summary>
    /// Picks the right sprite array based on current class tier.
    /// </summary>
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
    
    /// <summary>
    /// Maps the mouse-aimed facing direction to one of 3 sprite directions.
    /// Left = Right sprites with flipX enabled.
    /// </summary>
    void UpdateDirection()
    {
        if (playerController == null) return;
        
        Vector2 facing = playerController.GetFacingDirection();
        bool flip = false;
        
        if (Mathf.Abs(facing.x) > Mathf.Abs(facing.y))
        {
            // Horizontal — use Right frames, flip if facing left
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
    
    /// <summary>
    /// Determines animation state. Priority: Death > Attack > Walk > Idle.
    /// </summary>
    void UpdateState()
    {
        // Death overrides everything
        if (playerHealth != null && playerHealth.currentHealth <= 0 && playerHealth.currentLives <= 0)
        {
            SetState(AnimState.Death);
            return;
        }
        
        // Attack — plays while melee is mid-swing
        if (melee != null && melee.IsSwinging())
        {
            SetState(AnimState.Attack);
            return;
        }
        
        // Walk vs Idle — check movement input
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
    
    /// <summary>
    /// Changes state and resets frame counter when entering a new state.
    /// </summary>
    void SetState(AnimState newState)
    {
        if (currentState != newState)
        {
            currentState = newState;
            currentFrame = 0;
            frameTimer = 0f;
        }
    }
    
    /// <summary>
    /// Advances frame timer and applies the correct sprite.
    /// </summary>
    void Animate()
    {
        // Advance timer
        float rate = GetFrameRate();
        frameTimer += Time.deltaTime;
        
        if (frameTimer >= 1f / rate)
        {
            frameTimer -= 1f / rate;
            currentFrame++;
        }
        
        // Death uses its own separate sprite array
        if (currentState == AnimState.Death)
        {
            AnimateDeath();
            return;
        }
        
        // Normal animation from class sprite array
        if (activeSprites == null || activeSprites.Length == 0) return;
        
        // State offset: Idle=0, Walk=18, Attack=36
        // Direction offset: Down=0, Right=6, Up=12
        int stateOffset = (int)currentState * 18;
        int directionOffset = (int)currentDirection * 6;
        int startIndex = stateOffset + directionOffset;
        int frameCount = 6;
        
        // Idle and Walk loop, Attack loops while swing is active
        currentFrame = currentFrame % frameCount;
        
        int spriteIndex = startIndex + currentFrame;
        
        if (spriteIndex < activeSprites.Length)
        {
            spriteRenderer.sprite = activeSprites[spriteIndex];
        }
    }
    
    /// <summary>
    /// Death animation from the separate death sprite array.
    /// Plays once and holds the last frame.
    /// </summary>
    void AnimateDeath()
    {
        if (deathSprites == null || deathSprites.Length == 0) return;
        
        // Hold on last frame
        if (currentFrame >= deathSprites.Length)
        {
            currentFrame = deathSprites.Length - 1;
        }
        
        spriteRenderer.sprite = deathSprites[currentFrame];
        spriteRenderer.flipX = false;
    }
    
    float GetFrameRate()
    {
        switch (currentState)
        {
            case AnimState.Idle:   return idleFrameRate;
            case AnimState.Walk:   return walkFrameRate;
            case AnimState.Attack: return attackFrameRate;
            case AnimState.Death:  return deathFrameRate;
            default: return idleFrameRate;
        }
    }
}