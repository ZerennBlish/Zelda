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
        
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            PlayerPrefs.SetInt("SavedArrows", player.currentArrows);
        }
        
        PlayerHealth health = FindFirstObjectByType<PlayerHealth>();
        if (health != null)
        {
            PlayerPrefs.SetInt("SavedMaxHealth", health.maxHealth);
        }
        
        PlayerClass playerClass = FindFirstObjectByType<PlayerClass>();
        if (playerClass != null)
        {
            PlayerPrefs.SetInt("SavedClassTier", (int)playerClass.GetCurrentClass());
        }
        
        PlayerPrefs.Save();
    }
}