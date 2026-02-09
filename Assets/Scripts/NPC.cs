using UnityEngine;

public class NPC : MonoBehaviour
{
    [Header("Dialog")]
    [TextArea(2, 5)]
    public string[] dialogLines;
    
    [Header("Idle Animation")]
    public Sprite[] idleSprites;
    public float idleFrameRate = 4f;
    
    [Header("Interaction")]
    public float interactRange = 1.5f;
    public GameObject promptUI;
    
    private SpriteRenderer spriteRenderer;
    private Transform player;
    private bool playerInRange = false;
    private float frameTimer;
    private int currentFrame;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        
        if (promptUI != null)
        {
            promptUI.SetActive(false);
        }
    }

    void Update()
    {
        if (player == null) return;
        
        float distance = Vector2.Distance(transform.position, player.position);
        bool wasInRange = playerInRange;
        playerInRange = distance <= interactRange;
        
        // Show/hide prompt
        if (playerInRange && !wasInRange)
        {
            if (promptUI != null && !DialogueBox.IsActive)
            {
                promptUI.SetActive(true);
            }
        }
        else if (!playerInRange && wasInRange)
        {
            if (promptUI != null)
            {
                promptUI.SetActive(false);
            }
        }

        // Start dialog
        if (playerInRange && Input.GetKeyDown(KeyCode.E) && !DialogueBox.IsActive)
        {
            if (DialogueBox.Instance != null && dialogLines.Length > 0)
            {
                if (promptUI != null)
                {
                    promptUI.SetActive(false);
                }

                FacePlayer();
                DialogueBox.Instance.onDialogueComplete = () =>
                {
                    if (promptUI != null)
                    {
                        promptUI.SetActive(true);
                    }
                };
                DialogueBox.Instance.Show(dialogLines);
            }
        }
        
        // Idle animation
        AnimateIdle();
    }
    
    void AnimateIdle()
    {
        if (idleSprites == null || idleSprites.Length == 0) return;
        
        frameTimer += Time.deltaTime;
        if (frameTimer >= 1f / idleFrameRate)
        {
            frameTimer -= 1f / idleFrameRate;
            currentFrame = (currentFrame + 1) % idleSprites.Length;
            spriteRenderer.sprite = idleSprites[currentFrame];
        }
    }
    
    void FacePlayer()
    {
        if (player == null || spriteRenderer == null) return;
        
        float direction = player.position.x - transform.position.x;
        if (Mathf.Abs(direction) > 0.1f)
        {
            spriteRenderer.flipX = direction < 0;
        }
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 0f, 0.2f);
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}
