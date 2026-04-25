using UnityEngine;

public class BuildingEntrance : MonoBehaviour
{
    [Header("Destination")]
    public Vector2 targetRoom;
    public Vector2 spawnOffset;
    
    [Header("Prompt (optional)")]
    public GameObject promptUI;
    
    private bool playerInRange = false;
    private bool wasDialogueActive = false;

    void Start()
    {
        if (promptUI != null)
        {
            promptUI.SetActive(false);
        }
    }

    void Update()
    {
        if (!playerInRange) return;
        if (DialogueBox.IsActive || ShopUI.IsActive ||
            PauseManager.IsPaused || GameOverUI.IsActive) return;

        if (wasDialogueActive && !DialogueBox.IsActive)
        {
            wasDialogueActive = DialogueBox.IsActive;
            return;
        }
        wasDialogueActive = DialogueBox.IsActive;

        if (InputManager.Instance.InteractPressed)
        {
            RoomManager.Instance.TeleportToRoom(targetRoom, spawnOffset);
            
            if (promptUI != null)
            {
                promptUI.SetActive(false);
            }
            playerInRange = false;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && other.transform == other.transform.root)
        {
            playerInRange = true;
            if (promptUI != null)
            {
                promptUI.SetActive(true);
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && other.transform == other.transform.root)
        {
            playerInRange = false;
            if (promptUI != null)
            {
                promptUI.SetActive(false);
            }
        }
    }
    
    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0f, 1f, 1f, 0.3f);
        BoxCollider2D box = GetComponent<BoxCollider2D>();
        if (box != null)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(box.offset, box.size);
            Gizmos.DrawWireCube(box.offset, box.size);
        }
    }
}