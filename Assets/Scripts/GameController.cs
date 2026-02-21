using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    void Update()
    {
        // Debug refill + unlock all - O key
        if (InputManager.Instance.DebugRefillPressed)
        {
            RefillEverything();
        }

        // Debug class upgrade - T key
        if (InputManager.Instance.DebugCycleClassPressed)
        {
            PlayerClass pc = FindFirstObjectByType<PlayerClass>();
            if (pc != null) pc.UpgradeClass();
        }

        // Full reset - R key
        if (InputManager.Instance.DebugResetPressed)
        {
            FullReset();
        }

        if (ShopUI.IsActive || DialogueBox.IsActive) return;

        // Quit - Escape only
        if (InputManager.Instance.EscapePressed)
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
        
        // Refill arrows and bombs + unlock all items
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
            
            // Unlock all items
            player.UnlockItem("Boomerang");
            player.UnlockItem("Bombs");
            player.UnlockItem("Grapple");
            player.UnlockItem("Wand");
            player.UnlockItem("Book");
        }
        
        // Add 50 rupees
        if (GameState.Instance != null)
        {
            GameState.Instance.AddRupees(50);
        }
        
        Debug.Log("Refilled everything + unlocked all items + 50 rupees");
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