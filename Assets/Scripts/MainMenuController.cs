using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    public Button continueButton;
    
    void Start()
    {
        // Gray out Continue if no save exists
        if (continueButton != null)
        {
            continueButton.interactable = SaveManager.Instance != null && SaveManager.Instance.HasSaveData();
        }
    }
    
    public void NewGame()
    {
        // Delete old save when starting new game
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.DeleteSave();
        }
        SceneManager.LoadScene("SampleScene");
    }
    
    public void ContinueGame()
    {
        // Load scene - RoomManager will read save data
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