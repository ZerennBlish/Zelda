using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    public void Continue()
    {
        // Save inventory before restarting
        SaveInventory();
        
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    public void QuitToMenu()
    {
        // Save inventory before quitting
        SaveInventory();
        
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
    
    void SaveInventory()
    {
        // Save rupees
        if (GameState.Instance != null)
        {
            PlayerPrefs.SetInt("SavedRupees", GameState.Instance.rupees);
        }
        
        // Save arrows
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            PlayerPrefs.SetInt("SavedArrows", player.currentArrows);
        }
        
        // Save max health (for when you add heart containers)
        PlayerHealth health = FindFirstObjectByType<PlayerHealth>();
        if (health != null)
        {
            PlayerPrefs.SetInt("SavedMaxHealth", health.maxHealth);
        }
        
        PlayerPrefs.Save();
    }
}