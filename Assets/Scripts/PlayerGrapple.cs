using UnityEngine;

public class PlayerGrapple : MonoBehaviour
{
    [Header("Grappling Hook")]
    public GameObject grapplingHookPrefab;
    public float grapplePullSpeed = 12f;

    private PlayerController playerController;
    private Rigidbody2D rb;
    private Collider2D playerCollider;

    private bool isGrappling = false;
    private bool isPullingPlayer = false;
    private Vector3 grappleTarget;
    private GameObject grappleHookInstance;

    void Awake()
    {
        playerController = GetComponent<PlayerController>();
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();
    }

    void Update()
    {
        if (DialogueBox.IsActive || ShopUI.IsActive || PauseManager.IsPaused || GameOverUI.IsActive) return;

        if (isGrappling && isPullingPlayer)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                grappleTarget,
                grapplePullSpeed * Time.deltaTime
            );

            if (Vector2.Distance(transform.position, grappleTarget) < 0.1f)
            {
                transform.position = grappleTarget;
                EndGrapple();
            }
        }
    }

    public bool IsGrappling()
    {
        return isGrappling;
    }

    public void FireGrapple()
    {
        if (grapplingHookPrefab == null) return;

        isGrappling = true;
        isPullingPlayer = false;

        if (rb != null) rb.linearVelocity = Vector2.zero;

        Vector2 facing = playerController != null ? playerController.GetFacingDirection() : Vector2.down;
        Vector3 spawnPos = transform.position + (Vector3)(facing * 0.5f);
        grappleHookInstance = Instantiate(grapplingHookPrefab, spawnPos, Quaternion.identity);
        grappleHookInstance.GetComponent<GrapplingHook>().Initialize(transform, facing, this);
    }

    public void GrappleLatched(Vector3 targetPosition)
    {
        isPullingPlayer = true;
        grappleTarget = targetPosition;

        if (playerCollider != null) playerCollider.enabled = false;

        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero;
        }
    }

    public void GrappleGrabbed()
    {
        isPullingPlayer = false;
    }

    public void GrappleMissed()
    {
        isGrappling = false;
        isPullingPlayer = false;
        grappleHookInstance = null;
    }

    public void GrappleFinished()
    {
        isGrappling = false;
        isPullingPlayer = false;
        grappleHookInstance = null;
    }

    public void CancelGrapple()
    {
        if (!isGrappling && !isPullingPlayer) return;
        EndGrapple();
    }

    void EndGrapple()
    {
        if (playerCollider != null) playerCollider.enabled = true;

        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.linearVelocity = Vector2.zero;
        }

        isGrappling = false;
        isPullingPlayer = false;

        if (grappleHookInstance != null)
        {
            Destroy(grappleHookInstance);
            grappleHookInstance = null;
        }
    }
}
