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
        if (SaveManager.Instance != null)
        {
            SaveManager.SaveAll();
        }
    }
}
