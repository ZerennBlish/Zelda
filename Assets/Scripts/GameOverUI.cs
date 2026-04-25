using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    public static bool IsActive { get; private set; } = false;

    void OnEnable()
    {
        IsActive = true;
    }

    void OnDisable()
    {
        IsActive = false;
    }

    public void Continue()
    {
        IsActive = false;
        SaveInventory();
        Time.timeScale = 1f;
        SceneManager.LoadScene("Game");
    }

    public void QuitToMenu()
    {
        IsActive = false;
        SaveInventory();
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
    
    void SaveInventory()
    {
        if (GameState.Instance != null)
        {
            PlayerPrefs.SetInt("SavedRupees", GameState.Instance.rupees);
        }

        PlayerController playerController = FindFirstObjectByType<PlayerController>(FindObjectsInactive.Include);
        if (playerController != null)
        {
            PlayerPrefs.SetInt("HasBoomerang", playerController.hasBoomerang ? 1 : 0);
            PlayerPrefs.SetInt("HasBombs", playerController.hasBombs ? 1 : 0);
            PlayerPrefs.SetInt("HasGrapple", playerController.hasGrapple ? 1 : 0);
            PlayerPrefs.SetInt("HasWand", playerController.hasWand ? 1 : 0);
            PlayerPrefs.SetInt("HasBook", playerController.hasBook ? 1 : 0);
        }

        PlayerHealth playerHealth = FindFirstObjectByType<PlayerHealth>(FindObjectsInactive.Include);
        if (playerHealth != null)
        {
            PlayerPrefs.SetInt("SavedMaxHealth", playerHealth.maxHealth);
        }

        PlayerClass playerClass = FindFirstObjectByType<PlayerClass>(FindObjectsInactive.Include);
        if (playerClass != null)
        {
            PlayerPrefs.SetInt("SavedClassTier", (int)playerClass.GetCurrentClass());
        }

        PlayerPrefs.SetInt("HasSave", 1);
        PlayerPrefs.Save();
    }
}
