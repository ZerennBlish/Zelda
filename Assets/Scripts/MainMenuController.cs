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
            continueButton.interactable = SaveManager.Instance != null && SaveManager.Instance.HasSaveData();
        }
    }
    
    public void NewGame()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.DeleteSave();
        }
        
        PlayerPrefs.DeleteKey("SavedRupees");
        PlayerPrefs.DeleteKey("SavedArrows");
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