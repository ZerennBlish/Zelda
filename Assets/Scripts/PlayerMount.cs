using UnityEngine;

public class PlayerMount : MonoBehaviour
{
    [Header("Mount")]
    public Sprite horseSprite;
    public float mountedSpeedMultiplier = 2.5f;
    public int ramDamageToEnemy = 1;
    public int ramDamageToPlayer = 1;
    public float ramCooldown = 0.5f;

    private PlayerController playerController;
    private SpriteRenderer spriteRenderer;
    private Sprite normalSprite;
    private bool isMounted = false;
    private float ramTimer = 0f;

    void Awake()
    {
        playerController = GetComponent<PlayerController>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) normalSprite = spriteRenderer.sprite;
    }

    void Update()
    {
        if (DialogueBox.IsActive || ShopUI.IsActive || PauseManager.IsPaused) return;

        if (ramTimer > 0f)
        {
            ramTimer -= Time.deltaTime;
        }
    }

    public bool IsMounted()
    {
        return isMounted;
    }

    public float GetSpeedMultiplier()
    {
        return isMounted ? mountedSpeedMultiplier : 1f;
    }

    public void ToggleMount()
    {
        if (isMounted) Dismount(); else Mount();
    }

    public void ForceDismount()
    {
        if (isMounted) Dismount();
    }

    void Mount()
    {
        if (horseSprite == null) return;

        // Reset melee state before deactivating
        Melee melee = playerController != null ? playerController.melee : null;
        if (melee != null)
        {
            melee.ResetSwingState();
            melee.gameObject.SetActive(false);
        }

        isMounted = true;
        if (spriteRenderer != null) spriteRenderer.sprite = horseSprite;
    }

    void Dismount()
    {
        isMounted = false;
        if (spriteRenderer != null) spriteRenderer.sprite = normalSprite;

        Melee melee = playerController != null ? playerController.melee : null;
        if (melee != null)
        {
            melee.gameObject.SetActive(true);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isMounted) return;
        if (ramTimer > 0f) return;

        if (collision.gameObject.CompareTag("Enemy"))
        {
            IDamageable damageable = collision.gameObject.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(ramDamageToEnemy);
            }

            HitFlash flash = collision.gameObject.GetComponent<HitFlash>();
            if (flash != null) flash.Flash();

            PlayerHealth playerHealth = GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(ramDamageToPlayer);
            }

            ramTimer = ramCooldown;
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (!isMounted) return;
        if (ramTimer > 0f) return;

        if (collision.gameObject.CompareTag("Enemy"))
        {
            IDamageable damageable = collision.gameObject.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(ramDamageToEnemy);
            }

            HitFlash flash = collision.gameObject.GetComponent<HitFlash>();
            if (flash != null) flash.Flash();

            PlayerHealth playerHealth = GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(ramDamageToPlayer);
            }

            ramTimer = ramCooldown;
        }
    }
}
