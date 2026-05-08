using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    private RoomManager cachedRoomManager;
    private PlayerController cachedPlayer;
    private PlayerWeapons cachedWeapons;
    private PlayerHealth cachedHealth;
    private PlayerClass cachedClass;
    private GameState cachedGameState;
    private RoomTracker cachedRoomTracker;

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

    private void CacheReferences()
    {
        if (cachedPlayer == null)
            cachedPlayer = FindFirstObjectByType<PlayerController>();
        if (cachedWeapons == null)
            cachedWeapons = FindFirstObjectByType<PlayerWeapons>();
        if (cachedHealth == null)
            cachedHealth = FindFirstObjectByType<PlayerHealth>();
        if (cachedClass == null)
            cachedClass = FindFirstObjectByType<PlayerClass>();
        if (cachedGameState == null)
            cachedGameState = GameState.Instance;
        if (cachedRoomTracker == null)
            cachedRoomTracker = RoomTracker.Instance;
        if (cachedRoomManager == null)
            cachedRoomManager = RoomManager.Instance;
    }

    public static void SaveAll()
    {
        if (Instance == null) return;
        Instance.CacheReferences();

        RoomManager roomManager = Instance.cachedRoomManager;
        PlayerHealth playerHealth = Instance.cachedHealth;
        GameState gameState = Instance.cachedGameState;
        PlayerController playerController = Instance.cachedPlayer;
        PlayerWeapons playerWeapons = Instance.cachedWeapons;
        PlayerClass playerClass = Instance.cachedClass;

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
            PlayerPrefs.SetInt("HasBoomerang", playerController.hasBoomerang ? 1 : 0);
            PlayerPrefs.SetInt("HasBombs", playerController.hasBombs ? 1 : 0);
            PlayerPrefs.SetInt("HasGrapple", playerController.hasGrapple ? 1 : 0);
            PlayerPrefs.SetInt("HasWand", playerController.hasWand ? 1 : 0);
            PlayerPrefs.SetInt("HasBook", playerController.hasBook ? 1 : 0);
        }

        if (playerWeapons != null)
        {
            PlayerPrefs.SetInt("SavedArrows", playerWeapons.currentArrows);
            PlayerPrefs.SetInt("SavedBombs", playerWeapons.currentBombs);
            PlayerPrefs.SetInt("EquippedWeaponIndex", (int)playerWeapons.GetActiveWeapon());
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
        // Run state — resets on game over
        PlayerPrefs.DeleteKey("RoomX");
        PlayerPrefs.DeleteKey("RoomY");
        PlayerPrefs.DeleteKey("Lives");
        PlayerPrefs.DeleteKey("HasSave");
        PlayerPrefs.Save();
    }

    public void DeleteAllData()
    {
        // Run state
        DeleteSave();

        // Persistent unlocks and progression
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
        PlayerPrefs.DeleteKey("HeartUpgradeBought");
        PlayerPrefs.Save();
    }
}
