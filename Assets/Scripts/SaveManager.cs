using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void SaveGame(int roomX, int roomY, int lives)
    {
        PlayerPrefs.SetInt("RoomX", roomX);
        PlayerPrefs.SetInt("RoomY", roomY);
        PlayerPrefs.SetInt("Lives", lives);
        PlayerPrefs.SetInt("HasSave", 1);
        PlayerPrefs.Save();
    }

    public static void SaveAll()
    {
        RoomManager roomManager = FindFirstObjectByType<RoomManager>();
        PlayerHealth playerHealth = FindFirstObjectByType<PlayerHealth>();
        GameState gameState = FindFirstObjectByType<GameState>();
        PlayerController playerController = FindFirstObjectByType<PlayerController>();
        PlayerClass playerClass = FindFirstObjectByType<PlayerClass>();

        if (roomManager != null)
        {
            Vector2 room = roomManager.GetCurrentRoom();
            int lives = playerHealth != null ? playerHealth.currentLives : 3;
            PlayerPrefs.SetInt("RoomX", (int)room.x);
            PlayerPrefs.SetInt("RoomY", (int)room.y);
            PlayerPrefs.SetInt("Lives", lives);
            PlayerPrefs.SetInt("HasSave", 1);
        }

        if (gameState != null)
        {
            PlayerPrefs.SetInt("SavedRupees", gameState.rupees);
        }

        if (playerController != null)
        {
            PlayerPrefs.SetInt("SavedArrows", playerController.currentArrows);
            PlayerPrefs.SetInt("SavedBombs", playerController.currentBombs);

            PlayerPrefs.SetInt("HasBoomerang", playerController.hasBoomerang ? 1 : 0);
            PlayerPrefs.SetInt("HasBombs", playerController.hasBombs ? 1 : 0);
            PlayerPrefs.SetInt("HasGrapple", playerController.hasGrapple ? 1 : 0);
            PlayerPrefs.SetInt("HasWand", playerController.hasWand ? 1 : 0);
            PlayerPrefs.SetInt("HasBook", playerController.hasBook ? 1 : 0);

            PlayerPrefs.SetInt("EquippedWeaponIndex", playerController.GetEquippedWeaponIndex());
        }

        if (playerHealth != null)
        {
            PlayerPrefs.SetInt("SavedMaxHealth", playerHealth.maxHealth);
        }

        if (playerClass != null)
        {
            PlayerPrefs.SetInt("SavedClassTier", (int)playerClass.GetCurrentClass());
        }

        PlayerPrefs.Save();
    }
    
    public bool HasSaveData()
    {
        return PlayerPrefs.GetInt("HasSave", 0) == 1;
    }
    
    public int GetSavedRoomX()
    {
        return PlayerPrefs.GetInt("RoomX", 0);
    }
    
    public int GetSavedRoomY()
    {
        return PlayerPrefs.GetInt("RoomY", 0);
    }
    
    public int GetSavedLives()
    {
        return PlayerPrefs.GetInt("Lives", 3);
    }
    
    public void DeleteSave()
    {
        PlayerPrefs.DeleteKey("RoomX");
        PlayerPrefs.DeleteKey("RoomY");
        PlayerPrefs.DeleteKey("Lives");
        PlayerPrefs.DeleteKey("HasSave");
        PlayerPrefs.DeleteKey("SavedRupees");
        PlayerPrefs.DeleteKey("SavedArrows");
        PlayerPrefs.DeleteKey("SavedBombs");
        PlayerPrefs.DeleteKey("SavedMaxHealth");
        PlayerPrefs.DeleteKey("SavedClassTier");
        PlayerPrefs.DeleteKey("HasBoomerang");
        PlayerPrefs.DeleteKey("HasBombs");
        PlayerPrefs.DeleteKey("HasGrapple");
        PlayerPrefs.DeleteKey("HasWand");
        PlayerPrefs.DeleteKey("HasBook");
        PlayerPrefs.DeleteKey("EquippedWeaponIndex");
        PlayerPrefs.DeleteKey("VisitedRooms");
        PlayerPrefs.Save();
    }
}
