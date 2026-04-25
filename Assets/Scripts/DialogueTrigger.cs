using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [Header("Dialogue")]
    [TextArea]
    public string[] lines;

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
        if (DialogueBox.IsActive || ShopUI.IsActive ||
            PauseManager.IsPaused || GameOverUI.IsActive) return;

        if (InputManager.Instance.InteractPressed)
        {
            DialogueBox.Instance.Show(lines);

            if (promptUI != null)
            {
                promptUI.SetActive(false);
            }
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
}
