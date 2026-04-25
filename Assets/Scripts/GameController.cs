using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    void Update()
    {
        if (ShopUI.IsActive || DialogueBox.IsActive) return;

        #if UNITY_EDITOR
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
        #endif
    }
    
    void RefillEverything()
    {
        #if UNITY_EDITOR
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
        #endif
    }
    
    void FullReset()
    {
        #if UNITY_EDITOR
        // Clear all saved data
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.DeleteAllData();
        }
        else
        {
            PlayerPrefs.DeleteAll();
        }
        PlayerPrefs.Save();

        // Reload scene
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);

        Debug.Log("Full game reset");
        #endif
    }
}