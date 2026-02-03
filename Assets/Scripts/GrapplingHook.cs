using UnityEngine;

public class GrapplingHook : MonoBehaviour
{
    public float speed = 20f;
    public float maxDistance = 8f;
    public float pullSpeed = 12f;
    public Color chainColor = Color.gray;
    public float chainWidth = 0.06f;
    
    [Header("Enemy Grab")]
    public float hookStunDuration = 1.5f;
    
    [Header("Grappable Landing")]
    public float grappableLandingOffset = 0.8f;
    
    private enum HookState { Flying, PullPlayer, PullTarget, Missed }
    private HookState currentState = HookState.Flying;
    
    private Vector2 direction;
    private Vector3 startPosition;
    private Transform player;
    private PlayerController playerController;
    private LineRenderer lineRenderer;
    
    // Grabbed target (item or enemy)
    private Transform grabbedTarget;
    private Rigidbody2D grabbedRb;
    private bool grabbedIsEnemy = false;

    public void Initialize(Transform playerTransform, Vector2 hookDirection, PlayerController controller)
    {
        player = playerTransform;
        direction = hookDirection.normalized;
        startPosition = transform.position;
        playerController = controller;
        
        // Rotate hook sprite to face direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
        
        // Chain visual from player to hook tip
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.startWidth = chainWidth;
        lineRenderer.endWidth = chainWidth;
        lineRenderer.positionCount = 2;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = chainColor;
        lineRenderer.endColor = chainColor;
        lineRenderer.sortingOrder = 5;
    }

    void Update()
    {
        if (player == null)
        {
            Destroy(gameObject);
            return;
        }
        
        switch (currentState)
        {
            case HookState.Flying:
                UpdateFlying();
                break;
                
            case HookState.PullPlayer:
                // Player pull is handled by PlayerController
                // Just keep chain drawn
                UpdateChain();
                break;
                
            case HookState.PullTarget:
                UpdatePullTarget();
                break;
        }
    }
    
    void UpdateFlying()
    {
        // Move hook forward
        transform.Translate(direction * speed * Time.deltaTime, Space.World);
        
        // Draw chain
        UpdateChain();
        
        // Max distance — missed
        float distance = Vector2.Distance(startPosition, transform.position);
        if (distance >= maxDistance)
        {
            playerController.GrappleMissed();
            Destroy(gameObject);
        }
    }
    
    void UpdatePullTarget()
    {
        // Target was destroyed during pull (killed by something else)
        if (grabbedTarget == null)
        {
            playerController.GrappleFinished();
            Destroy(gameObject);
            return;
        }
        
        // Move target toward player
        grabbedTarget.position = Vector3.MoveTowards(
            grabbedTarget.position,
            player.position,
            pullSpeed * Time.deltaTime
        );
        
        // Keep hook on the target
        transform.position = grabbedTarget.position;
        
        // Draw chain from player to target
        UpdateChain();
        
        // Target arrived at player
        float distance = Vector2.Distance(grabbedTarget.position, player.position);
        if (distance < 0.5f)
        {
            DeliverTarget();
        }
    }
    
    void UpdateChain()
    {
        if (lineRenderer != null && player != null)
        {
            lineRenderer.SetPosition(0, player.position);
            lineRenderer.SetPosition(1, transform.position);
        }
    }
    
    void DeliverTarget()
    {
        if (grabbedTarget != null)
        {
            if (grabbedIsEnemy)
            {
                // Re-enable enemy physics
                if (grabbedRb != null)
                {
                    grabbedRb.bodyType = RigidbodyType2D.Dynamic;
                    grabbedRb.linearVelocity = Vector2.zero;
                }
                
                // Apply stun if the enemy supports it
                IStunnable stunnable = grabbedTarget.GetComponent<IStunnable>();
                if (stunnable != null)
                {
                    stunnable.Stun(hookStunDuration);
                }
                
                // Non-stunnable enemies (Bat, BoomShroom, GoblinThief)
                // just get dumped at your feet — good luck
            }
            else
            {
                // It's a pickup — collect it
                Collectible collectible = grabbedTarget.GetComponent<Collectible>();
                if (collectible != null)
                {
                    collectible.CollectNow(player.gameObject);
                }
            }
        }
        
        playerController.GrappleFinished();
        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (currentState != HookState.Flying) return;
        
        // Priority 1: Dedicated grapple points — pull player across
        if (other.CompareTag("GrapplePoint"))
        {
            currentState = HookState.PullPlayer;
            transform.position = other.transform.position;
            
            GrapplePoint point = other.GetComponent<GrapplePoint>();
            Vector3 landingPos;
            
            if (point != null)
            {
                landingPos = point.GetLandingPosition();
            }
            else
            {
                landingPos = other.transform.position;
            }
            
            playerController.GrappleLatched(landingPos);
            return;
        }
        
        // Priority 2: Environmental grappable objects — pull player to them
        // Trees, stumps, posts, rocks — anything tagged "Grappable"
        if (other.CompareTag("Grappable"))
        {
            currentState = HookState.PullPlayer;
            transform.position = other.transform.position;
            
            // Land next to the object, offset back toward where the player fired from
            // This prevents the player from ending up inside the tree/stump
            Vector2 approachDirection = ((Vector2)other.transform.position - (Vector2)player.position).normalized;
            Vector3 landingPos = other.transform.position - (Vector3)(approachDirection * grappableLandingOffset);
            
            playerController.GrappleLatched(landingPos);
            return;
        }
        
        // Priority 3: Enemies — pull enemy to player (dangerous!)
        if (other.CompareTag("Enemy"))
        {
            currentState = HookState.PullTarget;
            grabbedTarget = other.transform;
            grabbedIsEnemy = true;
            
            // Freeze enemy physics during pull
            grabbedRb = other.GetComponent<Rigidbody2D>();
            if (grabbedRb != null)
            {
                grabbedRb.bodyType = RigidbodyType2D.Kinematic;
                grabbedRb.linearVelocity = Vector2.zero;
            }
            
            playerController.GrappleGrabbed();
            return;
        }
        
        // Priority 4: Pickups — pull item to player
        if (other.CompareTag("Pickup"))
        {
            currentState = HookState.PullTarget;
            grabbedTarget = other.transform;
            grabbedIsEnemy = false;
            
            // Mark as carried so it stops bobbing
            Collectible collectible = other.GetComponent<Collectible>();
            if (collectible != null)
            {
                collectible.SetCarried(true);
            }
            
            playerController.GrappleGrabbed();
            return;
        }
        
        // Priority 5: Walls — miss
        if (other.CompareTag("Wall"))
        {
            playerController.GrappleMissed();
            Destroy(gameObject);
        }
    }
    
    public void DestroyHook()
    {
        Destroy(gameObject);
    }
}