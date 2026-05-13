using UnityEngine;

public abstract class EnemyBase : MonoBehaviour, IDamageable
{
    [Header("Health")]
    public int health = 1;

    protected Rigidbody2D rb;
    protected SpriteRenderer spriteRenderer;
    protected Transform player;
    protected bool isDead = false;

    protected Rect roomBounds;

    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        InitializeRoomBounds();
    }

    protected virtual void InitializeRoomBounds()
    {
        float roomW = 18f;
        float roomH = 10f;
        if (RoomManager.Instance != null)
        {
            roomW = RoomManager.Instance.roomWidth;
            roomH = RoomManager.Instance.roomHeight;
        }

        int roomX = Mathf.RoundToInt(transform.position.x / roomW);
        int roomY = Mathf.RoundToInt(transform.position.y / roomH);

        const float inset = 0.5f;
        float halfW = roomW * 0.5f - inset;
        float halfH = roomH * 0.5f - inset;
        float centerX = roomX * roomW;
        float centerY = roomY * roomH;

        roomBounds = new Rect(centerX - halfW, centerY - halfH, halfW * 2f, halfH * 2f);
    }

    protected virtual void LateUpdate()
    {
        if (isDead) return;
        ClampToRoomBounds();
    }

    protected virtual void ClampToRoomBounds()
    {
        if (rb == null) return;

        Vector2 pos = rb.position;
        pos.x = Mathf.Clamp(pos.x, roomBounds.xMin, roomBounds.xMax);
        pos.y = Mathf.Clamp(pos.y, roomBounds.yMin, roomBounds.yMax);
        rb.position = pos;
    }

    public virtual void TakeDamage(int amount)
    {
        if (isDead) return;
        health -= amount;
        if (health <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        if (isDead) return;
        isDead = true;
        if (rb != null) rb.linearVelocity = Vector2.zero;
        OnDie();
        DropAndDestroy();
    }

    // Hook for per-enemy death side effects that run before drop+destroy
    // (player-buff award on OrcChief, rupee refund on GoblinThief, etc.).
    protected virtual void OnDie() { }

    // Reusable death tail. Subclasses that fully override Die (SlimeSplitter
    // for the Small-size branch) call this for the standard path.
    protected void DropAndDestroy()
    {
        Dropper dropper = GetComponent<Dropper>();
        if (dropper != null)
        {
            dropper.Drop();
        }
        Destroy(gameObject);
    }
}
