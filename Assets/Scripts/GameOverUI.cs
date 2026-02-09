using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    public void Continue()
    {
        SaveInventory();
        Time.timeScale = 1f;
        SceneManager.LoadScene("Game");
    }
    
    public void QuitToMenu()
    {
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

        PlayerController playerController = FindFirstObjectByType<PlayerController>();
        if (playerController != null)
        {
            PlayerPrefs.SetInt("HasBoomerang", playerController.hasBoomerang ? 1 : 0);
            PlayerPrefs.SetInt("HasBombs", playerController.hasBombs ? 1 : 0);
            PlayerPrefs.SetInt("HasGrapple", playerController.hasGrapple ? 1 : 0);
            PlayerPrefs.SetInt("HasWand", playerController.hasWand ? 1 : 0);
            PlayerPrefs.SetInt("HasBook", playerController.hasBook ? 1 : 0);
        }

        PlayerHealth playerHealth = FindFirstObjectByType<PlayerHealth>();
        if (playerHealth != null)
        {
            PlayerPrefs.SetInt("SavedMaxHealth", playerHealth.maxHealth);
        }

        PlayerClass playerClass = FindFirstObjectByType<PlayerClass>();
        if (playerClass != null)
        {
            PlayerPrefs.SetInt("SavedClassTier", (int)playerClass.GetCurrentClass());
        }

        PlayerPrefs.Save();
    }
}
