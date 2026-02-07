using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public GameObject pauseMenu;
    private bool isPaused = false;

    void Start()
    {
        if (pauseMenu != null)
        {
            pauseMenu.SetActive(false);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.JoystickButton7))
        {
            if (pauseMenu == null) return;
            
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    public void PauseGame()
    {
        if (pauseMenu == null) return;
        
        isPaused = true;
        pauseMenu.SetActive(true);
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        if (pauseMenu == null) return;
        
        isPaused = false;
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
    }

    public void QuitToMenu()
    {
        SaveGame();
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
    
    void SaveGame()
    {
        // Save room and lives
        if (SaveManager.Instance != null && RoomManager.Instance != null)
        {
            Vector2 room = RoomManager.Instance.GetCurrentRoom();
            PlayerHealth health = FindFirstObjectByType<PlayerHealth>();
            int lives = health != null ? health.currentLives : 3;
            SaveManager.Instance.SaveGame((int)room.x, (int)room.y, lives);
        }
        
        // Save inventory
        if (GameState.Instance != null)
        {
            PlayerPrefs.SetInt("SavedRupees", GameState.Instance.rupees);
        }
        
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            PlayerPrefs.SetInt("SavedArrows", player.currentArrows);
            
            // Save item unlocks
            PlayerPrefs.SetInt("HasBoomerang", player.hasBoomerang ? 1 : 0);
            PlayerPrefs.SetInt("HasBombs", player.hasBombs ? 1 : 0);
            PlayerPrefs.SetInt("HasGrapple", player.hasGrapple ? 1 : 0);
            PlayerPrefs.SetInt("HasWand", player.hasWand ? 1 : 0);
            PlayerPrefs.SetInt("HasBook", player.hasBook ? 1 : 0);
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