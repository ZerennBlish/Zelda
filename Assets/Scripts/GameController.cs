using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    void Update()
    {
        // Temporary - find which button is which on your Xbox controller
        for (int i = 0; i < 20; i++)
        {
            if (Input.GetKeyDown((KeyCode)(350 + i)))
            {
                Debug.Log("Joystick Button " + i + " pressed");
            }
        }
        
        // Debug refill - O key
        if (Input.GetKeyDown(KeyCode.O))
        {
            RefillEverything();
        }
        
        // Debug class upgrade - T key
        if (Input.GetKeyDown(KeyCode.T))
        {
            PlayerClass pc = FindFirstObjectByType<PlayerClass>();
            if (pc != null) pc.UpgradeClass();
        }
        
        // Full reset - R key
        if (Input.GetKeyDown(KeyCode.R))
        {
            FullReset();
        }
        
        // Quit - Escape only
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }
    }
    
    void RefillEverything()
    {
        // Refill health
        PlayerHealth health = FindFirstObjectByType<PlayerHealth>();
        if (health != null)
        {
            health.currentHealth = health.maxHealth;
            if (health.healthUI != null)
            {
                health.healthUI.UpdateHearts(health.currentHealth, health.maxHealth);
            }
        }
        
        // Refill arrows and bombs
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            player.currentArrows = player.maxArrows;
            player.currentBombs = player.maxBombs;
            
            if (player.arrowUI != null)
            {
                player.arrowUI.UpdateCount(player.currentArrows);
            }
            if (player.bombUI != null)
            {
                player.bombUI.UpdateCount(player.currentBombs);
            }
        }
        
        // Add 50 rupees
        if (GameState.Instance != null)
        {
            GameState.Instance.AddRupees(50);
        }
        
        Debug.Log("Refilled everything + 50 rupees");
    }
    
    void FullReset()
    {
        // Clear all saved data
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        
        // Reload scene
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        
        Debug.Log("Full game reset");
    }
}