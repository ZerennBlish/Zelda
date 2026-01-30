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
        // Delete room/lives save
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.DeleteSave();
        }
        
        // Delete inventory save
        PlayerPrefs.DeleteKey("SavedRupees");
        PlayerPrefs.DeleteKey("SavedArrows");
        PlayerPrefs.DeleteKey("SavedMaxHealth");
        PlayerPrefs.Save();
        
        SceneManager.LoadScene("SampleScene");
    }
    
    public void ContinueGame()
    {
        SceneManager.LoadScene("SampleScene");
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