using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public void NewGame()
    {
        SceneManager.LoadScene("SampleScene");
    }
    
    public void ContinueGame()
    {
        // We'll add save/load logic later
        // For now, just load the game
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