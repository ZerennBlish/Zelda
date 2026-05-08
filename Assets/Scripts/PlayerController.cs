using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;

    [Header("Item Unlocks")]
    public bool hasBoomerang = false;
    public bool hasBombs = false;
    public bool hasGrapple = false;
    public bool hasWand = false;
    public bool hasBook = false;

    [Header("References")]
    public Melee melee;

    private Rigidbody2D rb;
    private Collider2D playerCollider;
    private Camera mainCamera;
    private PlayerMount playerMount;
    private PlayerGrapple playerGrapple;
    private PlayerWeapons playerWeapons;
    private Vector2 movement;
    private Vector2 facingDirection = Vector2.down;

    void Awake()
    {
        // Awake (not Start) so sibling components can read these in their own Start().
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();
        mainCamera = Camera.main;
        playerMount = GetComponent<PlayerMount>();
        playerGrapple = GetComponent<PlayerGrapple>();
        playerWeapons = GetComponent<PlayerWeapons>();

        hasBoomerang = PlayerPrefs.GetInt("HasBoomerang", 0) == 1;
        hasBombs = PlayerPrefs.GetInt("HasBombs", 0) == 1;
        hasGrapple = PlayerPrefs.GetInt("HasGrapple", 0) == 1;
        hasWand = PlayerPrefs.GetInt("HasWand", 0) == 1;
        hasBook = PlayerPrefs.GetInt("HasBook", 0) == 1;
    }

    void Update()
    {
        if (DialogueBox.IsActive || ShopUI.IsActive || PauseManager.IsPaused || GameOverUI.IsActive) return;
        if (InputManager.Instance == null) return;

        // --- GRAPPLE STATE: block all other input (pull movement runs in PlayerGrapple.Update) ---
        if (playerGrapple.IsGrappling()) return;

        // --- MOVEMENT INPUT ---
        movement = InputManager.Instance.Move;
        
        if (movement.magnitude > 1f)
        {
            movement = movement.normalized;
        }
        
        // --- AIM DIRECTION follows mouse ---
        if (mainCamera != null)
        {
            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(InputManager.Instance.AimScreenPosition);
            mouseWorldPos.z = 0f;
            Vector2 aimDirection = ((Vector2)mouseWorldPos - (Vector2)transform.position);
            
            if (aimDirection.magnitude > 0.1f)
            {
                facingDirection = aimDirection.normalized;
            }
        }
        
        // --- MOUNT TOGGLE - M / Back button ---
        if (InputManager.Instance.MountTogglePressed)
        {
            playerMount.ToggleMount();
        }
        
        // --- CYCLE SUB-WEAPON: Mouse Wheel / I key ---
        float scroll = InputManager.Instance.ScrollWeapon;
        if (scroll > 0f)
        {
            playerWeapons.CycleWeapon(1);
        }
        else if (scroll < 0f)
        {
            playerWeapons.CycleWeapon(-1);
        }

        if (InputManager.Instance.CycleWeaponPressed)
        {
            playerWeapons.CycleWeapon(1);
        }

        // --- MELEE: Space / Left Click / X button (blocked while mounted) ---
        // Arrow + sub-weapon input lives in PlayerWeapons.Update.
        if (!playerMount.IsMounted() && InputManager.Instance.AttackPressed)
        {
            if (melee != null) melee.Swing(facingDirection);
        }
    }

    void FixedUpdate()
    {
        if (playerGrapple.IsGrappling()) return;

        float currentSpeed = moveSpeed * playerMount.GetSpeedMultiplier();
        rb.MovePosition(rb.position + movement * currentSpeed * Time.fixedDeltaTime);
    }
    
    // --- ITEM UNLOCK ---
    
    public void UnlockItem(string itemName)
    {
        switch (itemName)
        {
            case "Boomerang":
                hasBoomerang = true;
                PlayerPrefs.SetInt("HasBoomerang", 1);
                break;
            case "Bombs":
                hasBombs = true;
                PlayerPrefs.SetInt("HasBombs", 1);
                playerWeapons.OnBombBagUnlocked();
                break;
            case "Grapple":
                hasGrapple = true;
                PlayerPrefs.SetInt("HasGrapple", 1);
                break;
            case "Wand":
                hasWand = true;
                PlayerPrefs.SetInt("HasWand", 1);
                break;
            case "Book":
                hasBook = true;
                PlayerPrefs.SetInt("HasBook", 1);
                break;
        }
        PlayerPrefs.Save();

        // Rebuild so the new weapon appears in the rotation
        playerWeapons.RebuildWeaponList();
    }
    
    public Vector2 GetFacingDirection()
    {
        return facingDirection;
    }
    
    public bool IsMounted()
    {
        return playerMount.IsMounted();
    }
    
    public bool IsShooting()
    {
        return playerWeapons.IsShooting();
    }

    public void ResetActionStates()
    {
        playerGrapple.CancelGrapple();
        playerWeapons.CancelShooting();
        playerWeapons.ClearBoomerangLock();
        playerMount.ForceDismount();
        movement = Vector2.zero;
    }
}
