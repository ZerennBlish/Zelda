using UnityEngine;

public class BuildingEntrance : MonoBehaviour
{
    [Header("Destination")]
    public Vector2 targetRoom;
    public Vector2 spawnOffset;
    
    [Header("Prompt (optional)")]
    public GameObject promptUI; // a small "Press F" text object, child of this
    
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
        
        // F key / A button to enter
        if (Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.JoystickButton0))
        {
            RoomManager.Instance.TeleportToRoom(targetRoom, spawnOffset);
            
            // Hide prompt after teleporting
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
}