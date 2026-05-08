using UnityEngine;

public abstract class StunnableEnemy : EnemyBase, IStunnable
{
    [Header("Stun")]
    public Color stunColor = new Color(0.5f, 0.5f, 1f, 1f);

    protected float stunTimer;
    protected Color originalColor;

    private bool _isStunned = false;
    public bool IsStunned => _isStunned;

    protected override void Start()
    {
        base.Start();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    public void Stun(float duration)
    {
        if (isDead) return;
        if (!CanBeStunned()) return;

        _isStunned = true;
        stunTimer = duration;
        if (rb != null) rb.linearVelocity = Vector2.zero;
        if (spriteRenderer != null) spriteRenderer.color = stunColor;

        OnStunEnter();
    }

    // Subclass calls this from its Update() before running its state machine.
    // Returns true while stunned (caller should `return` when true).
    // Restores tint and calls OnStunExit when the timer expires.
    protected bool TickStun()
    {
        if (!_isStunned) return false;

        if (rb != null) rb.linearVelocity = Vector2.zero;
        stunTimer -= Time.deltaTime;

        if (stunTimer <= 0)
        {
            _isStunned = false;
            if (spriteRenderer != null) spriteRenderer.color = originalColor;

            EnemyBuff buff = GetComponent<EnemyBuff>();
            if (buff != null) buff.ReapplyTint();

            OnStunExit();
        }

        return true;
    }

    protected virtual bool CanBeStunned() => true;
    protected virtual void OnStunEnter() { }
    protected virtual void OnStunExit() { }
}
