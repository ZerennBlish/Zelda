using UnityEngine;

public class ShopKeeper : MonoBehaviour
{
    [Header("Dialogue")]
    [TextArea]
    public string[] lines;

    [Header("Prompt (optional)")]
    public GameObject promptUI;

    private bool playerInRange = false;
    private bool wasShopActive = false;

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

        if (wasShopActive && !ShopUI.IsActive)
        {
            wasShopActive = false;
            return;
        }
        wasShopActive = ShopUI.IsActive;

        if (DialogueBox.IsActive || ShopUI.IsActive ||
            PauseManager.IsPaused || GameOverUI.IsActive) return;

        if (InputManager.Instance.InteractPressed)
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
