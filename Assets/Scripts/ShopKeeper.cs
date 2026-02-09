using UnityEngine;

public class ShopKeeper : MonoBehaviour
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
        if (DialogueBox.IsActive) return;
        if (ShopUI.IsActive) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            // Open shop after dialogue finishes
            DialogueBox.Instance.onDialogueComplete = () =>
            {
                ShopUI.Instance.Show();
            };

            DialogueBox.Instance.Show(lines);

            if (promptUI != null)
            {
                promptUI.SetActive(false);
            }
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
