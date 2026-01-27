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
        PlayerPrefs.Save();
    }
}