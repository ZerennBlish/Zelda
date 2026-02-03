using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    public Button continueButton;
    
    void Start()
    {
        if (continueButton != null)
        {
            continueButton.interactable = PlayerPrefs.GetInt("HasSave", 0) == 1;
        }
    }
    
    public void NewGame()
    {
        PlayerPrefs.DeleteKey("RoomX");
        PlayerPrefs.DeleteKey("RoomY");
        PlayerPrefs.DeleteKey("Lives");
        PlayerPrefs.DeleteKey("HasSave");
        PlayerPrefs.DeleteKey("SavedRupees");
        PlayerPrefs.DeleteKey("SavedArrows");
        PlayerPrefs.DeleteKey("SavedBombs");
        PlayerPrefs.DeleteKey("SavedMaxHealth");
        PlayerPrefs.Save();
        
        SceneManager.LoadScene("Game");
    }
    
    public void ContinueGame()
    {
        SceneManager.LoadScene("Game");
    }
    
    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}