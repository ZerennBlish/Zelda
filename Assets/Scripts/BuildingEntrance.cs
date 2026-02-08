using UnityEngine;

public class BuildingEntrance : MonoBehaviour
{
    [Header("Destination")]
    public Vector2 targetRoom;
    public Vector2 spawnOffset;
    
    [Header("Prompt (optional)")]
    public GameObject promptUI;
    
    private bool playerInRange = false;

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
        
        if (Input.GetKeyDown(KeyCode.E))
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
        if (other.CompareTag("Player"))
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
        if (other.CompareTag("Player"))
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